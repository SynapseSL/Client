using HarmonyLib;
using SynapseClient.API;
using UnityEngine;

namespace SynapseClient.Patches
{
    public class ServerListPatches
    {
        public static ServerFilter ListSingleton { get; set; }

        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.Refresh))]
        [HarmonyPrefix]
        public static bool OnServerListRefresh()
        {
            SynapseServerList.Get.Download();
            return true;
        }
 
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ReapplyFilters))]
        [HarmonyPrefix]
        public static bool OnReapply(ServerFilter __instance)
        {
            ListSingleton = __instance;
            Composite(__instance);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ChangeTab), typeof(int))]
        [HarmonyPrefix]
        public static bool OnReapplyWithInt(ServerFilter __instance)
        {
            ListSingleton = __instance;
            Composite(__instance);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerFilter), nameof(ServerFilter.ChangeTab), typeof(ServerTab))]
        [HarmonyPrefix]
        public static bool OnReapplyWithTab(ServerFilter __instance)
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
            foreach (var serverEntry in SynapseServerList.Get.ServerCache)
            {
                SynapseServerList.Get.AddServer(filter, serverEntry);
            }
            filter.DisplayServers();
            try
            {
                GameObject.Find("New Main Menu/Servers/ServerBrowser/Loading").active = false;
            } catch {}
        }

        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.DownloadList))]
        [HarmonyPrefix]
        public static bool OnServerListAwake()
        {
            SynapseServerList.Get.Download();
            Composite(ListSingleton);
            return false;
        }
        
        [HarmonyPatch(typeof(ServerElementButton), nameof(ServerElementButton.PlayButton))]
        [HarmonyPrefix]
        public static bool OnPlayButton(ServerElementButton __instance)
        {
            var entry = SynapseServerList.Get.ResolveIdAddress(__instance.IpAddress);
            var resolved = NetworkingUtils.GetConnection(entry.Address);
            var allow = API.Events.SynapseEvents.InvokeServerConnect(resolved);
            if (!allow) return false;
            Client.Get.Connect(resolved);
            return false;
        }
    }
}