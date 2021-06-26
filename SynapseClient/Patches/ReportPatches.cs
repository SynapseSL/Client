using System;
using HarmonyLib;

namespace SynapseClient.Patches
{
    public static class ReportPatches
    {
        [HarmonyPatch(typeof(CheaterReport),nameof(CheaterReport.Report))]
        [HarmonyPrefix]
        public static bool OnReport(CheaterReport __instance,int playerId, string reason, bool notifyGm)
        {
            if (!notifyGm) return true;

            try
            {
                Logger.Info($"Reporting {playerId} for {reason}");
                var ply = ReferenceHub.GetHub(playerId);
                SynapseCentral.Report(ply.characterClassManager.UserId, reason);

                //This is blocked but something like this must be implemented so that the Report Window shows that the Report was a success
                __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORT] Report success", "white");
            }
            catch(Exception e)
            {
                Logger.Error("Synapse-Report: Global Report failed:\n" + e);
            }
            return false;
        }
    }
}
