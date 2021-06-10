using SynapseClient.Patches;
using UnityEngine.SceneManagement;

namespace SynapseClient.API
{
    public class Events
    {
        public static event NoArgsClientEvent OnRoundStart;
        public static event NoArgsClientEvent OnConnectionSuccessful;
        public static event ClientEvent<ServerConnectArgs> OnServerConnect;
        public static event ClientEvent<Scene> OnSceneLoad;

        internal static void InvokeRoundStart()
        {
            OnRoundStart?.Invoke();
        }
        
        internal static void InvokeConnectionSuccessful()
        {
            OnConnectionSuccessful?.Invoke();
        }
        
        internal static bool InvokeServerConnect(string server)
        {
            var args = new ServerConnectArgs
            {
                Server = server,
                Allow = true
            };
            OnServerConnect?.Invoke(args);
            return args.Allow;
        }
        
        internal static void InvokeSceneLoad(Scene scene)
        {
            OnSceneLoad?.Invoke(scene);
        }

        
        public delegate void ClientEvent<in TEvent>(TEvent ev);
        public delegate void NoArgsClientEvent();
        
        public class ServerConnectArgs
        {
            public string Server { get; set; }
            public bool Allow { get; set; }
        }
    }
}