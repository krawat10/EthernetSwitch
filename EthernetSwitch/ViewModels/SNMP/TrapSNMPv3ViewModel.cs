using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.SNMP.Commands;
using Lextm.SharpSnmpLib;
using Microsoft.EntityFrameworkCore;
using VersionCode = EthernetSwitch.Data.Models.VersionCode;

namespace EthernetSwitch.Models.SNMP
{
    public class TrapSNMPv3ViewModel : BaseViewModel
    {
        [Required] public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        [Required] public string EngineId { get; set; } = "0x090807060504030201";
        [Required] public int Port { get; set; } = 162;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Encryption { get; set; }

        public EncryptionType EncryptionType { get; set; }
    }
}