using HarmonyLib;
using Mirror;
using RemoteAdmin;
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
        public static bool OnConsolePrint(GameConsoleTransmission __instance, NetworkConnection connection, Il2CppStructArray<byte> data, bool encrypted)
        {
            if (data == null) return false;
            if (encrypted || !DataUtils.isData(data)) return true;
            ClientPipeline.receive(DataUtils.unpack(data));
            return false;
        }
    }
}