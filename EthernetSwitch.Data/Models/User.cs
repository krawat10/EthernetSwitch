using System.ComponentModel.DataAnnotations;

namespace EthernetSwitch.Data.Models
{
    public enum UserRole
    {
        Admin,
        User,
        NotConfirmed
    }

    public class User
    {
        [Key] public long Id { get; set; }

        public UserRole Role { get; set; }
        public string UserName { get; set; }
        public string PasswordEncrypted { get; set; }
    }
}