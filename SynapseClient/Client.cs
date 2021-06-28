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
using SynapseClient.API.Events;
using SynapseClient.API.Mods;
using SynapseClient.Components;
using SynapseClient.Models;
using SynapseClient.Patches;
using SynapseClient.Pipeline;
using SynapseClient.Pipeline.Packets;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Action = System.Action;
using Console = GameCore.Console;
using Encoding = Il2CppSystem.Text.Encoding;
using Exception = System.Exception;
using Random = System.Random;
using String = Il2CppSystem.String;

namespace SynapseClient
{
    [BepInPlugin("xyz.synapse.client.plugin", "SynapseClient", "0.0.0.1")]
    public class Client : BasePlugin
    {
        public const string ClientVersion = "1.0.0";
        public const int ClientMajor = 1;
        public const int ClientMinor = 0;
        public const int ClientPatch = 0;

        public static string CentralServer = "https://central.synapsesl.xyz";
        public static string ServerListServer = "https://servers.synapsesl.xyz";

        public static string name = "SynapsePlayer";
        
        public static Client Get { get; private set; }

        public SynapseServerList SynapseServerList { get; } = new SynapseServerList();

        public SynapseCentral Central { get; } = new SynapseCentral();

        public PatchHandler Patcher { get; } = new PatchHandler();

        public API.Events.EventHandlers EventHandlers { get; } = new EventHandlers();

        public Computer Computer { get; } = new Computer();

        public static bool IsLoggedIn { get; set; } = false;

        internal static Action _redirectCallback;

        public static Queue<Action> CallbackQueue { get; } = new Queue<Action>();

        public SpawnController SpawnController { get; internal set; } = new SpawnController();

        public ClientModLoader ModLoader { get; } = new ClientModLoader();

        public override void Load()
        {
            Get = this;
            Logger._logger = Log;

            try
            {
                ModLoader.LoadAll();
                ModLoader.EnableAll();

                Logger.Info("Registering Types for Il2Cpp use...");
                UnhollowerSupport.Initialize();
                ClassInjector.RegisterTypeInIl2Cpp<SynapseMenuWorker>();
                ClassInjector.RegisterTypeInIl2Cpp<LocalPlayer>();
                ClassInjector.RegisterTypeInIl2Cpp<SynapseSpawned>();
                ClassInjector.RegisterTypeInIl2Cpp<LookReceiver>();
                ClassInjector.RegisterTypeInIl2Cpp<CreditsHook>();

                Logger.Info("Loading Prefabs");
                if (!Directory.Exists("bundles")) Directory.CreateDirectory("bundles");

                Patcher.PatchAll();

                /*
                PlayerPrefsSl.add_SettingsRefreshed(new System.Action(t));
                Logger.Info("Registered Settings Refresh Listener");
                */

                Logger.Info("====================");
                SpawnController.Subscribe();
            }
            catch(Exception e)
            {
                Logger.Error("SynapseClient: Startup failed:\n" + e);
            }
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
    }
}