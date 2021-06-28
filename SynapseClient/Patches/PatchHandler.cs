using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SynapseClient.Patches
{
    public class PatchHandler
    {
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
            foreach (var type in DefaultTypesToPatch)
                PatchType(type);
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
