using System;
using System.IO;
using BepInEx;
using BepInEx.IL2CPP;
using SynapseClient.API;
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