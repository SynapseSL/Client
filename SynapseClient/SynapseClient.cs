using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.IL2CPP;
using DnsClient;
using DnsClient.Protocol;
using HarmonyLib;
using Jwt;
using MelonLoader;
using RemoteAdmin;
using Steamworks;
using Synapse.Client;
using SynapseClient.API;
using SynapseClient.Patches;
using SynapseClient.Pipeline;
using SynapseClient.Pipeline.Packets;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Action = System.Action;
using Console = GameCore.Console;
using Encoding = Il2CppSystem.Text.Encoding;
using Random = System.Random;
using String = Il2CppSystem.String;
using Exception = System.Exception;
using Object = UnityEngine.Object;

namespace SynapseClient
{
    [BepInPlugin("xyz.synapse.client.plugin", "SynapseClient", "0.0.0.1")]
    public class SynapseClientPlugin : BasePlugin
    {

        public static string name = "SynapsePlayer";

        public MedkitBundle medkitBundle = new MedkitBundle();
        public EnvironmentBundle environmentBundle = new EnvironmentBundle();
        public SynapseServerList SynapseServerList = new SynapseServerList();
        
        public static Texture2D synapseLogo;

        public static SynapseClientPlugin Singleton;

        public static bool isLoggedIn = false;

        private static Action _redirectCallback;
        public static Queue<Action> CallbackQueue { get; } = new Queue<Action>();

        [BundleDescriptor(Bundle = "firstaid.bundle", Source = "firstaid.bundle")]
        public class MedkitBundle : BundleEntity
        {
            [AssetDescriptor(Asset = "FirstAidKit_Green.prefab")]
            public GameObject aidKitPrefab;
        }
        
        [BundleDescriptor(Bundle = "environment.bundle", Source = "environment.bundle")]
        public class EnvironmentBundle : BundleEntity
        {
            [AssetDescriptor(Asset = "ExampleEnvironment.prefab")]
            public GameObject environmentPrefab;
        }

        [Blueprint(Name = "FirstAid")]
        public class MedkitSpawnHandler : SpawnHandler
        {
            public override GameObject Spawn(Vector3 pos, Quaternion rot, string name)
            {
                var obj = GameObject.Instantiate(Singleton.medkitBundle.aidKitPrefab, pos, rot);
                obj.name = name;
                return obj;
            }

            public override void Destroy(GameObject gameObject)
            {
                Object.Destroy(gameObject);
            }
        }

        [Blueprint(Name = "Environment")]
        public class EnvironmentSpawnHandler : SpawnHandler
        {
            public override GameObject Spawn(Vector3 pos, Quaternion rot, string name)
            {
                var obj = GameObject.Instantiate(Singleton.environmentBundle.environmentPrefab, pos, rot);
                obj.name = name;
                return obj;
            }

            public override void Destroy(GameObject gameObject)
            {
                Object.Destroy(gameObject);
            }
        }
        
        public SpawnController SpawnController { get; internal set; } = new SpawnController();

        public override void Load()
        {
            Singleton = this;
            Logger._logger = Log;
            Logger.Info("1");
            new ClientModLoader().LoadAll();
            Logger.Info("2");
            
            Logger.Info("Registering Types for Il2Cpp use...");
            UnhollowerSupport.Initialize();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseBackgroundWorker>();
            ClassInjector.RegisterTypeInIl2Cpp<SynapsePlayerHook>();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseSpawned>();
            Logger.Info("Loading Prefabs");
            if (!Directory.Exists("bundles")) Directory.CreateDirectory("bundles");
            medkitBundle.LoadBundle();
            environmentBundle.LoadBundle();
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
            SpawnController.Subscribe();
            SpawnController.Register(new MedkitSpawnHandler());
            SpawnController.Register(new EnvironmentSpawnHandler());
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
        
        public void MainReceivePipelineData(PipelinePacket packet)
        {
            switch (packet.PacketId)
            {
                case ConnectionSuccessfulPacket.ID:
                {
                    Logger.Info("WelcomePacket: " + packet.AsString());
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
                    Logger.Info("Init done");   
                    ClientPipeline.invoke(PipelinePacket.@from(1, "Client connected successfully"));
                    medkitBundle.LoadPrefabs();
                    environmentBundle.LoadPrefabs();
                    Logger.Info("Loaded Runtime Prefabs");
                    Events.InvokeConnectionSuccessful();
                    break;
                }
                case RoundStartPacket.ID:
                    Events.InvokeRoundStart();
                    break;
            }
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
                _redirectCallback.Invoke();
                _redirectCallback = null;
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
            Logger.Info($"Scene changed to {scene.name}");
            try
            {
                switch (scene.name)
                {
                    case "Facility":
                    {
                        //var gameObject = new GameObject();
                        //gameObject.AddComponent<SynapseBackgroundWorker>();
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
            
            Events.InvokeSceneLoad(scene);
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