using HarmonyLib;
using UnityEngine;

namespace SynapseClient.Platform.Patches;

public class ServerListPatches
{
    [HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.OnEnable))]
    [HarmonyPrefix]
    public static bool OnServerListEnable(NewServerBrowser __instance)
    {
        Logger.Info("ServerList Enable!");
        var filter = __instance.GetComponent<ServerFilter>();
        var gameObject = GameObject.Find("New Main Menu/Servers/Auth Status");
        gameObject.SetActive(false);
        return true;
    }
}