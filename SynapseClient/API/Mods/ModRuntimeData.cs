using System;
using System.Linq;
using System.Reflection;
using SynapseClient.Command;
using System.Collections.Generic;

namespace SynapseClient.API.Mods
{
    public class ModRuntimeData
    {
        public Assembly ModAssembly { get; private set; }

        public Type[] Types { get; private set; }

        public ClientMod ClientMod { get; private set; }

        public List<FullCommand> Commands { get; private set; }

        public ClientModDetails Details { get; private set; }


        public void Load(byte[] assembly, string name)
        {
            Logger.Info($"Loading Assembly '{name}'");

            ModAssembly = Assembly.Load(assembly);

            foreach (var assemblyName in ModAssembly.GetReferencedAssemblies())
            {
                Logger.Info(assemblyName.ToString());
            }

            Types = ModAssembly.GetTypes();

            var maintype = Types.FirstOrDefault(x => x.IsSubclassOf(typeof(ClientMod)));

            if (maintype == null)
            {
                Logger.Error($"ClientMod at '{name}' doesn't have a main class.");
                return;
            }

            Details = maintype.GetCustomAttribute<ClientModDetails>();

            if(Details == null)
            {
                Logger.Error($"ClientMod at '{name}' doesn't have a ClientModDetails attribute");
                return;
            }

            Logger.Info($"ClientMod '{Details.Name}' found, proceeding with Initialization");
            var mainInstance = maintype.GetConstructor(new Type[0])?.Invoke(new object[0]);
            if (mainInstance == null)
            {
                Logger.Error($"Can't instantiate main class of '{Details.Name}'.");
                return;
            }

            ClientMod = mainInstance as ClientMod;

            foreach(var type in Types)
            {
                if (!type.IsSubclassOf(typeof(ISynapseCommand))) continue;

                Commands.Add(SynapseCommandHandler.Get.RegisterSynapseCommand(type, maintype, ClientMod));
            }
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
