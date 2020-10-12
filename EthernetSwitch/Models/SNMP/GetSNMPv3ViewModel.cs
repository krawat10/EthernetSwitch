﻿using System.ComponentModel.DataAnnotations;
using Lextm.SharpSnmpLib;

namespace EthernetSwitch.Models.SNMP
{
    public class OID
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }


    public class GetSNMPv3ViewModel
    {
        public string UserName { get; set; }
        public VersionCode VersionCode { get; set; } = VersionCode.V3;
        public string IpAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 161;
        [DataType(DataType.Password)]public string Password { get; set; }
        [DataType(DataType.Password)]public string Encryption { get; set; }
        public OID OID { get; set; }
        public string Error { get;  set; }
    }
}