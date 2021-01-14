using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using EthernetSwitch.Infrastructure.Ethernet;

namespace EthernetSwitch.ViewModels
{
    public class InterfaceViewModel
    {
        public Guid Guid { get; }
        public string Name { get; set; }
        public IEnumerable<string> VirtualLANs { get; set; } = new List<string>();
        public bool Tagged { get; set; } = false;
        public bool IsGVRP { get; set; } = false;
        public OperationalStatus Status { get; set; }
        public bool IsActive { get; set; }
        public bool Hidden { get; set; } = false;
        public InterfaceType Type { get; set; } = InterfaceType.Off;
        public InterfaceViewModel()
        {
            Guid = Guid.NewGuid();
        }

        public bool CanAddVlan
        {
            get
            {
                if (Tagged) return true;
                else return !VirtualLANs.Any();
            }
        }

        public string StatusClass
        {
            get
            {
                return Status switch
                {
                    OperationalStatus.Up => "success",
                    OperationalStatus.Down => "danger",
                    _ => "warning"
                };
                ;
            }
        }


        public IEnumerable<string> AllVirtualLANs { get; set; } = new List<string>();
        public IEnumerable<string> OtherVirtualLANs => AllVirtualLANs.Except(VirtualLANs);

        public bool IsHostInterface { get; set; }
        public bool AllowTagging { get; set; }
        public EthernetNeighbor Neighbor { get; internal set; }
    }
}