using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EthernetSwitch.BackgroundWorkers;
using EthernetSwitch.Data;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Bash.Exceptions;
using EthernetSwitch.Infrastructure.Extensions;
using EthernetSwitch.Infrastructure.Patterns;
using EthernetSwitch.Infrastructure.Settings;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samples.Pipeline;
using ServiceStack;
using DESPrivacyProvider = EthernetSwitch.Security.DESPrivacyProvider;

namespace EthernetSwitch.Infrastructure.SNMP
{
    public class OID
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class SNMPServices :
        IQueryHandler<WalkQuery, OID[]>,
        IQueryHandler<GetV3Query, OID>,
        ICommandHandler<SetV3Command>
    {
        private readonly ILogger<SNMPServices> logger;
        private readonly IBashCommand bash;
        private readonly ISettingsRepository settingsRepository;
        public SNMPServices(ILoggerFactory loggerFactory, IBashCommand bash, ISettingsRepository settingsRepository)
        {
            this.logger = loggerFactory.CreateLogger<SNMPServices>();
            this.bash = bash;
            this.settingsRepository = settingsRepository;
        }

        public async Task<string> Handle(SNMPUser user)
        {
            try
            {
                bash.Execute("/etc/init.d/snmpd stop");
                bash.Execute($"net-snmp-config --create-snmpv3-user -A {user.Password} -X {user.Encryption} -a MD5 -x {user.EncryptionType} {user.UserName}");
                bash.Execute("/etc/init.d/snmpd start");
            }
            catch (ProcessException ex)
            {
                return ex.Message;
            }

            return null;
        }

        public async Task<string[]> Handle(GetSNMPUsers query)
        {
            var regex = new Regex(@"usmUser \S+ \S+ \S+ \""(\S+)\""");
            var configPath = "/var/lib/snmp/snmpd.conf";

            if (!File.Exists(configPath)) throw new FileNotFoundException("Configuration File doesn't exists", "/var/lib/snmp/snmpd.conf");

            var varSnmpd = await File.ReadAllLinesAsync(configPath);

            var users = varSnmpd
                .Where(line => regex.IsMatch(line))
                .Select(line => regex.Match(line).Groups[1].Value)
                .ToArray();

            return users;
        }


        public async Task<string> Handle(SNMPConfiguration configuration)
        {
            var settings = await settingsRepository.GetSettings();
            settings.SNMPConfiguration = configuration;


            bash.Install("snmpd");
            bash.Install("snmp");
            bash.Install("libsnmp-dev");

            var snmpd = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "snmpd.conf"));
            snmpd = snmpd.FormatWith(configuration);

            var snmpdPath = "/etc/snmp/snmpd.conf";

            bash.Execute("/etc/init.d/snmpd stop");

            if (File.Exists(snmpdPath))
                File.Copy(snmpdPath, $"/etc/snmp/snmpd_{DateTime.Now.Ticks}.bak");

            using StreamWriter stream = File.CreateText(snmpdPath);
            await stream.WriteAsync(snmpd);

            bash.Execute("/etc/init.d/snmpd start");

            await settingsRepository.SaveSettings(settings);

            return null;
        }

        public async Task<OID[]> Handle(WalkQuery query)
        {
            IList<Variable> result = new List<Variable>();

            await Messenger.WalkAsync(Lextm.SharpSnmpLib.VersionCode.V1,
                new IPEndPoint(query.IpAddress, query.Port),
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

            IPrivacyProvider provider;
            if (query.EncryptionType == EncryptionType.DES)
            {
                provider = new DESPrivacyProvider(
                    new OctetString(query.Encryption),
                    new MD5AuthenticationProvider(new OctetString(query.Password)));
            }
            else
            {
                provider = new AESPrivacyProvider(
                    new OctetString(query.Encryption),
                    new MD5AuthenticationProvider(new OctetString(query.Password)));
            }

            var request = new GetRequestMessage(
                Lextm.SharpSnmpLib.VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(query.UserName),
                new List<Variable> { new Variable(new ObjectIdentifier(query.OID_Id)) },
                provider,
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

            IPrivacyProvider provider;
            if (command.EncryptionType == EncryptionType.DES)
            {
                provider = new DESPrivacyProvider(
                    new OctetString(command.Encryption),
                    new MD5AuthenticationProvider(new OctetString(command.Password)));
            }
            else
            {
                provider = new AESPrivacyProvider(
                    new OctetString(command.Encryption),
                    new MD5AuthenticationProvider(new OctetString(command.Password)));
            }

            SetRequestMessage request = new SetRequestMessage(
                Lextm.SharpSnmpLib.VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(command.UserName),
                new List<Variable> { new Variable(new ObjectIdentifier(command.OID.Id), new OctetString(command.OID.Value)) },
                provider,
                report);

            ISnmpMessage reply = await request.GetResponseAsync(new IPEndPoint(command.IpAddress, command.Port));

            if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create("error in response", command.IpAddress, reply);
            }

        }
    }
}