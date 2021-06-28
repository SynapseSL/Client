using System;

//Change this to SynapseClient.API.Mods later with Ambience SL
namespace SynapseClient.API
{
    public abstract class ClientMod
    {
        public virtual void Enable()
        {
            
        }

        public virtual void Disable()
        {
            
        }

        public virtual void ActivateForServer()
        {
            
        }

        public virtual void ServerConnectionEnd()
        {
            
        }
    }

    public class ClientModDetails : Attribute
    {
        /// <summary>
        /// The Name of the ClientMod
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Version of the ClientMod
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// If the ClientMod should be activated by a server
        /// </summary>
        public bool ActivatedByServer { get; set; } = false;
    }
}