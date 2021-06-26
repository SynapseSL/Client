using System;
using HarmonyLib;

namespace SynapseClient.Patches
{
    public class ReportPatches
    {
        [HarmonyPatch(typeof(CheaterReport),nameof(CheaterReport.CmdReport))]
        [HarmonyPrefix]
        public bool OnReport(CheaterReport __instance,int playerId, string reason, bool notifyGm)
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
