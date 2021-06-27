using System;
using System.Linq;
using System.Reflection;

namespace SynapseClient.API.Mods
{
    public class ModRuntimeData
    {
        public Assembly ModAssembly { get; private set; }
        public ClientMod ClientMod { get; private set; }

        public ClientModDetails Details { get; private set; }

        public void Load(byte[] assembly, string name)
        {
            Logger.Info($"Loading Assembly '{name}'");
            ModAssembly = Assembly.Load(assembly);
            foreach (var assemblyName in ModAssembly.GetReferencedAssemblies())
            {
                Logger.Info(assemblyName.ToString());
            }
            var main = ModAssembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(ClientMod)));
            if (main == null)
            {
                Logger.Error($"ClientMod at '{name}' doesn't have a main class.");
                return;
            }
            Details = main.GetCustomAttribute<ClientModDetails>();
            Logger.Info($"ClientMod '{Details.Name}' found, proceeding with Initialization");
            var mainInstance = main.GetConstructor(new Type[0])?.Invoke(new object[0]);
            if (mainInstance == null)
            {
                Logger.Error($"Can't instantiate main class of '{Details.Name}'.");
                return;
            }
            ClientMod = mainInstance as ClientMod;
        }

        public void MainEnable()
        {
            if (Details.ActivatedByServer) return;
            ClientMod.Enable();
        }

        public void MainDisable()
        {
            if (Details.ActivatedByServer) return;
            ClientMod.Disable();
        }

        public void ActivateForServer()
        {
            Logger.Info($"Activating ClientMod '{Details.Name}' for server");
            if (Details.ActivatedByServer) ClientMod.Enable();
            ClientMod.ActivateForServer();
        }

        public void ServerConnectionEnd()
        {
            ClientMod.ServerConnectionEnd();
            if (Details.ActivatedByServer) ClientMod.Disable();
        }
    }
}
