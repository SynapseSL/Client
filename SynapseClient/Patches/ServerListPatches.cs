using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HarmonyLib;
using Il2CppSystem;
using LiteNetLib.Utils;
using MelonLoader;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;
using String = Il2CppSystem.String;

namespace SynapseClient.Patches
{
    public class ServerListPatches
    {

        public static List<SynapseServerEntry> Servers { get; set; } = new List<SynapseServerEntry>();
        public static ServerFilter ListSingleton { get; set; }

        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.Refresh))]
        [HarmonyPrefix]
        public static bool OnServerListRefresh(NewServerBrowser __instance)
        {
            return true;
        }
 
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ReapplyFilters))]
        [HarmonyPrefix]
        public static bool OnReapply(ServerFilter __instance, bool forceCleanup)
        {
            ListSingleton = __instance;
            Composite(__instance);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ChangeTab), typeof(int))]
        [HarmonyPrefix]
        public static bool OnReapply(ServerFilter __instance, int tab)
        {
            ListSingleton = __instance;
            Composite(__instance);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ChangeTab), typeof(ServerTab))]
        [HarmonyPrefix]
        public static bool OnReapply(ServerFilter __instance, ServerTab tab)
        {
            ListSingleton = __instance;
            Composite(__instance);
            return false;
        }

        
        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.OnEnable))]
        [HarmonyPrefix]
        public static bool OnServerListEnable(NewServerBrowser __instance)
        {
            var filter = __instance.GetComponent<ServerFilter>();
            ListSingleton = filter;
            Composite(filter);
            return true;
        }

        private static void Composite(ServerFilter filter)
        {
            filter.FilteredListItems = new Il2CppSystem.Collections.Generic.List<ServerListItem>();
            foreach (var serverEntry in Servers)
            {
                AddServer(filter, serverEntry);
            }
            filter.DisplayServers();
            try
            {
                GameObject.Find("New Main Menu/Servers/ServerBrowser/Loading").active = false;
            } catch {}
            Logger.Info("Servers displayed");
        }

        public static void AddServer(ServerFilter filter, SynapseServerEntry entry)
        {
            var leakingObject = new LeakingObject<ServerListItem>();
            leakingObject.decorated = new ServerListItem
            {
                ip = (String) entry.address,
                port = 0000,
                players = (String) (entry.onlinePlayers + "/" + entry.maxPlayers),
                info = (String) entry.info,
                pastebin = (String) entry.pastebin,
                version = (String) entry.version,
                whitelist = entry.whitelist,
                modded = entry.modded,
                friendlyFire = entry.friendlyFire,
                officialCode = entry.officialCode
            };
            filter.FilteredListItems.Add(leakingObject.decorated);
            leakingObject.Dispose();
        }
        
        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.DownloadList))]
        [HarmonyPrefix]
        public static bool OnServerListAwake()
        {
            Logger.Info("Download Servers");
            var webClient = new WebClient();
            var response = webClient.DownloadString("https://servers.synapsesl.xyz/serverlist");
            Servers = JsonConvert.DeserializeObject<List<SynapseServerEntry>>(response);
            Composite(ListSingleton);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerElementButton), nameof(ServerElementButton.PlayButton))]
        [HarmonyPrefix]
        public static bool OnPlayButton(ServerElementButton __instance)
        {
            Logger.Info("Pressed Play Button");
            Logger.Info("Should connect to: " + __instance.IpAddress);
            var msg = SynapseClientPlugin.GetConnection(__instance.IpAddress.Replace(":0", ""));
            Logger.Info("Final: " + msg);
            SynapseClientPlugin.Connect(msg);
            return false;
        }
        

        public class SynapseServerEntry
        {
            public string id { get; set; }
            public string address { get; set; }
            public int onlinePlayers { get; set; }
            public int maxPlayers { get; set; }
            public string info { get; set; }
            public string pastebin { get; set; }
            public string version { get; set; }
            public bool whitelist { get; set; } = false;
            public bool modded { get; set; } = true;
            public bool friendlyFire { get; set; } = true;
            public byte officialCode { get; set; } = Byte.MinValue;
            
        }
    }
}