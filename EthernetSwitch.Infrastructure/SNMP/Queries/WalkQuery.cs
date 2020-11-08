using System.Net;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Infrastructure.SNMP.Queries
{
    public class WalkQuery
    {
        public WalkQuery(string @group, string startObjectId, IPAddress ipAddress,
            int port, VersionCode versionCode = VersionCode.V1)
        {
            Group = @group;
            VersionCode = versionCode;
            StartObjectId = new ObjectIdentifier(startObjectId);
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