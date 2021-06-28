using SynapseClient.API.Events.EventArguments;

namespace SynapseClient.API.Events
{
    public partial class SynapseEvents
    {
        /// <summary>
        /// Invokes when the player tries to connect to a server
        /// </summary>
        public static event ClientEvent<ServerConnectArgs> OnServerConnect;

        /// <summary>
        /// Invokes when the Connection to a Server was successfully established
        /// </summary>
        public static event ClientEvent OnConnectionSuccessful;

        /// <summary>
        /// Invokes when a new Round starts
        /// </summary>
        public static event ClientEvent OnRoundStart;

        /// <summary>
        /// Invokes when a Round Ends
        /// </summary>
        public static event ClientEvent OnRoundEnd;
    }
}
