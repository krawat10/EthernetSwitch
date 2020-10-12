using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Patterns;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using EthernetSwitch.Infrastructure.SNMP.Queries;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;

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
        ICommandHandler<SetV3Command>
    {
        public async Task<OID[]> Handle(WalkV1Query query)
        {
            IList<Variable> result = new List<Variable>();

            await Messenger.WalkAsync(VersionCode.V1,
                new IPEndPoint((query.IpAddress), query.Port),
                new OctetString(query.Group),
                query.StartObjectId,
                result,
                WalkMode.WithinSubtree);


            return result
                .Select(variable => new OID {Id = variable.Id.ToString(), Value = variable.Data.ToString()})
                .ToArray();
        }

        public async Task<OID> Handle(GetV3Query query)
        {
            var discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            var report = discovery.GetResponse(10000, new IPEndPoint(query.IpAddress, query.Port));

            var request = new GetRequestMessage(
                VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(query.UserName),
                new List<Variable> {new Variable(new ObjectIdentifier(query.OID_Id))},
                new DESPrivacyProvider(
                    new OctetString(query.Encryption),
                    new MD5AuthenticationProvider(new OctetString(query.Password))
                ),
                Messenger.MaxMessageSize,
                report);

            var reply = await request.GetResponseAsync(new IPEndPoint(query.IpAddress, query.Port));

            var oid = reply
                .Pdu().Variables
                .FirstOrDefault(variable => variable.Id.ToString() == query.OID_Id);

            if (oid != null)
            {
                throw new KeyNotFoundException($"Cannot find variable with ID {query.OID_Id}");
            }

            if (reply.Pdu().ErrorStatus.ToInt32() != 0) // != ErrorCode.NoError
            {
                throw ErrorException.Create("error in response", query.IpAddress, reply);
            }

            return new OID {Id = oid.Id.ToString(), Value = oid.Data.ToString()};
        }

        public async Task Handle(SetV3Command command)
        {
            Discovery discovery = Messenger.GetNextDiscovery(SnmpType.GetRequestPdu);
            ReportMessage report = discovery.GetResponse(10000, new IPEndPoint(command.IpAddress, command.Port));

            SetRequestMessage request = new SetRequestMessage(
                VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                new OctetString(command.UserName),
                new List<Variable> { new Variable(new ObjectIdentifier(command.OID.Id)) },
                new DESPrivacyProvider(
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