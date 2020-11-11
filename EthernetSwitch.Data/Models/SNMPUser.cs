using System.ComponentModel.DataAnnotations;

namespace EthernetSwitch.Data.Models
{
    public class SNMPUser
    {
        [Key] public long Id { get; set; }
        public string UserName { get; set; }
        [DataType(DataType.Password)] public string Password { get; set; }
        [DataType(DataType.Password)] public string Encryption { get; set; }
        public EncryptionType EncryptionType { get; set; }
    }
}
