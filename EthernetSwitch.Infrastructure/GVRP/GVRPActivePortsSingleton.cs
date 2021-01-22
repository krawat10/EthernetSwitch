using System.Collections.Generic;

namespace EthernetSwitch.Infrastructure.GVRP
{

    public enum InterfaceState
    {
        Off, Listening
    }
    public class GVRPActivePortsSingleton
    {
        private static GVRPActivePortsSingleton instance = null;
        private static readonly object padlock = new object();
        public IDictionary<string, InterfaceState> InterfaceStates { get; set; }


        GVRPActivePortsSingleton()
        {
            InterfaceStates =new Dictionary<string, InterfaceState>();

        }

        public static GVRPActivePortsSingleton Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ??= new GVRPActivePortsSingleton();
                }
            }
        }
    }
}