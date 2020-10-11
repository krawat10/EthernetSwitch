using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Models.SNMP
{
    public class OID
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class GetSNMPv3ViewModel
    {
        public string UserName { get; set; }
        public VersionCode VersionCode { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string Encryption { get; set; }
        public OID OID { get; set; }
    }
}