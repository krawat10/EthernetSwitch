using System.Collections.Generic;

namespace EthernetSwitch.ViewModels
{
    public class SettingsViewModel
    {
        public bool AllowTagging { get; set; }
        public bool AllowRegistration { get; set; }

        public IEnumerable<string> NotConfirmedUsers { get; set; }

        public IEnumerable<string> ConfirmedUsers { get; set; }
    }
}