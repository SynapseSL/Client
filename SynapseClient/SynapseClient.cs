using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Authenticator;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using DnsClient;
using DnsClient.Protocol;
using GameCore;
using HarmonyLib;
using Il2CppMono.Unity;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Jwt;
using LiteNetLib.Utils;
using MEC;
using MelonLoader;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using RemoteAdmin;
using Steamworks;
using Steamworks.Data;
using SynapseClient.Patches;
using SynapseClient.Pipeline;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils.ConfigHandler;
using Action = System.Action;
using Activator = Il2CppSystem.Activator;
using Byte = System.Byte;
using Console = GameCore.Console;
using DateTimeOffset = System.DateTimeOffset;
using Encoding = Il2CppSystem.Text.Encoding;
using IntPtr = Il2CppSystem.IntPtr;
using Logger = UnityEngine.Logger;
using Process = Il2CppSystem.Diagnostics.Process;
using Random = System.Random;
using String = Il2CppSystem.String;
using Exception = System.Exception;

namespace SynapseClient
{
    [BepInPlugin("xyz.synapse.client.plugin", "SynapseClient", "0.0.0.1")]
    public class SynapseClientPlugin : BasePlugin
    {

        public static string name = "SynapsePlayer";

        public static GameObject aidkit;
        public static Texture2D synapseLogo;

        public static bool isLoggedIn = false;

        private static Action _redirectCallback;
        public static Queue<Action> CallbackQueue { get; } = new Queue<Action>();
        public static string currentScene;

        public override void Load()
        {
            Logger._logger = Log;
            Logger.Info("Registering Types for Il2Cpp use...");
            UnhollowerSupport.Initialize();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseBackgroundWorker>();
            ClassInjector.RegisterTypeInIl2Cpp<SynapsePlayerHook>();
            Logger.Info("Loading Prefabs");
            var bytes = File.ReadAllBytes("firstaid.bundle");
            aidkit = Il2CppAssetBundleManager.LoadFromMemory(bytes).LoadAsset<GameObject>("FirstAidKit_Green.prefab");
            aidkit.AddComponent<Rigidbody>();
            Logger.Info("Patching client...");
            Harmony.CreateAndPatchAll(typeof(SynapseClientPlugin));
            Harmony.CreateAndPatchAll(typeof(AuthPatches));
            Harmony.CreateAndPatchAll(typeof(PipelinePatches));
            Harmony.CreateAndPatchAll(typeof(ServerListPatches));
            Harmony.CreateAndPatchAll(typeof(CommandLinePatch));
            Logger.Info("All patches applied!");
            SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
            Logger.Info("Registered Scene Loaded Listener");
            /*
            PlayerPrefsSl.add_SettingsRefreshed(new System.Action(t));
            Logger.Info("Registered Settings Refresh Listener");
            */
            ClientPipeline.DataReceivedEvent += MainReceivePipelineData;
        }

        internal static void DoQueueTick()
        {
            for (int i = 0; i < CallbackQueue.Count; i++)
            {
                CallbackQueue.Dequeue().Invoke();
            }
        }

        public static string GetConnection(string address)
        {
            var possibleSrv = ResolveSrvDomainOrNull($"_syn._udp.{address}").GetAwaiter().GetResult();
            if (possibleSrv == null)
            {
                Logger.Info("No SRV Records found");
                return address;
            }

            var target = possibleSrv.Target.Value;
            var targetAddress = target.Substring(0, target.Length - 1);
            return $"{targetAddress}:{possibleSrv.Port}";
        }
        
        public static async Task<SrvRecord> ResolveSrvDomainOrNull(string s)
        {
            try
            {
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(s, QueryType.SRV);
                var srvRecords = result.Answers.SrvRecords().ToArray();
                var sumWeight = srvRecords.Sum(x => x.Weight);
                var cur = new Random().Next(1, sumWeight + 1);
                foreach (var srv in srvRecords)
                {
                    if (cur <= srv.Weight)
                    {
                        return srv;
                    }

                    cur -= srv.Weight;
                }

                return srvRecords.FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());
                return null;
            }
        }
        
