using System.Text.Json.Serialization;


namespace EthernetSwitch.Infrastructure.LLDP
{
    public partial class LldpOutput
    {
        [JsonPropertyName("lldp")]
        public Lldp[] Lldp { get; set; }
    }

    public partial class Lldp
    {
        [JsonPropertyName("interface")]
        public Interface[] Interface { get; set; }
    }

    public partial class Interface
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("via")]
        public string Via { get; set; }

        [JsonPropertyName("rid")]
        public string Rid { get; set; }

        [JsonPropertyName("age")]
        public string Age { get; set; }

        [JsonPropertyName("chassis")]
        public Chassis[] Chassis { get; set; }

        [JsonPropertyName("port")]
        public Port[] Port { get; set; }
    }

    public partial class Chassis
    {
        [JsonPropertyName("id")]
        public Variable[] Id { get; set; }

        [JsonPropertyName("name")]
        public Variable[] Name { get; set; }

        [JsonPropertyName("descr")]
        public Variable[] Descr { get; set; }

        [JsonPropertyName("mgmt-ip")]
        public Variable[] MgmtIp { get; set; }

        [JsonPropertyName("capability")]
        public Variable[] Capability { get; set; }
    }

    public partial class Port
    {
        [JsonPropertyName("id")]
        public Variable[] Id { get; set; }

        [JsonPropertyName("descr")]
        public Variable[] Descr { get; set; }

        [JsonPropertyName("ttl")]
        public Variable[] Ttl { get; set; }
    }

    public partial class Variable
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

    }

}
