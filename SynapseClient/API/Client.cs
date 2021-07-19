using System;
using System.Collections.Generic;
using System.Threading;
using Mirror.LiteNetLib4Mirror;
using SynapseClient.API.Events;
using SynapseClient.API.UI;
using SynapseClient.Command;
using SynapseClient.Components;
using SynapseClient.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;
using Console = GameCore.Console;

namespace SynapseClient.API
{
    public class Client
    {
        private NewMainMenu MainMenu => UnityEngine.Object.FindObjectOfType<NewMainMenu>();

        public static Client Get => ClientBepInExPlugin.Get.Client;

        internal Client() { }

        #region API Controllers
        public SpawnController SpawnController { get; internal set; } = new SpawnController();

        public EventHandlers EventHandlers { get; } = new EventHandlers();

        public Computer Computer { get; } = new Computer();

        public SynapseServerList SynapseServerList { get; } = new SynapseServerList();

        public SynapseCentral Central { get; } = new SynapseCentral();

        public PatchHandler Patcher { get; } = new PatchHandler();

        public ComponentHandler ComponentHandler { get; } = new ComponentHandler();

        public UiManager UiManager { get; } = new UiManager();

        public SynapseCommandHandler CommandHandler { get; } = new SynapseCommandHandler();
        #endregion


        public string PlayerName { get; internal set; }

        public bool CredentialsValid { get; set; } = false;

        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        public string ServerIp => LiteNetLib4MirrorTransport.Singleton.clientAddress;

        public ushort ServerPort => LiteNetLib4MirrorTransport.Singleton.port;

        public bool IsConnected => CurrentSceneName == "Facility";

        public void QuitGame() => Application.Quit();

        public void Connect(string address)
        {
            if(!IsConnected)
                MainMenu.Connect(address);
        }

        public void Disconnect()
        {
            if(IsConnected)
            {
                Console.singleton.TypeCommand("disconnect");
                Console.singleton._clientCommandLogs.RemoveAt(Console.singleton._clientCommandLogs.Count - 1);
            }
        }

        public void Reconnect()
        {
            Disconnect();

            SynapseCoroutine.CallDelayed(0.5f, () => Connect(ServerIp + ":" + ServerPort));
        }

        public void Redirect(string address)
        {
            Disconnect();
            _redirectCallback = delegate
            {
                CallbackQueue.Enqueue(
                    delegate
                    {
                        Logger.Info("Trying to connect again");
                        Thread.Sleep(500);
                        Connect(address);
                    });
            };

        }
        internal Action _redirectCallback;
        public Queue<Action> CallbackQueue { get; } = new Queue<Action>();
        internal void DoQueueTick()
        {
            for (int i = 0; i < CallbackQueue.Count; i++)
            {
                CallbackQueue.Dequeue().Invoke();
            }
        }
    }
}
