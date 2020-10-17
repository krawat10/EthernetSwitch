using System.Collections.Generic;
using System.Text.Json.Serialization;

public class LLDPOutput {
    [JsonPropertyName ("lldp")]
    public LLDP LLDP { get; set; }
}

public class LLDP {
    [JsonPropertyName ("interface")]
    public LLDPInterfaces Interface { get; set; }
}

public class LLDPInterfaces {
    public IDictionary<string, LLDPInterface> Interfaces { get; set; }
}

public class LLDPInterface {
    [JsonPropertyName ("via")]
    public string Via { get; set; }

    [JsonPropertyName ("rid")]
    public string Rid { get; set; }

    [JsonPropertyName ("age")]
    public string Age { get; set; }

    [JsonPropertyName ("chassis")]
    public Chassis Chassis { get; set; }

    [JsonPropertyName ("port")]
    public Port Port { get; set; }
}

public class Chassis {
    [JsonPropertyName ("raspberrypi")]
    public Raspberrypi Raspberrypi { get; set; }
}

public class Raspberrypi {
    [JsonPropertyName ("id")]
    public Id Id { get; set; }

    [JsonPropertyName ("descr")]
    public string Descr { get; set; }

    [JsonPropertyName ("mgmt-ip")]
    public string MgmtIp { get; set; }

    [JsonPropertyName ("capability")]
    public Capability Capability { get; set; }
}

public class Capability {
    [JsonPropertyName ("type")]
    public string Type { get; set; }

    [JsonPropertyName ("enabled")]
    public bool Enabled { get; set; }
}

public class Id {
    [JsonPropertyName ("type")]
    public string Type { get; set; }

    [JsonPropertyName ("value")]
    public string Value { get; set; }
}

public class Port {
    [JsonPropertyName ("id")]
    public Id Id { get; set; }

    [JsonPropertyName ("descr")]
    public string Descr { get; set; }

    [JsonPropertyName ("ttl")]
    public string Ttl { get; set; }
}