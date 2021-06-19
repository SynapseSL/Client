using System.Collections.Generic;
using System.Threading;
using HarmonyLib;
using MelonLoader.Support;
using SynapseClient.API;
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