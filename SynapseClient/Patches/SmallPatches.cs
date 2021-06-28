using System.IO;
using HarmonyLib;
using SynapseClient.API;
using SynapseClient.Components;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.Patches
{
    public class SmallPatches
    {
        [HarmonyPatch(typeof(NewMainMenu), nameof(NewMainMenu.Start))]
        [HarmonyPrefix]
        public static bool OnMainMenuStart()
        {
            Logger.Info("Main Menu hooked!");
            var obj = new GameObject();
            obj.AddComponent<SynapseMenuWorker>();
            
            var texture = new Texture2D(256, 256);
            ImageConversion.LoadImage(texture, File.ReadAllBytes("synapse.png"), false);
            GameObject.Find("Canvas/Logo").GetComponent<RawImage>().texture = texture;

            if (Client.Get._redirectCallback != null)
            {
                Client.Get._redirectCallback.Invoke();
                Client.Get._redirectCallback = null;
            }
            
            return true;
        }
        
        [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.Start))]
        [HarmonyPrefix]
        public static bool StartNicknameSync(ServerRoles __instance)
        {
            var ns = __instance.GetComponent<NicknameSync>();
            if (ns.isLocalPlayer)
            {
                Logger.Info("Loaded Player!!");
                __instance.gameObject.AddComponent<LocalPlayer>();
            }
            return true;
        }
    }
}