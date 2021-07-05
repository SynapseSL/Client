using SynapseClient.Components;
using UnityEngine.SceneManagement;

namespace SynapseClient.API.Events
{
    public partial class SynapseEvents
    {
        /// <summary>
        /// Invokes when starting the Menu to load the Credits
        /// </summary>
        public static event ClientEvent<CreditsHook> OnCreateCreditsEvent;

        /// <summary>
        /// Invokes when a new Scene is loaded
        /// </summary>
        public static event ClientEvent<Scene> OnSceneLoad;

        /// <summary>
        /// Invokes when the Menu is loaded
        /// </summary>
        public static event ClientEvent OnMenuLoaded;
    }
}
