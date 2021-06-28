using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;

namespace SynapseClient.API
{
    public class Computer
    {
        internal Computer() { }

        public static Computer Get => Client.Get.Computer;

        public string Mac => NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault() ?? "Unknown";

        public string PcName => Environment.MachineName ?? "Unknown";

        public string UserName => Environment.UserName ?? "Unknown";

        public string ApplicationDataDir
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse Client");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }
    }
}
