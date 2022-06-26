using System;
using System.IO;
using BepInEx;
using BepInEx.IL2CPP;
using CommandSystem.Commands.Shared;
using HarmonyLib;
using Neuron.Core;
using Neuron.Core.Platform;
using Neuron.Core.Scheduling;
using SynapseClient.Platform.Patches;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SynapseClient.Platform;

[BepInPlugin("xyz.synapse.client.plugin", "SynapseClient", "0.0.0.1")]
public class BepInExHook : BasePlugin
{
    public override void Load()
    {
        Logger.BeepInExLogger = Log;
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = $"\n=====ClientMod=====\nClient: Synapse";
        var patcher = new Harmony("Platform Patcher");
        patcher.PatchAll(typeof(AuthBasePatches));
        patcher.PatchAll(typeof(ServerListPatches));

        var platform = new SynapseClientPlatform();
        platform.Boostrap();
    }
}

public class NeuronCoroutineHook : MonoBehaviour
{
    public NeuronCoroutineHook(IntPtr intPtr) : base(intPtr) {}
        
    public void Update() => SynapseClientPlatform.Reactor.DirectTick();
}

public class DirectTickCoroutineReactor : CoroutineReactor
{
    public void DirectTick()
    {
        Tick();
    }
        
}
    
public class SynapseClientPlatform : Neuron.Core.Platform.IPlatform
{
    public PlatformConfiguration Configuration { get; set; } = new PlatformConfiguration();
    public DirectTickCoroutineReactor CoroutineReactor = Reactor;
    public NeuronBase NeuronBase { get; set; }

    public static DirectTickCoroutineReactor Reactor = new DirectTickCoroutineReactor();
        
    public void Load()
    {   
        Configuration.BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Synapse");
        Configuration.FileIo = true;
        Configuration.CoroutineReactor = CoroutineReactor;
        Configuration.OverrideConsoleEncoding = true;
        Configuration.EnableConsoleLogging = true;
    }

    public void Enable()
    {
        Logger.Error("Enable!");
        SceneManager.add_sceneLoaded(new Action<Scene, LoadSceneMode>(OnSceneLoaded));
        ClassInjector.RegisterTypeInIl2Cpp<NeuronCoroutineHook>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var current = GameObject.Find("NeuronCoroutineHook");
        if (current != null) return;
        var gameObject = new GameObject("NeuronCoroutineHook");
        Object.DontDestroyOnLoad(gameObject);
        var coroutine = gameObject.AddComponent<NeuronCoroutineHook>();
    }
        

    public void Continue()
    {
        
    }

    public void Disable()
    {
        
    }
}