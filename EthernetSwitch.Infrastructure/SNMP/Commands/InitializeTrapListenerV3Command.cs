using System.Net;

namespace EthernetSwitch.Infrastructure.SNMP.Commands
{
    public class InitializeTrapListenerV3Command
    {
        public InitializeTrapListenerV3Command(string userName, IPAddress ipAddress, int port, string password,
            string encryption)
        {
            UserName = userName;
            IpAddress = ipAddress;
            Port = port;
            Password = password;
            Encryption = encryption;
        }

        public string UserName { get; internal set; }
        public IPAddress IpAddress { get; internal set; }
        public int Port { get; internal set; }
        public string Password { get; internal set; }
        public string Encryption { get; internal set; }
    }
}