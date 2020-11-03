using System.Net;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Infrastructure.SNMP.Commands
{
    public enum EncryptionType
    {
        DES, AES
    }

    public class InitializeTrapListenerV3Command
    {
        public InitializeTrapListenerV3Command(string userName, IPAddress ipAddress, int port, string password,
            string encryption, string engineId)
        {
            UserName = userName;
            IpAddress = ipAddress;
            Port = port;
            Password = password;
            Encryption = encryption;
            EngineId = engineId;
        }

        public string UserName { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public int Port { get; internal set; }
        public string Password { get; internal set; }
        public string Encryption { get; internal set; }
        public string EngineId { get; internal set; }
        public EncryptionType EncryptionType { get; set; }
    }
}