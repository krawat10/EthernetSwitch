using System.Collections.Generic;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Models.SNMP
{
    public class WalkSNMPv1ViewModel
    {
        public string Group { get; set; }
        public VersionCode VersionCode { get; set; }
        public string StartObjectId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public IEnumerable<OID> OIDs { get; set; } = new List<OID>();
    }
}