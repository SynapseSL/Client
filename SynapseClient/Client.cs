using System.Collections;
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
using Il2CppSystem;
using Jwt;
using MelonLoader;
using MelonLoader.Support;
using RemoteAdmin;
using Steamworks;
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
    public class Client : BasePlugin
    {

        public static string name = "SynapsePlayer";
        
        public static Client Singleton;
        
        public static bool isLoggedIn = false;
        
        internal static Texture2D synapseLogo;

        internal static Action _redirectCallback;
        
        public SynapseServerList SynapseServerList = new SynapseServerList();

        public static Queue<Action> CallbackQueue { get; } = new Queue<Action>();

        public SpawnController SpawnController { get; internal set; } = new SpawnController();

        public static string CentralServer = "http://localhost:8080";
        public static string ServerListServer = "https://servers.synapsesl.xyz";

        public override void Load()
        {
            Singleton = this;
            Logger._logger = Log;
            Logger.Info("Loading Mods");
            new ClientModLoader().LoadAll();
            Logger.Info("Loaded Mods");
            
            Logger.Info("Registering Types for Il2Cpp use...");
            UnhollowerSupport.Initialize();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseMenuWorker>();
            ClassInjector.RegisterTypeInIl2Cpp<LocalPlayer>();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseSpawned>();
            ClassInjector.RegisterTypeInIl2Cpp<LookReceiver>();
            ClassInjector.RegisterTypeInIl2Cpp<CreditsHook>();
            Logger.Info("Loading Prefabs");
            if (!Directory.Exists("bundles")) Directory.CreateDirectory("bundles");
            Logger.Info("Patching client...");
            Harmony.CreateAndPatchAll(typeof(SmallPatches));
            Harmony.CreateAndPatchAll(typeof(AuthPatches));
            Harmony.CreateAndPatchAll(typeof(PipelinePatches));
            Harmony.CreateAndPatchAll(typeof(ServerListPatches));
            Harmony.CreateAndPatchAll(typeof(CommandLinePatch));
            Harmony.CreateAndPatchAll(typeof(CreditsHookPatch));
            Harmony.CreateAndPatchAll(typeof(GlobalPermissionPatches));
            Logger.Info("All patches applied!");
            SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));
            Logger.Info("Registered Scene Loaded Listener");
            /*
            PlayerPrefsSl.add_SettingsRefreshed(new System.Action(t));
            Logger.Info("Registered Settings Refresh Listener");
            */

            Logger.Info("====================");
            try
            {
                ClientPipeline.DataReceivedEvent += MainReceivePipelineData;
                SpawnController.Subscribe();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            
            Events.OnCreateCreditsEvent += delegate(CreditsHook ev)
            {
                // Synapse Client Credits
                ev.CreateCreditsCategory("Synapse Client");
                ev.CreateCreditsEntry("Helight", "Maintainer", "Synapse Client", CreditColors.Red600);
                ev.CreateCreditsEntry("Wholesome", "Developer", "Synapse Client", CreditColors.Blue100);
                ev.CreateCreditsEntry("Dimenzio", "Developer", "Synapse Client", CreditColors.Blue100);
                ev.CreateCreditsEntry("Mika", "Developer", "Synapse Client", CreditColors.Blue100);
                ev.CreateCreditsEntry("Cubuzz", "Developer", "Synapse Client", CreditColors.Blue100);
                ev.CreateCreditsEntry("Flo0205", "Developer", "Synapse Client", CreditColors.Blue100);
                
                // Synapse Server Credits
                ev.CreateCreditsCategory(("Synapse Server"));
                ev.CreateCreditsEntry("Dimenzio", "Creator, Maintainer", "Synapse Server", CreditColors.Red600);
                ev.CreateCreditsEntry("Helight", "Maintainer", "Synapse Server", CreditColors.Red600);
                ev.CreateCreditsEntry("MineTech13", "NuGet-Maintainer", "Synapse Server", CreditColors.Yellow300);
                ev.CreateCreditsEntry("moelrobi", "Former-Maintainer", "Synapse Server", CreditColors.Gray);
                ev.CreateCreditsEntry("Mika", "Contributor", "Synapse Server", CreditColors.Blue100);
                ev.CreateCreditsEntry("AlmightyLks", "Contributor", "Synapse Server", CreditColors.Blue100);
                ev.CreateCreditsEntry("TheVoidNebula", "Contributor", "Synapse Server", CreditColors.Blue100);
                ev.CreateCreditsEntry("PintTheDragon", "Contributor", "Synapse Server", CreditColors.Blue100);

            };
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
                    ClientPipeline.invoke(PipelinePacket.@from(1, "Client connected successfully"));
                    Events.InvokeConnectionSuccessful();
                    SharedBundleManager.LogLoaded();
                    break;
                }
                case RoundStartPacket.ID:
                    Events.InvokeRoundStart();
                    break;
                case RedirectPacket.ID:
                    RedirectPacket.Decode(packet, out var target);
                    Redirect(target);
                    break;
            }
        }

        public static object StartCoroutine(IEnumerator enumerator)
        {
            return Coroutines.Start(enumerator);
        }

        public static void StopCoroutine(object token)
        {
            Coroutines.Stop((IEnumerator)token);
        }
        
        public static void Connect(String address)
        {
            Console.singleton.TypeCommand("connect " + address);
        }

        public static void Redirect(String address)
        {
            Console.singleton.TypeCommand("disconnect");
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

                        SynapseCentral.ConnectCentralServer();
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

        public static string ApplicationDataDir()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse Client");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}