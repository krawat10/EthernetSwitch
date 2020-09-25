using System.Net.NetworkInformation;

namespace EthernetSwitch.Extensions
{
    public static class NetworkInterfaceExtensions
    {
        public static bool IsEthernet(this NetworkInterface networkInterface)
        {
            return networkInterface.Name.Contains("vlan") == false & 
                   networkInterface.Name.Contains(".") == false &
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx ||  
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet;
        }
    }
}