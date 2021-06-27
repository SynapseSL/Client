namespace SynapseClient.API.Events
{
    public partial class SynapseEvents
    {
        public delegate void ClientEvent<in TEvent>(TEvent ev);
        public delegate void ClientEvent();
    }
}
