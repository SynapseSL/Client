using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.IO;
using File = Il2CppSystem.IO.File;

namespace SynapseClient.API.Mods
{
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
}
