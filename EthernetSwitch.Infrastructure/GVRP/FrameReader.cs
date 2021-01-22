using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EthernetSwitch.Infrastructure.Bash;
using EthernetSwitch.Infrastructure.Ethernet;
using EthernetSwitch.Infrastructure.Settings;
using SharpPcap;
using SQLitePCL;

namespace EthernetSwitch.Infrastructure.GVRP
{
    public class GVRP_data
    {
        public int event_id;
        public int vlan_ID;
    }
    public class FrameReader
    {

        public static async Task StartCapturing(string interface_name, EthernetServices _ethernetservices, CancellationToken stoppingToken)
        {
            var devices = CaptureDeviceList.Instance;
            var interfaceMAC = NetworkInterface.GetAllNetworkInterfaces().Where(x => x.Name == interface_name).First().GetPhysicalAddress();
            byte[] interfaceMACbyte = interfaceMAC.GetAddressBytes();
            var device = devices.Where(x => x.Name == interface_name).First();
            await Task.Run(async () => {
                var readTimeoutMilliseconds = 1000;
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
                while (true)
                {
                    var rawCapture = device.GetNextPacket();

                    if (rawCapture == null)
                    {
                        continue;
                    }
                    if ( //if MAC == 01:80:C2:00:00:21 (GVRP)
                        rawCapture.Data.Count() > 12 &&
                        rawCapture.Data[0] == 1 &&
                        rawCapture.Data[1] == 128 &&
                        rawCapture.Data[2] == 194 &&
                        rawCapture.Data[3] == 0 &&
                        rawCapture.Data[4] == 0 &&
                        rawCapture.Data[5] == 33 &&
                        (rawCapture.Data[6] != interfaceMACbyte[0] |
                        rawCapture.Data[7] != interfaceMACbyte[1] |
                        rawCapture.Data[8] != interfaceMACbyte[2] |
                        rawCapture.Data[9] != interfaceMACbyte[3] |
                        rawCapture.Data[10] != interfaceMACbyte[4] |
                        rawCapture.Data[11] != interfaceMACbyte[5])
                        )
                    {
                        List<RawCapture> commands = new List<RawCapture>();
                        commands.Add(rawCapture);
                        for (int i = 0; i < 50; i++)
                        {
                            rawCapture = device.GetNextPacket();
                            if (rawCapture == null)
                            {
                                continue;
                            }
                            if ( //if MAC == 01:80:C2:00:00:21 (GVRP)
                                rawCapture.Data.Count() > 12 &&
                                rawCapture.Data[0] == 1 &&
                                rawCapture.Data[1] == 128 &&
                                rawCapture.Data[2] == 194 &&
                                rawCapture.Data[3] == 0 &&
                                rawCapture.Data[4] == 0 &&
                                rawCapture.Data[5] == 33 &&
                                (rawCapture.Data[6] != interfaceMACbyte[0] |
                                rawCapture.Data[7] != interfaceMACbyte[1] |
                                rawCapture.Data[8] != interfaceMACbyte[2] |
                                rawCapture.Data[9] != interfaceMACbyte[3] |
                                rawCapture.Data[10] != interfaceMACbyte[4] |
                                rawCapture.Data[11] != interfaceMACbyte[5])
                                )
                            {
                                commands.Add(rawCapture);
                            }
                        }
                        AnalyseFrame(commands, interface_name, _ethernetservices);
                    }
                }
                device.Close();
            }, stoppingToken);
        }
        static void AnalyseFrame(List<RawCapture> frames, string interface_name, EthernetServices _ethernetservices)
        {
            //await Task.Run(() => {

                List<EthernetInterface> GVRP_ON_Interfaces = new List<EthernetInterface>();
                foreach (EthernetInterface nic in _ethernetservices.GetEthernetInterfaces().Where(x => x.GVRP_Enabled == true))
                {
                    GVRP_ON_Interfaces.Add(nic);
                }

                List<GVRP_data> gvrp_events = new List<GVRP_data>();
                foreach (RawCapture frame in frames)
                {
                    string tmpFrame = BitConverter.ToString(frame.Data);
                    string[] frameArry = tmpFrame.Split('-');
                    int i = 20;
                    while (frameArry[i] != "00" && frameArry[i + 1] != "00")// while gvrp data 
                    {
                        gvrp_events.Add(new GVRP_data());
                        gvrp_events.Last().event_id = int.Parse(frameArry[i + 1], System.Globalization.NumberStyles.HexNumber);
                        gvrp_events.Last().vlan_ID = int.Parse(frameArry[i + 2] + frameArry[i + 3], System.Globalization.NumberStyles.HexNumber);
                        i = i + 4;
                    }
                }
                List<int> GVRPadd = new List<int>();
                List<int> GVRPaddTmp = new List<int>();
                List<int> GVRPrm = new List<int>();

                foreach (GVRP_data d in gvrp_events)
                {
                    if (d.event_id == 3) GVRPrm.Add(d.vlan_ID);
                    if (d.event_id == 2) GVRPadd.Add(d.vlan_ID);
                }
                GVRPadd = GVRPadd.Distinct().ToList();
                GVRPrm = GVRPrm.Distinct().ToList();


                //GVRPaddTmp = GVRPadd.Except(GVRPrm).ToList();
                GVRPrm = GVRPrm.Except(GVRPadd).ToList();

                List<GVRP_data> gvrp_eventsToSet = new List<GVRP_data>();
                foreach (int i in GVRPadd)
                {
                    GVRP_data tmp = new GVRP_data();
                    tmp.event_id = 2;
                    tmp.vlan_ID = i;
                    gvrp_eventsToSet.Add(tmp);
                }
                foreach (int i in GVRPrm)
                {
                    GVRP_data tmp = new GVRP_data();
                    tmp.event_id = 3;
                    tmp.vlan_ID = i;
                    gvrp_eventsToSet.Add(tmp);
                }

                foreach (GVRP_data g in gvrp_eventsToSet)
                {
                    if (g.event_id == 3)// remove vlan from interface
                    {
                        foreach (EthernetInterface nic in GVRP_ON_Interfaces)
                        {
                            List<string> vlans = _ethernetservices.GetEthernetInterfaces().Where(x => x.Name == nic.Name).First().VirtualLANs.ToList();

                            bool isVlanToRemove = false;
                            foreach (var vlan in vlans)
                            {
                                if (vlan == g.vlan_ID.ToString()) isVlanToRemove = true;
                            }
                            if (isVlanToRemove == true)
                            {
                                //vlans.Remove(g.vlan_ID.ToString());
                                List<string> tmp = new List<string>();
                                tmp.Add(g.vlan_ID.ToString());
                                //Console.WriteLine("UsuwamVLAN:{0}", tmp[0]);
                                if(tmp.Count() > 0)
                                _ethernetservices.RemoveVlanFromInterface(nic.Name, tmp);
                            }
                        }
                    }
                    if (g.event_id == 2)// add vlan to interface
                    {
                        List<string> vlans = new List<string>();
                        vlans.Add(g.vlan_ID.ToString()); //vlans to add
                        foreach (EthernetInterface nic in GVRP_ON_Interfaces)
                        {
                            List<string> tmp = new List<string>();
                            foreach (string s in nic.VirtualLANs.ToList())
                            {
                                tmp.Add(s);
                            }
                            tmp.Add(g.vlan_ID.ToString());
                            if(tmp.Count() > 0)
                            _ethernetservices.ApplyEthernetGVRPInterfaceVLANs(nic.Name, true, true, tmp);
                        }
                    }
                }
           // });
        }
    }
}
