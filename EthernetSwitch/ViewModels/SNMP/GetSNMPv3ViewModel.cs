using System.ComponentModel.DataAnnotations;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.SNMP;

namespace EthernetSwitch.Models.SNMP
{
    public class GetSNMPv3ViewModel : BaseViewModel
    {
        [Required] public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        [Required] public string IpAddress { get; set; } = "127.0.0.1";
        [Required] public int Port { get; set; } = 161;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Encryption { get; set; }

        [Required] public OID OID { get; set; }
        public EncryptionType EncryptionType { get; set; }
        [Required] public PasswordType PasswordType { get; set; }
    }
}