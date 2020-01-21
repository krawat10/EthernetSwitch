using System.Collections.Generic;

namespace EthernetSwitch.Models
{
    public class Settings
    {
        public bool AllowTagging { get; set; }
        public IEnumerable<User> Users { get; set; }
    }

    public enum UserRole
    {
        Admin, User, NotConfirmed
    }

    public class User
    {
        public UserRole Role { get; set; }
        public string UserName { get; set; }
        public string PasswordEncrypted { get; set; }
    }
}