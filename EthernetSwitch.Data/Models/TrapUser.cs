using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EthernetSwitch.Data.Models
{
    public enum EncryptionType
    {
        DES, AES
    }

    public class TrapUser
    {
        public TrapUser(string userName, int port, string password,
            string encryption, string engineId)
        {
            UserName = userName;
            Port = port;
            Password = password;
            Encryption = encryption;
            EngineId = engineId;
        }
        [Key] public long Id { get; set; }
        public string UserName { get; internal set; }
        public int Port { get; internal set; }
        public string Password { get; internal set; }
        public string Encryption { get; internal set; }
        public string EngineId { get; internal set; }
        public EncryptionType EncryptionType { get; set; }
    }
}
