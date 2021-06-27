using SynapseClient.API.Events.EventArguments;

namespace SynapseClient.API.Events
{
    public partial class SynapseEvents
    {
        public static event ClientEvent OnRoundStart;
        public static event ClientEvent OnConnectionSuccessful;
        public static event ClientEvent OnRoundEnd;
        public static event ClientEvent<ServerConnectArgs> OnServerConnect;
    }
}