        public void MainReceivePipelineData(byte[] data)
        {
            Logger.Info("Initialising after Spawn");
            var salt = new byte[32];
            for (var i = 0; i < 32; i++) salt[i] = 0x00;
            var jwt = JsonWebToken.DecodeToObject<ClientConnectionData>(AuthPatches.synapseSessionToken, "", false);
            var sessionBytes = Encoding.UTF8.GetBytes(jwt.session);
            var key = new byte[32];
            for (var i = 0; i < 24; i++) key[i] = sessionBytes[i];
            for (var i = 24; i < 32; i++) key[i] = 0x00;

            QueryProcessor.Localplayer.Key = key;
            QueryProcessor.Localplayer.CryptoManager.ExchangeRequested = true;
            QueryProcessor.Localplayer.CryptoManager.EncryptionKey = key;
            QueryProcessor.Localplayer.Salt = salt;
            QueryProcessor.Localplayer.ClientSalt = salt;
            Logger.Info("Initialised QueryAuth");
        }

        public static void Connect(String address)
        {
            GameCore.Console.singleton.TypeCommand("connect " + address);
        }

        public static void Redirect(String address)
        {
            GameCore.Console.singleton.TypeCommand("disconnect");
            _redirectCallback = delegate
            {
                CallbackQueue.Enqueue(
                    delegate
                    {
                        Logger.Info("Trying to connect again");
                        Thread.Sleep(500);
                        Console.singleton.TypeCommand("connect " + address);
                    });
            };
   
        }
        
        [HarmonyPatch(typeof(NewMainMenu), nameof(NewMainMenu.Start))]
        [HarmonyPrefix]
        public static bool OnMainMenuStart(NewMainMenu __instance)
        {
            Logger.Info("Main Menu hooked!");
            var obj = new GameObject();
            obj.AddComponent<SynapseBackgroundWorker>();
            
            var texture = new Texture2D(256, 256);
            ImageConversion.LoadImage(texture, File.ReadAllBytes("synapse.png"), false);
            GameObject.Find("Canvas/Logo").GetComponent<RawImage>().texture = texture;

            if (_redirectCallback != null)
            {
                SynapseClientPlugin._redirectCallback.Invoke();
                SynapseClientPlugin._redirectCallback = null;
            }
            
            return true;
        }
        
        [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.Start))]
        [HarmonyPrefix]
        public static bool StartNicknameSync(ServerRoles __instance)
        {
            Logger.Info("Loaded Player!!");
            if (__instance.gameObject.GetComponent<SynapsePlayerHook>() == null)
            {
                __instance.gameObject.AddComponent<SynapsePlayerHook>();
            }
            return true;
        }
        

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentScene = scene.name;
            Logger.Info($"Scene changed to {scene.name}");
            try
            {
                switch (scene.name)
                {
                    case "Facility":
                    {
                        var gameObject = new GameObject();
                        gameObject.AddComponent<SynapseBackgroundWorker>();
                        break;
                    }
                    
                    case "Loader":
                    {
                        if (SteamClient.IsLoggedOn)
                        {
                            name = SteamClient.Name;
                            Logger.Info($"Changed current prefered name to {name}");
                        }

                        ConnectCentralServer();
                        break;
                    }

                    case "NewGameMenu":
                    {
                        Logger.Info("Changed to Game-Menu");
                        CentralAuthManager.InitAuth();
                        CentralAuthManager.Authentication();
                        break;
                    }

                    default: break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        private void ConnectCentralServer()
        {
            Logger.Info("Connecting to Synapse Central-Server");
            try
            {
                if (File.Exists("user.dat") && File.Exists("certificate.dat"))
                {
                    //Logged in
                    Log.LogInfo(File.ReadAllText("certificate.dat"));
                    isLoggedIn = true;
                }
                else if (File.Exists("user.dat"))
                {
                    SynapseCentralAuth.Certificate();
                    isLoggedIn = true;
                }
                else
                {
                    isLoggedIn = false;
                    var thread = new Thread(DoRegisterAsync);
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public void DoRegisterAsync()
        {
            try
            {
                SynapseCentralAuth.Register();
                SynapseCentralAuth.Certificate();
                isLoggedIn = true;
                Logger.Info("Central-Authentication / Registration complete");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
    }

    public class ClientConnectionData
    {
        //JWT Subject == Name
        public string sub { get; set; }
        //JWT Audience == Connected Server
        public string aud { get; set; }
        //JWT Issuer == Most likely Synapse
        public string iss { get; set; }
        public string uuid { get; set; }
        public string session { get; set; }
    }
}