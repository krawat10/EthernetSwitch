using System.Collections.Generic;

namespace EthernetSwitch.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<InterfaceViewModel> Interfaces { get; set; } = new List<InterfaceViewModel>();
    }
}