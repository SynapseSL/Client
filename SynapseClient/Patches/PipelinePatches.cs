using HarmonyLib;
using SynapseClient.Pipeline;
using UnhollowerBaseLib;

namespace SynapseClient.Patches
{
    public class PipelinePatches
    {
        public static GameConsoleTransmission Transmission = null;

        [HarmonyPatch(typeof(GameConsoleTransmission), nameof(GameConsoleTransmission.Start))]
        [HarmonyPrefix]
        public static bool OnStart(GameConsoleTransmission __instance)
        {
            Transmission = __instance;
            return true;
        }
        
        
        [HarmonyPatch(typeof(GameConsoleTransmission), nameof(GameConsoleTransmission.CallTargetPrintOnConsole))]
        [HarmonyPrefix]
        public static bool OnConsolePrint(Il2CppStructArray<byte> data, bool encrypted)
        {
            if (data == null) return false;
            if (encrypted || !DataUtils.IsData(data)) return true;
            ClientPipeline.Receive(DataUtils.Unpack(data));
            return false;
        }
    }
}