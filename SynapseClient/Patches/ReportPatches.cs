using System;
using HarmonyLib;
using SynapseClient.API;

namespace SynapseClient.Patches
{
    public static class ReportPatches
    {
        [HarmonyPatch(typeof(CheaterReport),nameof(CheaterReport.Report))]
        [HarmonyPrefix]
        public static bool OnReport(int playerId, string reason, bool notifyGm)
        {
            if (!notifyGm) return true;

            try
            {
                Logger.Info($"Reporting {playerId} for {reason}");
                var ply = ReferenceHub.GetHub(playerId);
                Report(ply.characterClassManager.UserId, reason);
            }
            catch(Exception e)
            {
                Logger.Error("Synapse-Report: Global Report failed:\n" + e);
            }
            return false;
        }

        public static async void Report(string id,string reason)
        {
            await System.Threading.Tasks.Task.Delay(3000);
            try
            {
                await SynapseCentral.Get.Report(id, reason);
            }
            catch
            {
                //ply.GetComponent<PlayerList>().ShowReportResponse("Report failed");
            }

            //This will crash the Client but something like this is needed
            //ply.GetComponent<PlayerList>().ShowReportResponse("Player Reported to Synapse moderation");
            //GameCore.Console.AddLog("[REPORT] Success",UnityEngine.Color.white);
        }
    }
}
