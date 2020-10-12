using System.Net;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Infrastructure.SNMP.Queries
{
    public class WalkV1Query
    {
        public WalkV1Query(string @group, VersionCode versionCode, ObjectIdentifier startObjectId, IPAddress ipAddress,
            int port)
        {
            Group = @group;
            VersionCode = versionCode;
            StartObjectId = startObjectId;
            IpAddress = ipAddress;
            Port = port;
        }

        public string Group { get; internal set; }
        public VersionCode VersionCode { get; internal set; }
        public ObjectIdentifier StartObjectId { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public int Port { get; internal set; }
    }
}