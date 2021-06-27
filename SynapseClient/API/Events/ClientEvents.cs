using UnityEngine.SceneManagement;
using SynapseClient.Components;

namespace SynapseClient.API.Events
{
    public partial class SynapseEvents
    {
        public static event ClientEvent<CreditsHook> OnCreateCreditsEvent;
        public static event ClientEvent<Scene> OnSceneLoad;
    }
}
