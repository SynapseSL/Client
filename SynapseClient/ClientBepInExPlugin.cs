using System;
using System.IO;
using BepInEx;
using BepInEx.IL2CPP;
using SynapseClient.API;
using SynapseClient.API.Mods;
using SynapseClient.Components;

namespace SynapseClient
{
    [BepInPlugin("xyz.synapse.client.plugin", "SynapseClient", "0.0.0.1")]
    public class ClientBepInExPlugin : BasePlugin
    {
        public const string ClientVersion = "1.0.0";
        public const int ClientMajor = 1;
        public const int ClientMinor = 0;
        public const int ClientPatch = 0;

        //Change this Later to get it from the json
        public const string CentralServer = "https://central.synapsesl.xyz";
        public const string ServerListServer = "https://servers.synapsesl.xyz";
        
        public static ClientBepInExPlugin Get { get; private set; }

        public Client Client { get; } = new Client();

        public ClientModLoader ModLoader { get; } = new ClientModLoader();

        public override void Load()
        {
            Get = this;

            try
            {
                ModLoader.LoadAll();
                ModLoader.EnableAll();

                Logger.Info("Registering Types for Il2Cpp use...");
                ComponentHandler.Get.RegisterTypes();

                Logger.Info("Loading Prefabs");
                if (!Directory.Exists("bundles")) Directory.CreateDirectory("bundles");

                Client.Patcher.PatchAll();

                Client.SpawnController.Subscribe();
            }
            catch(Exception e)
            {
                Logger.Error("SynapseClient: Startup failed:\n" + e);
            }
        }
    }
}