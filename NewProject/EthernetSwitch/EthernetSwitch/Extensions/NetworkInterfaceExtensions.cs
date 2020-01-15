using System.Net.NetworkInformation;

namespace EthernetSwitch.Extensions
{
    public static class NetworkInterfaceExtensions
    {
        public static bool IsEthernet(this NetworkInterface networkInterface)
        {
            return networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT ||
                   networkInterface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet;
        }
    }
}