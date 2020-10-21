using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EthernetSwitch.BackgroundWorkers;
using EthernetSwitch.Infrastructure.Patterns;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using EthernetSwitch.Infrastructure.SNMP.Queries;
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

        public SNMPServices(IBackgroundTaskQueue taskQueue)
        {
            this.taskQueue = taskQueue;
        }

        public async Task Handle(InitializeTrapListenerV3Command query)
        {
            query.IpAddress ??= IPAddress.Any;

            var users = new UserRegistry();
            users.Add(new OctetString("neither"), DefaultPrivacyProvider.DefaultPair);
            
            users.Add(
                new OctetString(query.UserName),
                new DESPrivacyProvider(
                    new OctetString(query.Encryption),
                    new MD5AuthenticationProvider(new OctetString(query.Password))));

            taskQueue.QueueBackgroundWorkItem(async token =>
            {
                var trapv2 = new TrapV2MessageHandler();
                trapv2.MessageReceived += (object sender, TrapV2MessageReceivedEventArgs e) =>
                {
                    Console.WriteLine("TRAP version {0}: {1}", e.TrapV2Message.Version, e.TrapV2Message);
                    foreach (var variable in e.TrapV2Message.Variables())
                    {
                        Console.WriteLine(variable);
                    }
                };
                var trapv2Mapping = new HandlerMapping("v2,v3", "TRAPV2", trapv2);

                var inform = new InformRequestMessageHandler();
                inform.MessageReceived += (object sender, InformRequestMessageReceivedEventArgs e) =>
                {
                    Console.WriteLine("INFORM version {0}: {1}", e.InformRequestMessage.Version, e.InformRequestMessage);
                    foreach (var variable in e.InformRequestMessage.Variables())
                    {
                        Console.WriteLine(variable);
                    }
                };

                var informMapping = new HandlerMapping("v2,v3", "INFORM", inform);
                var store = new ObjectStore();
                var v1 = new Version1MembershipProvider(new OctetString("public"), new OctetString("public"));
                var v2 = new Version2MembershipProvider(new OctetString("public"), new OctetString("public"));
                var v3 = new Version3MembershipProvider();
                var membership = new ComposedMembershipProvider(new IMembershipProvider[] { v1, v2, v3 });
                var handlerFactory = new MessageHandlerFactory(new[] { trapv2Mapping, informMapping });

                var pipelineFactory = new SnmpApplicationFactory(store, membership, handlerFactory);

                using var engine = new SnmpEngine(pipelineFactory, new Listener { Users = users }, new EngineGroup());
                engine.Listener.AddBinding(new IPEndPoint(IPAddress.Any, query.Port));
                engine.Start();
            });
            
            //engine.Stop();
        }

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
                .Select(variable => new OID { Id = variable.Id.ToString(), Value = variable.Data.ToString() })
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
                new List<Variable> { new Variable(new ObjectIdentifier(query.OID_Id)) },
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

            return new OID { Id = oid.Id.ToString(), Value = oid.Data.ToString() };
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