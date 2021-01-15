using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Bash.Exceptions;
using EthernetSwitch.Infrastructure.LLDP;

public class EthernetNeighbor
{
    public string EthernetInterfaceName { get; set; }
    public string MAC { get; set; }
    public string SystemName { get; set; }
    public string SystemDescription { get; set; }
    public IPAddress IPAddress { get; set; }
    public IEnumerable<string> Capabilities { get; internal set; }
    public string Age { get; set; }
}

public class LLDPServices
{
    private readonly IBashCommand _bash;

    public LLDPServices(IBashCommand bash)
    {
        this._bash = bash;
    }

    public void ActivateLLDPAgent()
    {
        try
        {
            _bash.Install("lldpd");
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            _bash.Execute("lldpd -d");
        }
        catch (ProcessException processException)
        {
            if (processException.ExitCode != 1) throw;
        }
    }

    public List<EthernetNeighbor> GetNeighbours()
    {
        var result = new List<EthernetNeighbor>();
        var output = _bash.Execute("lldpcli show neighbors -f json0");

        try
        {
            var lldpOutput = JsonSerializer.Deserialize<LldpOutput>(output);

            foreach (var neighbor in lldpOutput.Lldp.First().Interface)
            {
                result.Add(new EthernetNeighbor
                {
                    EthernetInterfaceName = neighbor.Name,
                    IPAddress = IPAddress.Parse(neighbor.Chassis.FirstOrDefault()?.MgmtIp.FirstOrDefault()?.Value),
                    MAC = neighbor.Chassis.FirstOrDefault()?.Id.FirstOrDefault(id => id.Type == "mac")?.Value,
                    SystemDescription = neighbor.Chassis.FirstOrDefault()?.Descr.FirstOrDefault()?.Value,
                    SystemName = neighbor.Chassis.FirstOrDefault()?.Name.FirstOrDefault()?.Value,
                    Capabilities = neighbor.Chassis.FirstOrDefault()?.Capability?.Where(cap => cap.Enabled).Select(variable => variable.Type) ?? new List<string>(),
                    Age = neighbor.Age
                });
            }
        }
        catch(Exception e)
        {

        }

        return result;
    }
}