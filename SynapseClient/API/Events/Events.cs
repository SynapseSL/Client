namespace SynapseClient.API.Events
{
    /// <summary>
    /// The Main Class for all Events of the Synapse Client
    /// </summary>
    public partial class SynapseEvents
    {
        public delegate void ClientEvent<in TEvent>(TEvent ev);
        public delegate void ClientEvent();
    }
}
