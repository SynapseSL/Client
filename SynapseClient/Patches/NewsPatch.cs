using HarmonyLib;

namespace SynapseClient.Patches
{
    [HarmonyPatch(typeof(NewsLoader),nameof(NewsLoader.Start))]
    public static class NewsPatch
    {
        [HarmonyPrefix]
        public static bool OnRequest(NewsLoader __instance)
        {
            Logger.Info("NEWSPATCH");
            Logger.Info(__instance._curAnncUrl);

            __instance._announcements = new Il2CppSystem.Collections.Generic.List<NewsLoader.Announcement>();
            __instance._announcements.Add(new NewsLoader.Announcement("Synapse Client", "Welcome to the beta version of our modded client for SCP:SL\nThe synapse client modification comes with a custom central server and server list therefore can you play on servers with client mods without the fear of being global banned by Northwood!\nBut Hack Client are still not allowed an we will ban you if we notice it\n\nOur official Client Mods:\n<color=blue>AmbienceSL</color>", "", "https://synapsesl.xyz", null));
            __instance.ShowAnnouncement(0);

            return false;
        }
    }
}
