using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EthernetSwitch.BackgroundWorkers;
using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.Patterns;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using EthernetSwitch.Seciurity;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Samples.Pipeline;

namespace EthernetSwitch.Infrastructure.SNMP
{
    public class OID
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class SNMPServices :
        IQueryHandler<WalkV1Query, OID[]>,
        IQueryHandler<GetV3Query, OID>,
        ICommandHandler<SetV3Command>,
        ICommandHandler<InitializeTrapListenerV3Command>
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly EthernetSwitchContext context;

        public SNMPServices(IBackgroundTaskQueue taskQueue, EthernetSwitchContext context)
        {
            this.taskQueue = taskQueue;
            this.context = context;
        }
        
        public async Task Handle(InitializeTrapListenerV3Command query)
        {
            var users = new UserRegistry();
            users.Add(new OctetString("neither"), DefaultPrivacyProvider.DefaultPair);

            if (!string.IsNullOrWhiteSpace(query.UserName))
            {
                users.Add(
                new OctetString(query.UserName),
                new DESPrivacyProvider(
                    new OctetString(query.Encryption),
                    new MD5AuthenticationProvider(new OctetString(query.Password))));
            }
            taskQueue.QueueBackgroundWorkItem(async token =>
            {
                var trapv2 = new TrapV2MessageHandler();
                trapv2.MessageReceived += async (object sender, TrapV2MessageReceivedEventArgs e) =>
                {
                    Console.WriteLine("TRAP version {0}: {1}", e.TrapV2Message.Version, e.TrapV2Message);

                    var message = (new SNMPMessage
                    {
                        Type = SNMPMessageType.TRAP,
                        Version = (Data.Models.VersionCode)e.TrapV2Message.Version,
                        TimeStamp = e.TrapV2Message.TimeStamp,
                        ContextName = e.TrapV2Message.Scope.ContextName.ToString(),
                        MessageId = e.TrapV2Message.Header.MessageId,
                        Enterprise = e.TrapV2Message.Enterprise.ToString(),
                        UserName = e.TrapV2Message.Parameters.UserName.ToString()
                    });

                    foreach (var variable in e.TrapV2Message.Variables())
                    {
                        message.Variables.Add(new SNMPMessageVariable
                        {
                            VariableId = variable.Id.ToString(),
                            Value = variable.Data.ToString()
                        });
                    }

                    context.Add(message);
                    await context.SaveChangesAsync(token);
                };
                var trapv2Mapping = new HandlerMapping("v2,v3", "TRAPV2", trapv2);

                var inform = new InformRequestMessageHandler();
                inform.MessageReceived += async (object sender, InformRequestMessageReceivedEventArgs e) =>
                {
                    Console.WriteLine("Inform version {0}: {1}", e.InformRequestMessage.Version, e.InformRequestMessage);

                    var message = (new SNMPMessage
                    {
                        Type = SNMPMessageType.INFORM,
                        Version = (Data.Models.VersionCode)e.InformRequestMessage.Version,
                        TimeStamp = e.InformRequestMessage.TimeStamp,
                        ContextName = e.InformRequestMessage.Scope.ContextName.ToString(),
                        MessageId = e.InformRequestMessage.Header.MessageId,
                        Enterprise = e.InformRequestMessage.Enterprise.ToString(),
                        UserName = e.InformRequestMessage.Parameters.UserName.ToString()
                    });

                    foreach (var variable in e.InformRequestMessage.Variables())
                    {
                        message.Variables.Add(new SNMPMessageVariable
                        {
                            VariableId = variable.Id.ToString(),
                            Value = variable.Data.ToString()
                        });
                    }

                    context.Add(message);
                    await context.SaveChangesAsync(token);
                };

                var informMapping = new HandlerMapping("v2,v3", "INFORM", inform);
                var store = new ObjectStore();
                var membership = new ComposedMembershipProvider(new IMembershipProvider[] {
                    new Version1MembershipProvider(new OctetString("public"), new OctetString("public")),
                    new Version2MembershipProvider(new OctetString("public"), new OctetString("public")),
                    new Version3MembershipProvider()
                });
                var handlerFactory = new MessageHandlerFactory(new[] { trapv2Mapping, informMapping });
                var pipelineFactory = new SnmpApplicationFactory(store, membership, handlerFactory);

                using (var engine = new SnmpEngine(pipelineFactory, new Listener { Users = users }, new EngineGroup()))
                {
                    engine.Listener.AddBinding(new IPEndPoint(query.IpAddress, query.Port));

                    engine.Start();

                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(10000);
                    }
                    engine.Stop();
                }
            });
        }

        public async Task<OID[]> Handle(WalkV1Query query)
        {
            IList<Variable> result = new List<Variable>();

            await Messenger.WalkAsync(Lextm.SharpSnmpLib.VersionCode.V1,
                new IPEndPoint((query.IpAddress), query.Port),
                new OctetString(query.Group),
                query.StartObjectId,
                result,
                WalkMode.WithinSubtree);


            return result
                .Select(variable => new OID { Id = variable.Id.ToString(), Value = variable.Data.ToString() })
                .ToArray();
        }

        public async Task<OID> Handle(GetV3Query query)
        {
            var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            var report = discovery.GetResponse(10000, new IPEndPoint(query.IpAddress, query.Port));
               var isSupported1 = AESPrivacyProvider.IsSupported;
               var isSupported2 = DESPrivacyProvider.IsSupported;

            var request = new GetRequestMessage(
                Lextm.SharpSnmpLib.VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(query.UserName),
                new List<Variable> { new Variable(new ObjectIdentifier(query.OID_Id)) },
                new BouncyCastleDESPrivacyProvider(
                    new OctetString(query.Encryption),
                    new MD5AuthenticationProvider(new OctetString(query.Password))
                ),
                Messenger.MaxMessageSize,
                report);

            var reply = await request.GetResponseAsync(new IPEndPoint(query.IpAddress, query.Port));

            var oid = reply
                .Pdu().Variables
                .FirstOrDefault(variable => variable.Id.ToString() == query.OID_Id);

            if (oid == null)
            {
                throw new KeyNotFoundException($"Cannot find variable with ID {query.OID_Id}");
            }

            if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create("error in response", query.IpAddress, reply);
            }

            return new OID { Id = oid.Id.ToString(), Value = oid.Data.ToString() };
        }

        public async Task Handle(SetV3Command command)
        {
            Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            ReportMessage report = discovery.GetResponse(10000, new IPEndPoint(command.IpAddress, command.Port));

            SetRequestMessage request = new SetRequestMessage(
                Lextm.SharpSnmpLib.VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(command.UserName),
                new List<Variable> { new Variable(new ObjectIdentifier(command.OID.Id)) },
                new BouncyCastleDESPrivacyProvider(
                    new OctetString(command.Encryption),
                    new MD5AuthenticationProvider(new OctetString(command.Password))
                ),
                report);

            ISnmpMessage reply = await request.GetResponseAsync(new IPEndPoint(command.IpAddress, command.Port));

            if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create("error in response", command.IpAddress, reply);
            }

        }
    }
}