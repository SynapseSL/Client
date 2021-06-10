using System;
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
    }

    public class ClientModLoader
    {
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
                    run.Enable();
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
            return run;
        }
    }

    public class ModRuntimeData
    {
        public Assembly ModAssembly { get; private set; }
        public ClientMod ClientMod { get; private set; }

        public void Load(byte[] assembly, string name)
        {
            Logger.Info("Loading Assembly");
            ModAssembly = Assembly.Load(assembly);
            Logger.Info("Processing Assembly");
            foreach (var assemblyName in ModAssembly.GetReferencedAssemblies())
            { 
                Logger.Info(assemblyName.ToString());
            }
            var main = ModAssembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(ClientMod)));
            if (main == null)
            {
                Logger.Error($"ClientMod '{name}' doesn't have a main class.");
                return;
            }
            var mainInstance = main.GetConstructor(new Type[0])?.Invoke(new object[0]);
            if (mainInstance == null)
            {
                Logger.Error($"Can't instantiate main class of '{name}'.");
                return;
            }
            ClientMod = mainInstance as ClientMod;
        }

        public void Enable()
        {
            ClientMod.Enable();
        }

        public void Disable()
        {
            ClientMod.Disable();
        }
    }
}