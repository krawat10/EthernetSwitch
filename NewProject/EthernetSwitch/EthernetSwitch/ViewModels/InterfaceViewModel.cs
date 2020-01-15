using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace EthernetSwitch.ViewModels
{
    public class InterfaceViewModel
    {
        public string Name { get; set; }
        public IList<VirtualLanViewModel> VirtualLans { get; set; } = new List<VirtualLanViewModel>();
        public bool Tagged { get; set; } = false;
        public string Status { get; set; } 
        public bool Hidden { get; set; } = false;
    }

    public class VirtualLanViewModel
    {
        public int Name { get; set; }
        public string Status { get; set; }
    }
}