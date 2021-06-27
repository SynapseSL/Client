using HarmonyLib;
using SynapseClient.Components;
using UnityEngine;

namespace SynapseClient.Patches
{
    public class CreditsHookPatch
    {
        [HarmonyPatch(typeof(NewCredits), nameof(NewCredits.OnEnable))]
        [HarmonyPrefix]
        public static bool InjectCreditsHook(NewCredits __instance)
        {
            GameObject creditsHookObject = new GameObject();
            creditsHookObject.name = "Credits Hook";
            creditsHookObject.AddComponent<CreditsHook>();
            
            return true;
        }
    }
}