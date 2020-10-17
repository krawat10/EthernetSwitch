using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using EthernetSwitch.Infrastructure.Bash;


class EthernetNeighbor
{
    public string EthernetInterfaceName { get; set; }
    public string Updated { get; set; }
    public string MAC { get; set; }
    public string SystemName { get; set; }
    public string SystemDescription { get; set; }
    public IPAddress IPAddress { get; set; }
}

class LLDPServices
{
    private readonly IBashCommand _bash;

    public LLDPServices(IBashCommand bash)
    {
        this._bash = bash;
    }
    public IEnumerable<LLDPInterface> GetNeighbours()
    {
        var output = _bash.Execute("lldpcli show neighbors");
        var lldpOutput = JsonSerializer.Deserialize<LLDPOutput>(output);

        return lldpOutput.LLDP.Interface.Interfaces.To       
 
        var result = new List<EthernetNeighbor>();


        foreach(var neighbor in lldpOutput.LLDP.Interface.Interfaces)
        {
            result.Add(new EthernetNeighbor
            {
                EthernetInterfaceName = neighbor.Key,
                IPAddress = IPAddress.Parse(neighbor.Value.Chassis.)
            })
        }
    }
}