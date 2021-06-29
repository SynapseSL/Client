using System;
using System.IO;
using BepInEx;
using BepInEx.IL2CPP;
using CommandSystem.Commands;
using Newtonsoft.Json;
using SynapseClient.API;
using SynapseClient.API.Events;
using SynapseClient.API.Mods;
using SynapseClient.Components;
using SynapseClient.Models;
using SynapseClient.Patches;

namespace SynapseClient
{
    [BepInPlugin("xyz.synapse.client.plugin", "SynapseClient", "0.0.0.1")]
    public class ClientBepInExPlugin : BasePlugin
    {
        public const int ClientMajor = 1;
        public const int ClientMinor = 0;
        public const int ClientPatch = 0;
        public const string ClientVersion = "1.0.0-Beta";
        public const string ClientDescription = "The Synapse client is a modificated version of the SCP:SL client with a lot of Features like Custom Central Server support and Packet Pipeline support between Server and Client";

        public string CentralServer { get; private set; } = "https://central.synapsesl.xyz";
        public string ServerListServer { get; private set; } = "https://servers.synapsesl.xyz";
        
        public static ClientBepInExPlugin Get { get; private set; }

        public Client Client { get; } = new Client();

        public ClientModLoader ModLoader { get; } = new ClientModLoader();

        public override void Load()
        {
            Get = this;

            try
            {
                CustomNetworkManager.Modded = true;
                BuildInfoCommand.ModDescription = $"\n=====ClientMod=====\nClient: Synapse\nClient Version: {ClientVersion}\nDescription: {ClientDescription}";

                LoadServersFromJson();

                ModLoader.LoadAll();
                ModLoader.EnableAll();

                ComponentHandler.Get.RegisterTypes();
                PatchHandler.Get.PatchAll();
                EventHandlers.Get.RegisterEvents();

                if (!Directory.Exists("bundles")) Directory.CreateDirectory("bundles");
                Client.SpawnController.Subscribe();
            }
            catch(Exception e)
            {
                Logger.Error("SynapseClient: Startup failed:\n" + e);
            }
        }

        public void LoadServersFromJson()
        {
            var path = Path.Combine(Computer.Get.ApplicationDataDir, "apis.json");

            if (!File.Exists(path)) return;

            var usedAPIs = JsonConvert.DeserializeObject<UsedAPIs>(File.ReadAllText(path));

            CentralServer = usedAPIs.CentralServer;
            ServerListServer = usedAPIs.ServerList;
        }
    }
}