using System;
using System.Collections.Generic;
using System.Text;

namespace EthernetSwitch.Data.Models
{

    public class SNMPConfiguration
    {
        public string AgentAddresses { get; set; } = "udp:161";
        public string SysLocation { get; set; }
        public string SysContact { get; set; }
        public string TrapRecieverIpAddress { get; set; } = "127.0.0.1";
        public int TrapRecieverPort { get; set; } = 162;
    }
}
