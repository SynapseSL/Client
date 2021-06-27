using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace SynapseClient.API
{
    public class Computer
    {
        internal Computer() { }

        public static Computer Get => Client.Get.Computer;

        public string GetMac()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault() ?? "Unknown";
        }

        public string GetPcName()
        {
            return Environment.MachineName ?? "Unknown";
        }
    }
}
