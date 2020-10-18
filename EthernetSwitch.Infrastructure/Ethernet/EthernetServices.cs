using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Bash.Exceptions;
using EthernetSwitch.Infrastructure.Extensions;
using EthernetSwitch.Infrastructure.Settings;

namespace EthernetSwitch.Infrastructure.Ethernet
{
    public enum InterfaceType { Off, Isolated, Promiscuous }

    public class EthernetInterface
    {
        public string Name { get; set; }
        public IEnumerable<string> VirtualLANs { get; set; }
        public bool Tagged { get; set; }
        public OperationalStatus Status { get; set; }
        public InterfaceType Type { get; set; }
        public bool IsHostInterface { get; set; }
        public bool AllowTagging { get; set; }

    }

    public class EthernetServices
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IBashCommand _bash;

        public EthernetServices(ISettingsRepository settingsRepository, IBashCommand bash)
        {
            _settingsRepository = settingsRepository;
            _bash = bash;
        }

        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public IEnumerable<EthernetInterface> GetEthernetInterfaces()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                if (networkInterface.IsEthernet())
                {
                    var output = _bash.Execute(
                            $"ip link show | grep {networkInterface.Name}| grep vlan | cut -d' ' -f9 | cut -d'n' -f2");

                    var appliedVLANs = output
                        .Replace("\t", string.Empty)
                        .Replace(networkInterface.Name, string.Empty)
                        .Split('\n')
                        .Select(vlan => vlan.Trim('.'))
                        .Where(vlan => !string.IsNullOrWhiteSpace(vlan))
                        .Where(vlan => !vlan.ToLower().Equals("down"))
                        .ToList();

                    var isHostInterface = networkInterface
                        .GetIPProperties()
                        .UnicastAddresses
                        .Any(info => info.Address.Equals(GetLocalIPAddress()));

                    yield return new EthernetInterface
                    {
                        Name = networkInterface.Name,
                        Status = networkInterface.OperationalStatus,
                        VirtualLANs = appliedVLANs,
                        IsHostInterface = isHostInterface,
                        Tagged = IsTagged(networkInterface.Name),
                        Type = GetInterfaceType(networkInterface)
                    };
                }
        }

        private InterfaceType GetInterfaceType(NetworkInterface networkInterface)
        {
            InterfaceType type;
            if (IsIsolated(networkInterface))
                type = InterfaceType.Isolated;
            else if (IsPromiscuous(networkInterface))
                type = InterfaceType.Promiscuous;
            else
                type = InterfaceType.Off;
            return type;
        }

        private bool IsPromiscuous(NetworkInterface networkInterface)
        {
            var isPromiscuous = true;

            try
            {
                _bash.Execute(
                    $"ebtables -L | grep ACCEPT | cut -d' ' -f4 | grep {networkInterface.Name}");
            }
            catch (ProcessException e)
            {
                var error = e.ExitCode;
                if (error == 1) isPromiscuous = false;
            }

            return isPromiscuous;
        }

        private bool IsTagged(string name)
        {
            var isTagged = true;
            try
            {
                _bash.Execute($"ip link show | grep @{name}");
            }
            catch (ProcessException e)
            {
                var error = e.ExitCode;
                if (error == 1) isTagged = false;
            }

            return isTagged;
        }

        private bool IsIsolated(NetworkInterface networkInterface)
        {
            try
            {
                _bash.Execute($"ebtables -L | grep DROP | cut -d' ' -f2 | grep {networkInterface.Name}");
            }
            catch (ProcessException e)
            {
                var error = e.ExitCode;
                if (error == 1) return  false;
            }

            return true;
        }

        public void SetEthernetInterfaceState(string name, bool isActive)
        {
            if (isActive)
                _bash.Execute($"ip link set {name} up");
            else
                _bash.Execute($"ip link set {name} down");

        }

        public void ApplyEthernetInterfaceVLANs(string ethernetName, bool isTagged, IEnumerable<string> vlanNames)
        {
            foreach (var vlanName in vlanNames) // All selected VLANs
            {
                // Checks if VLAN exists
                var vlanExists = true;
                try
                {
                    _bash.Execute($"brctl show vlan{vlanName}");
                }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains($"bridge vlan{vlanName} does not exist!\n")) vlanExists = false; //true if exists
                }

                // Checks if interface is in any VLAN
                var interfaceHasVLAN = true;
                try
                {
                    _bash.Execute($"brctl show | grep {ethernetName}");
                }
                catch (ProcessException e)
                {
                    var error = e.ExitCode;
                    if (error == 1) interfaceHasVLAN = false;
                }

                // Creates VLAN
                if (!vlanExists)
                {
                    _bash.Execute($"brctl addbr vlan{vlanName}");
                    _bash.Execute($"ip link set vlan{vlanName} up"); //Create VLAN
                }

                // Adds non-tagged interface to VLAN
                var tagged = IsTagged(ethernetName);

                if (interfaceHasVLAN & (isTagged == false))
                {
                    // Removes from VLAN which is assigned to
                    var vlanID =
                        _bash.Execute(
                            $"ip link show | grep [[:space:]]{ethernetName}: | cut -d' ' -f9 | cut -d'n' -f2"); // Gets VLAN numer
                    vlanID = vlanID.Replace("\n", "");
                    _bash.Execute($"ip link set vlan{vlanID} down");
                    _bash.Execute($"brctl delif vlan{vlanID} {ethernetName}");
                }

                if (!isTagged)
                {
                    _bash.Execute($"ip link set vlan{vlanName} down");
                    _bash.Execute($"brctl addif vlan{vlanName} {ethernetName}");
                    _bash.Execute($"ip link set vlan{vlanName} up");
                }

                //Creates tagged interface
                if (isTagged)
                {
                    _bash.Execute($"ip link set vlan{vlanName} down");
                    _bash.Execute($"ip link add link {ethernetName} name {ethernetName}.{vlanName} type vlan id {vlanName}");
                    _bash.Execute($"ip link set vlan{vlanName} up");
                }

                //Adds tagged interface to VLAN
                if (isTagged)
                {
                    _bash.Execute($"ip link set {ethernetName} up");
                    _bash.Execute($"ip link set vlan{vlanName} down");
                    _bash.Execute($"brctl addif vlan{vlanName} {ethernetName}.{vlanName}");
                    _bash.Execute($"ip link set {ethernetName}.{vlanName} up");
                    _bash.Execute($"ip link set vlan{vlanName} up");
                }
            }

        }


        public void ClearEthernetInterfaceVLANs(string name)
        {
            // Gets interface config
            var ethConfigOutput =
                _bash.Execute(
                    $"ip link show | grep {name}| grep vlan | cut -d' ' -f9 | cut -d'n' -f2");

            var vlanNames = ethConfigOutput
                .Replace("\t", string.Empty)
                .Replace(name, string.Empty)
                .Split('\n')
                .Select(vlan => vlan.Trim('.'))
                .Where(vlan => !string.IsNullOrWhiteSpace(vlan))
                .ToList();

            foreach (var vlanName in vlanNames) // All selected vlans
            {
                // Tagged interfaces
                var ifToRemIsTaged = true;
                try
                {
                    _bash.Execute($"ip link show {name}.{vlanName}");
                }
                catch (ProcessException e)
                {
                    var error = e.Message;
                    if (error.Contains("does not exist.\n")) ifToRemIsTaged = false;
                }

                if (ifToRemIsTaged)
                {
                    _bash.Execute($"ip link set {name}.{vlanName} down"); // Off interface
                    _bash.Execute($"ip link delete {name}.{vlanName}");
                }
                else //Non-tagged
                {
                    _bash.Execute($"ip link set vlan{vlanName} down");
                    _bash.Execute($"brctl delif vlan{vlanName} {name}");
                    _bash.Execute($"ip link set vlan{vlanName} up");
                }

                // Clears empty bridged
                var output3 = _bash.Execute($"brctl show vlan{vlanName} | grep vlan{vlanName} | cut -f6");

                if (output3 == "\n")
                {
                    _bash.Execute($"ip link set vlan{vlanName} down");
                    _bash.Execute($"ip link delete vlan{vlanName}");
                }
            }
        }

        public void SetEthernetInterfaceType(string interfaceName, InterfaceType interfaceType)
        {
            switch (interfaceType)
            {
                case InterfaceType.Off:

                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        if (networkInterface.IsEthernet())
                        {
                            //Clear interface drop rules
                            try
                            {
                                var output = _bash.Execute(
                                    $"ebtables -D FORWARD -i {interfaceName} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            //Clear interface acceptation rules
                            try
                            {
                                var output = _bash.Execute(
                                    $"ebtables -D FORWARD -i {networkInterface.Name} -o {interfaceName} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }
                        }

                    break;

                case InterfaceType.Isolated:

                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        if (networkInterface.IsEthernet())
                        {
                            //Clear interface drop rules
                            try
                            {
                                var output = _bash.Execute(
                                    $"ebtables -D FORWARD -i {interfaceName} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            //Clear interface acceptation rules
                            try
                            {
                                var output = _bash.Execute(
                                    $"ebtables -D FORWARD -i {networkInterface.Name}  -o  {interfaceName} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            // Block access
                            if (interfaceName != networkInterface.Name)
                                try
                                {
                                    var output = _bash.Execute(
                                        $"ebtables -A FORWARD -i {interfaceName} -o {networkInterface.Name} -j DROP");
                                }
                                catch (ProcessException e)
                                {
                                    var error = e.ExitCode;
                                }
                        }

                    break;
                case InterfaceType.Promiscuous:
                    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                        if (networkInterface.IsEthernet())
                        {
                            // Clears interface drop rules
                            try
                            {
                                var output = _bash.Execute(
                                    $"ebtables -D FORWARD -i {interfaceName} -o {networkInterface.Name} -j DROP");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            //Clear interface acceptation rules
                            try
                            {
                                var output = _bash.Execute(
                                    $"ebtables -D FORWARD -i {networkInterface.Name} -o {interfaceName} -j ACCEPT");
                            }
                            catch (ProcessException e)
                            {
                                var error = e.ExitCode;
                            }

                            // Grants access
                            if (interfaceName != networkInterface.Name)
                                try
                                {
                                    _bash.Execute(
                                        $"ebtables -I FORWARD -i {networkInterface.Name} -o {interfaceName} -j ACCEPT");
                                }
                                catch (ProcessException e)
                                {
                                    var error = e.ExitCode;
                                }
                        }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}