using HarmonyLib;
using Mono.Cecil;
using Neuron.Core.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.ClientModule.Patches;

[Patches]
public class NewsPatch
{
    [HarmonyPatch(typeof(NewsLoader),nameof(NewsLoader.Start)), HarmonyPrefix]
    public static bool OnRequest(NewsLoader __instance)
    {
        //var parentObj = GameObject.Find("New Main Menu/News/Thumbnail/Scroll View/Viewport/Content");

        NeuronLogger.For<NewsPatch>().Verbose("Newspatch prefix entrypoint");
        __instance._announcements = new Il2CppSystem.Collections.Generic.List<NewsLoader.Announcement>();

        __instance._announcements.Add(new NewsLoader.Announcement(
            $"Synapse Client 1.0.0.0", 
            "<b><size=20>Welcome to the beta version of our modded client for SCP:SL</size></b>", 
            "06.21.2022", 
            "https://synapsesl.xyz",
            null
            )
        );
        
        __instance.ShowAnnouncement(0);
        return false;
    }
}