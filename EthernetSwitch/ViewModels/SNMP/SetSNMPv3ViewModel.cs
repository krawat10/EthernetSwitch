using System;
using System.ComponentModel.DataAnnotations;
using EthernetSwitch.Data.Models;
using EthernetSwitch.Infrastructure.SNMP;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EthernetSwitch.Models.SNMP
{
    public class SetSNMPv3ViewModel : BaseViewModel
    {
        [Required]
        public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        [Required]
        public string IpAddress { get; set; } = "127.0.0.1";
        [Required]
        public int Port { get; set; } = 161;
        [DataType(DataType.Password)][Required] public string Password { get; set; }
        [DataType(DataType.Password)][Required] public string Encryption { get; set; }
        [Required] public EncryptionType EncryptionType { get; set; }
        [Required]public OID OID { get; set; }

    }
}