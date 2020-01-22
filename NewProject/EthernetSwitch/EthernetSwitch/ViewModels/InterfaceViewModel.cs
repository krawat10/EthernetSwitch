using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace EthernetSwitch.ViewModels
{
    public enum InterfaceType {Off, Community, Isolated, Promiscuous }
    public class InterfaceViewModel
    {
        public Guid Guid { get; }
        public string Name { get; set; }
        public IEnumerable<string> VirtualLANs { get; set; } = new List<string>();
        public bool Tagged { get; set; } = false;
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
                switch (Status)
                {
                    case OperationalStatus.Up:
                        return "success";
                    case OperationalStatus.Down:
                        return "danger";
                    default: 
                        return "warning";
                };
            }
        }


        public IEnumerable<string> AllVirtualLANs { get; set; } = new List<string>();
        public IEnumerable<string> OtherVirtualLANs => AllVirtualLANs.Except(VirtualLANs);

        public bool IsHostInterface { get; set; }
    }
}