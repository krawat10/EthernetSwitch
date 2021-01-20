using EthernetSwitch.Models;
using System.Collections.Generic;

namespace EthernetSwitch.ViewModels
{
    public class IndexViewModel: BaseViewModel
    {
        public IList<BridgeViewModel> VLANs;
        public IEnumerable<InterfaceViewModel> Interfaces { get; set; } = new List<InterfaceViewModel>();
    }
}