using System;
using System.Buffers.Text;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace SynapseClient.ClientModule.Patches;

[Patches]
public class ServerListPatches
{
         public static ServerFilter ListSingleton { get; set; }

        [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.Refresh))]
        [HarmonyPrefix]
        public static bool OnServerListRefresh()
        {
            Composite(ListSingleton);
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
            for (int i = 0; i < 50; i++)
            {
                filter.FilteredListItems.Add(new ServerListItem()
                {
                    ip = "localhost",
                    port = 7777,
                    players = "0/0",
                    info = Convert.ToBase64String(Encoding.UTF8.GetBytes($"Placeholder {i}")),
                    pastebin = "",
                    version = "synapse-client",
                    whitelist = false,
                    modded = true,
                    friendlyFire = true,
                    officialCode = 0x69
                });
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
            Composite(ListSingleton);
            
            return false;
        }

        [HarmonyPatch(typeof(ServerElementButton), nameof(ServerElementButton.PlayButton))]
        [HarmonyPrefix]
        public static bool OnPlayButton(ServerElementButton __instance)
        {
            return false;
        }
}