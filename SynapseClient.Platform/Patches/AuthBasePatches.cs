using HarmonyLib;

namespace SynapseClient.Platform.Patches;

public class AuthBasePatches
{
               
    [HarmonyPatch(typeof(CentralAuthManager), nameof(CentralAuthManager.Sign))]
    [HarmonyPrefix]
    public static bool OnCentralAuthManagerSign(ref string __result, string ticket)
    {
        CentralAuthManager.Authenticated = true;
        __result = "TICKET";
        return false;
    }
}