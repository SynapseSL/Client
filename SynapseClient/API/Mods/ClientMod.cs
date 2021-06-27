using System;

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
        public string Name { get; set; }
        public string Version { get; set; }
        public bool ActivatedByServer { get; set; } = false;
    }
}