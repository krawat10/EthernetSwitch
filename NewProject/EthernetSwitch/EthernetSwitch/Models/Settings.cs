using System.Collections.Generic;

namespace EthernetSwitch.Models
{
    public class Settings
    {
        public bool AllowAnonymousView { get; set; }
        public bool AllowTagging { get; set; }
        public IEnumerable<User> Users { get; set; }
    }

    public class User
    {
        public bool IsAdmin { get; set; }
        public string UserName { get; set; }
        public bool CanEdit { get; set; }
        public string PasswordEncrypted { get; set; }
    }
}