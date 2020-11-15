using System.ComponentModel.DataAnnotations;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.SNMP;

namespace EthernetSwitch.Models.SNMP
{
    public class SetSNMPv3ViewModel
    {
        public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        public string IpAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 161;
        [DataType(DataType.Password)] public string Password { get; set; }
        [DataType(DataType.Password)] public string Encryption { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public OID OID { get; set; }
        public string Error { get; set; }
    }
}