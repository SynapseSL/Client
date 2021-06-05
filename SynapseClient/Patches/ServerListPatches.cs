using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using String = Il2CppSystem.String;

namespace SynapseClient.Patches
{
    public class ServerListPatches
    {
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
            Composite(__instance);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ChangeTab), typeof(int))]
        [HarmonyPrefix]
        public static bool OnReapply(ServerFilter __instance, int tab)
        {
            Composite(__instance);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ChangeTab), typeof(ServerTab))]
        [HarmonyPrefix]
        public static bool OnReapply(ServerFilter __instance, ServerTab tab)
        {
            Composite(__instance);
            return false;
        }

        
        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.OnEnable))]
        [HarmonyPrefix]
        public static bool OnServerListEnable(NewServerBrowser __instance)
        {
            var filter = __instance.GetComponent<ServerFilter>();
            Composite(filter);
            return true;
        }

        private static void Composite(ServerFilter filter)
        {
            filter.FilteredListItems = new Il2CppSystem.Collections.Generic.List<ServerListItem>();
            var servers = new List<SynapseServerEntry>();
            servers.Add(new SynapseServerEntry
            {
                ip = "localhost",
                port = 7777,
                players = "9/10",
                info = "Tm9yZGhvbHouZGU=",
                pastebin = "PsRbh1yR",
                version = "1.0",
                whitelist = false,
                modded = true,
                friendlyFire = true,
                officialCode = byte.MinValue
            });
            foreach (var serverEntry in servers)
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
                ip = (String) entry.ip,
                port = entry.port,
                players = (String) entry.players,
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
            return false;
        }

        public class SynapseServerEntry
        {
            public string ip { get; set; }
            public ushort port { get; set; } = 7777;
            public string players { get; set; }
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