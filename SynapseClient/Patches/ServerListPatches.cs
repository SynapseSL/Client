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
using SynapseClient.API;
using UnityEngine;
using String = Il2CppSystem.String;

namespace SynapseClient.Patches
{
    public class ServerListPatches
    {
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
            foreach (var serverEntry in SynapseClientPlugin.Singleton.SynapseServerList.ServerCache)
            {
                SynapseServerList.AddServer(filter, serverEntry);
            }
            filter.DisplayServers();
            try
            {
                GameObject.Find("New Main Menu/Servers/ServerBrowser/Loading").active = false;
            } catch {}
            Logger.Info("Servers displayed");
        }

        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.DownloadList))]
        [HarmonyPrefix]
        public static bool OnServerListAwake()
        {
            Logger.Info("Download Servers");
            SynapseClientPlugin.Singleton.SynapseServerList.Download();
            Composite(ListSingleton);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerElementButton), nameof(ServerElementButton.PlayButton))]
        [HarmonyPrefix]
        public static bool OnPlayButton(ServerElementButton __instance)
        {
            var entry = SynapseClientPlugin.Singleton.SynapseServerList.ResolveIdAddress(__instance.IpAddress);
            var resolved = SynapseClientPlugin.GetConnection(entry.address);
            var allow = Events.InvokeServerConnect(resolved);
            if (!allow) return false;
            SynapseClientPlugin.Connect(resolved);
            return false;
        }
    }
}