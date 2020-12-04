using System.Collections.Generic;
using EthernetSwitch.Infrastructure.SNMP;
using Lextm.SharpSnmpLib;
using ServiceStack.DataAnnotations;

namespace EthernetSwitch.Models.SNMP
{
    public class WalkSNMPv1ViewModel : BaseViewModel
    {
        [Required] public string Group { get; set; } = "public";
        public VersionCode VersionCode { get; set; } = VersionCode.V1;
        [Required] public string StartObjectId { get; set; } = "1.3.6.1.2.1.1";
        [Required] public string IpAddress { get; set; } = "127.0.0.1";
        [Required] public int Port { get; set; } = 161;
        [Required] public OID[] OIDs { get; set; } = new OID[0];
    }
}