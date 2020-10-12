using System.Net;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Infrastructure.SNMP.Commands
{
    public class SetV3Command
    {
        public SetV3Command(string userName, VersionCode versionCode, IPAddress ipAddress, int port, string password,
            string encryption, OID oid)
        {
            UserName = userName;
            VersionCode = versionCode;
            IpAddress = ipAddress;
            Port = port;
            Password = password;
            Encryption = encryption;
            OID = oid;
        }

        public string UserName { get; internal set; }
        public VersionCode VersionCode { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public int Port { get; internal set; }
        public string Password { get; internal set; }
        public string Encryption { get; internal set; }
        public OID OID { get; internal set; }
    }
}