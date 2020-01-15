using System.Collections.Generic;

namespace EthernetSwitch.ViewModels
{
    public class InterfaceViewModel
    {
        public string Name { get; set; }
        public IList<VirtualLanViewModel> VirtualLans { get; set; }
        public bool Tagged { get; set; } = false;
        public bool Enabled { get; set; } 
        public bool Hidden { get; set; } = false;
    }

    public class VirtualLanViewModel
    {
        public int Name { get; set; }
        public bool Enabled { get; set; }
    }
}