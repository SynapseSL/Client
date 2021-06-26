using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Il2CppSystem.IO;
using File = Il2CppSystem.IO.File;

namespace SynapseClient.API
{
    public abstract class ClientMod
    {
        public virtual void Enable()
        {
            
        }

        public virtual void Disable()
        {
            
        }

        public virtual void ActivateForServer()
        {
            
        }

        public virtual void ServerConnectionEnd()
        {
            
        }
    }

    public class ClientModDetails : Attribute
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool ActivatedByServer { get; set; } = false;
    }

    public class ClientModLoader
    {
        public List<ModRuntimeData> Mods { get; private set; } = new List<ModRuntimeData>();
        
        public void LoadAll()
        {
            if (!Directory.Exists("mods")) Directory.CreateDirectory("mods");
            foreach (var file in Directory.GetFiles("mods"))
            {
                Logger.Info($"Loading mod from file {file}");
                var fileStream = File.OpenRead(file);
                var memStream = new MemoryStream(); 
                fileStream.CopyToAsync(memStream).GetAwaiter().GetResult();
                var bytes = memStream.ToArray();
                memStream.Dispose();
                fileStream.Dispose();
                Logger.Info($"Loading mod...");
                try
                {
                    var run = LoadMod(bytes, file.Replace(".dll", ""));
                    Logger.Info($"Enabling mod...");
                    run.MainEnable();
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                    throw;
                }
            }
        }
        
        public ModRuntimeData LoadMod(byte[] bytes, string name)
        {
            var run = new ModRuntimeData();
            run.Load(bytes, name);
            Mods.Add(run);
            return run;
        }

        public void EnableAll()
        {
            Mods.ForEach(data => data.MainEnable());
        }

        public void ActivateForServer(string[] clientMods)
        {
            Mods.Where(x => clientMods.Contains(x.Details.Name)).ToList().ForEach(data => data.ActivateForServer());
        }

        public void ServerConnectionEnd()
        {
            Mods.ForEach(data => data.ServerConnectionEnd());
        }
    }

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