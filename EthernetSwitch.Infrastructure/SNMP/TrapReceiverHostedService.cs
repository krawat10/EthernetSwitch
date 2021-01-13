using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EthernetSwitch.Data.Models;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Pipeline;
using DESPrivacyProvider = EthernetSwitch.Security.DESPrivacyProvider;
using VersionCode = EthernetSwitch.Data.Models.VersionCode;

namespace EthernetSwitch.Infrastructure.SNMP
{
    public class TrapReceiverHostedService : BackgroundService
    {
        private readonly ILogger<TrapReceiverHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ICollection<SNMPTrapUser> _activeTrapUsers;
        private SnmpEngine _engine;


        public TrapReceiverHostedService(ILogger<TrapReceiverHostedService> logger, 
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _activeTrapUsers = new List<SNMPTrapUser>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"Queued Hosted Service is running");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<ITrapUsersRepository>();

                    if (await repository.HasNewUsers(_activeTrapUsers, stoppingToken))
                    {
                        _activeTrapUsers = await repository.GetUsers();
                        var ports = _activeTrapUsers.Select(usr => usr.Port).Distinct();
                        var users = new UserRegistry();
                        users.Add(new OctetString("neither"), DefaultPrivacyProvider.DefaultPair);

                        foreach (var trapUser in _activeTrapUsers)
                        {
                            IPrivacyProvider provider;
                            if (trapUser.EncryptionType == EncryptionType.DES)
                            {
                                provider = new DESPrivacyProvider(
                                    new OctetString(trapUser.Encryption),
                                    new MD5AuthenticationProvider(new OctetString(trapUser.Password)))
                                {
                                    EngineIds = new List<OctetString> {new OctetString(ByteTool.Convert(trapUser.EngineId))}
                                };
                            }
                            else
                            {
                                provider = new AESPrivacyProvider(
                                    new OctetString(trapUser.Encryption),
                                    new MD5AuthenticationProvider(new OctetString(trapUser.Password)))
                                {
                                    EngineIds = new List<OctetString> {new OctetString(ByteTool.Convert(trapUser.EngineId))}
                                };
                            }

                            users.Add(new OctetString(trapUser.UserName), provider);
                        }

                        var trap = new TrapV2MessageHandler();
                        trap.MessageReceived += TrapMessageReceived;
                        var trapv2Mapping = new HandlerMapping("v2,v3", "TRAPV2", trap);

                        //snmptrap -v3 -e 0x090807060504030201 -l authPriv -u snmpro -a MD5 -A STrP@SSWRD -x DES -X STr0ngP@SSWRD 192.168.0.10:162 ''  1.3.6.1.4.1.8072.2.3.0.1 1.3.6.1.4.1.8072.2.3.2.1 i 60
                        var inform = new InformRequestMessageHandler();
                        inform.MessageReceived += InformMessageReceived;
                        var informMapping = new HandlerMapping("v2,v3", "INFORM", inform);

                        var membership = new ComposedMembershipProvider(new IMembershipProvider[]
                        {
                            new Version1MembershipProvider(new OctetString("public"), new OctetString("public")),
                            new Version2MembershipProvider(new OctetString("public"), new OctetString("public")),
                            new Version3MembershipProvider()
                        });

                        var handlerFactory = new MessageHandlerFactory(new[] {trapv2Mapping, informMapping});
                        var pipelineFactory = new SnmpApplicationFactory(new ObjectStore(), membership, handlerFactory);

                        if (_engine?.Active ?? false)
                        {
                            _engine.Stop();
                            _engine.Dispose();
                        }

                        _engine = new SnmpEngine(pipelineFactory, new Listener {Users = users}, new EngineGroup());

                        foreach (var port in ports)
                        {
                            _engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, port));
                        }

                        _engine.Start();
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async void TrapMessageReceived(object sender, TrapV2MessageReceivedEventArgs e)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<ISNMPMessageStore>();

                _logger.LogInformation("TRAP version {0}: {1}", e.TrapV2Message.Version, e.TrapV2Message);

                var message = new SNMPMessage
                {
                    Type = SNMPMessageType.TRAP,
                    Version = (VersionCode) e.TrapV2Message.Version,
                    TimeStamp = e.TrapV2Message.TimeStamp,
                    ContextName = e.TrapV2Message.Scope?.ContextName?.ToString() ?? "",
                    MessageId = e.TrapV2Message.Header?.MessageId ?? 0,
                    Enterprise = e.TrapV2Message.Enterprise?.ToString() ?? "",
                    UserName = e.TrapV2Message.Parameters?.UserName?.ToString() ?? ""
                };

                foreach (var variable in e.TrapV2Message.Variables())
                {
                    message.Variables.Add(new SNMPMessageVariable
                    {
                        VariableId = variable.Id?.ToString() ?? "",
                        Value = variable.Data?.ToString() ?? ""
                    });
                }

                await store.Add(message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        private async void InformMessageReceived(object sender, InformRequestMessageReceivedEventArgs e)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<ISNMPMessageStore>();

                _logger.LogWarning("Inform version {0}: {1}", e.InformRequestMessage.Version, e.InformRequestMessage);

                var message = new SNMPMessage
                {
                    Type = SNMPMessageType.INFORM,
                    Version = (VersionCode) e.InformRequestMessage.Version,
                    TimeStamp = e.InformRequestMessage.TimeStamp,
                    ContextName = e.InformRequestMessage.Scope?.ContextName?.ToString() ?? "",
                    MessageId = e.InformRequestMessage.Header?.MessageId ?? 0,
                    Enterprise = e.InformRequestMessage.Enterprise?.ToString() ?? "",
                    UserName = e.InformRequestMessage.Parameters?.UserName?.ToString() ?? ""
                };

                foreach (var variable in e.InformRequestMessage.Variables())
                {
                    message.Variables.Add(new SNMPMessageVariable
                    {
                        VariableId = variable.Id?.ToString() ?? "",
                        Value = variable.Data?.ToString() ?? ""
                    });
                }

                await store.Add(message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}