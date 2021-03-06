using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.IO;
using File = Il2CppSystem.IO.File;

namespace SynapseClient.API.Mods
{
    public class ClientModLoader
    {
        internal ClientModLoader() { }

        public List<ModRuntimeData> Mods { get; private set; } = new List<ModRuntimeData>();

        public void LoadAll()
        {
            Logger.Info("Loading Mods");

            if (!Directory.Exists("mods")) Directory.CreateDirectory("mods");

            foreach (var file in Directory.GetFiles("mods", "*.dll")) 
            {
                try
                {
                    Logger.Info($"Loading mod from file {file}");

                    var fileStream = File.OpenRead(file);
                    var memStream = new MemoryStream();
                    fileStream.CopyToAsync(memStream).GetAwaiter().GetResult();
                    var bytes = memStream.ToArray();
                    memStream.Dispose();
                    fileStream.Dispose();

                    Logger.Info($"Loading mod...");

                    var run = LoadMod(bytes, Path.GetFileNameWithoutExtension(file));

                    Logger.Info($"Enabling mod...");

                    run.MainEnable();
                }
                catch (Exception e)
                {
                    Logger.Error($"Synapse-Mods: Error while Loading File:\n{file}\n{e}");
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
            Logger.Info("Loaded Mods. Enabling mods");
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
}
