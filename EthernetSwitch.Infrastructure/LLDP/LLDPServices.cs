using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Xml.Serialization;
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
        var output = _bash.Execute("lldpcli show neighbors -f xml");

        try
        {
            var serializer = new XmlSerializer(typeof(Lldp));
            using TextReader reader = new StringReader(output);
            var lldpOutput = (Lldp)serializer.Deserialize(reader);
            
            foreach (var neighbor in lldpOutput.Interface)
            {
                if (neighbor.Chassis != null && neighbor.Chassis.Mgmtip.Any())
                    result.Add(new EthernetNeighbor
                    {
                        EthernetInterfaceName = neighbor.Name,
                        IPAddress = IPAddress.Parse(neighbor.Chassis.Mgmtip.FirstOrDefault()?.Text),
                        MAC = neighbor.Chassis.Id?.Text,
                        SystemDescription = neighbor.Chassis.Descr.Text,
                        SystemName = neighbor.Chassis.Name?.Text,
                        Capabilities = neighbor.Chassis.Capability?.Where(cap => cap.Enabled == "on").Select(variable => variable.Type) ?? new List<string>(),
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