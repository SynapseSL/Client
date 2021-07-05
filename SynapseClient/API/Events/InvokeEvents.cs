using SynapseClient.API.Events.EventArguments;
using UnityEngine.SceneManagement;
using SynapseClient.Components;

namespace SynapseClient.API.Events
{
    public partial class SynapseEvents
    {
        internal static void InvokeCreateCreditsEvent(CreditsHook hook) => OnCreateCreditsEvent?.Invoke(hook);

        internal static void InvokeRoundStart() => OnRoundStart?.Invoke();

        internal static void InvokeConnectionSuccessful() => OnConnectionSuccessful?.Invoke();

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

        internal static void InvokeSceneLoad(Scene scene) => OnSceneLoad?.Invoke(scene);

        internal static void InvokeRoundEnd() => OnRoundEnd?.Invoke();

        internal static void InvokeMenuLoaded() => OnMenuLoaded?.Invoke();
    }
}
