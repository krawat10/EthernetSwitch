using System.ComponentModel.DataAnnotations;

namespace EthernetSwitch.Data.Models
{
    public class SNMPUser
    {
        [Key] public long Id { get; set; }
        public string UserName { get; internal set; }
        [DataType(DataType.Password)] public string Password { get; internal set; }
        [DataType(DataType.Password)] public string Encryption { get; internal set; }
        public EncryptionType EncryptionType { get; set; }
    }
}
