using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EthernetSwitch.Data.Models
{
    public class SNMPMessageVariable
    {
        [Key] public long Id { get; set; }
        public long TrapMessageId { get; set; }
        [ForeignKey(nameof(TrapMessageId))]public SNMPMessage TrapMessage { get; set; }
        public string VariableId { get; set; }
        public string Value { get; set; }
    }

    public enum VersionCode
    {
        V1 = 0,
        V2 = 1,
        V2U = 2,
        V3 = 3
    }

    public enum SNMPMessageType
    {
        SET, GET, WALK, TRAP, INFORM
    }

    public class SNMPMessage
    {
        public SNMPMessage()
        {
            Variables = new List<SNMPMessageVariable>();
        }
        [Key] public long Id { get; set; }

        public ICollection<SNMPMessageVariable> Variables { get; set; }
        public SNMPMessageType Type { get; set; }
        public VersionCode Version { get; set; }
        public uint TimeStamp { get; set; }
        public string ContextName { get; set; }
        public int MessageId { get; set; }
        public string Enterprise { get; set; }
        public string UserName { get; set; }
    }
}