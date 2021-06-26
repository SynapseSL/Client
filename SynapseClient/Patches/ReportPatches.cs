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
                Logger.Info($"Start Report of {playerId} for {reason}");
            }
            catch(Exception e)
            {
                Logger.Error("Synapse-Report: Global Report failed:\n" + e);
            }
            return false;
        }
    }
}
