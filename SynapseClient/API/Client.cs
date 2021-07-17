using System;
using System.Collections.Generic;
using System.Threading;
using SynapseClient.API.Events;
using SynapseClient.API.UI;
using SynapseClient.Components;
using SynapseClient.Patches;
using SynapseClient.Command;
using Console = GameCore.Console;

namespace SynapseClient.API
{
    public class Client
    {
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


        public void Connect(string address) => Console.singleton.TypeCommand("connect " + address);

        public void Disconnect() => Console.singleton.TypeCommand("disconnect");

        public void Redirect(string address)
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
