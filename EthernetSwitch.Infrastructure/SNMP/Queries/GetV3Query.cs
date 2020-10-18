using System.Net;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Infrastructure.SNMP.Queries
{
    public class GetV3Query
    {
        public GetV3Query(string userName, VersionCode versionCode, IPAddress ipAddress, int port, string password,
            string encryption, string oidId)
        {
            UserName = userName;
            VersionCode = versionCode;
            IpAddress = ipAddress;
            Port = port;
            Password = password;
            Encryption = encryption;
            OID_Id = oidId;
        }

        public string UserName { get; internal set; }
        public VersionCode VersionCode { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public int Port { get; internal set; }
        public string Password { get; internal set; }
        public string Encryption { get; internal set; }
        public string OID_Id { get; internal set; }
    }
}