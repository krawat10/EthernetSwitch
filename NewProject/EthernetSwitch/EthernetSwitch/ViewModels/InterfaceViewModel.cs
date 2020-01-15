using System.Collections.Generic;

namespace EthernetSwitch.ViewModels
{
    public class InterfaceViewModel
    {
        public string Name { get; set; }
        public IList<VirtualLANViewModel> VirtualLANs { get; set; } = new List<VirtualLANViewModel>();
        public bool Tagged { get; set; } = false;
        public string Status { get; set; }
        public bool Hidden { get; set; } = false;

        public string StatusClass =>
            Status.ToLower() switch
            {
                "up" => "success",
                "down" => "danger",
                _ => "warning"
            };
    }
}