using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using Lextm.SharpSnmpLib;
using Microsoft.EntityFrameworkCore;

namespace EthernetSwitch.Models.SNMP
{
    public class TrapSNMPv3ViewModel: BaseViewModel
    {
        public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        public string IpAddress { get; set; } = "";
        public string EngineId { get; set; }
        public int Port { get; set; } = 162;
        [DataType(DataType.Password)] public string Password { get; set; }
        [DataType(DataType.Password)] public string Encryption { get; set; }
        public ICollection<Data.Models.SNMPMessage> Messages { get; set; }
        public EncryptionType EncryptionType { get; set; }

    }
}