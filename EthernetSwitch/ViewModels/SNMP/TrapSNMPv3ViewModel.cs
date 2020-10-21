using System.ComponentModel.DataAnnotations;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Models.SNMP
{
    public class TrapSNMPv3ViewModel: BaseViewModel
    {
        public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        public string IpAddress { get; set; }
        public int Port { get; set; } = 162;
        [DataType(DataType.Password)] public string Password { get; set; }
        [DataType(DataType.Password)] public string Encryption { get; set; }


    }
}