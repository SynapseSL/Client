using System;
using System.Collections.Generic;
using HarmonyLib;
using SynapseClient.API;

namespace SynapseClient.Patches
{
    public class PatchHandler
    {
        public static PatchHandler Get => Client.Get.Patcher;

        internal PatchHandler() { }

        public List<Type> PatchedTypes { get; } = new List<Type>();

        public List<Type> DefaultTypesToPatch { get; } = new List<Type>
        {
            typeof(SmallPatches),
            typeof(AuthPatches),
            typeof(PipelinePatches),
            typeof(ServerListPatches),
            typeof(CommandLinePatch),
            typeof(CreditsHookPatch),
            typeof(GlobalPermissionPatches),
            typeof(ReportPatches)
        };

        public void PatchAll()
        {
            Logger.Info("Patching client...");

            foreach (var type in DefaultTypesToPatch)
                PatchType(type);

            Logger.Info("All patches applied!");
        }

        public void PatchType(Type type)
        {
            try
            {
                Harmony.CreateAndPatchAll(type);
                PatchedTypes.Add(type);
            }
            catch(Exception e)
            {
                Logger.Error($"Patching of {type} failed:\n" + e);
            }
        }
    }
}
