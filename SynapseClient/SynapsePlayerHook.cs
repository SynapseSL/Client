using System;
using SynapseClient.Patches;
using UnityEngine;

namespace SynapseClient
{ 
    
    public class SynapsePlayerHook : MonoBehaviour
    {
        public static int IncrementalCounter = 0;
        public static SynapsePlayerHook Singleton;
        
        public SynapsePlayerHook(IntPtr intPtr) : base(intPtr) {}


        public void Awake()
        {
            Singleton = this;
            IncrementalCounter += 1;
            
            Logger.Info("1");
            
            
            if (ReferenceHub.LocalHub.gameObject.name == gameObject.name)
            {
                Logger.Info("Initialised as LocalPlayers");
                Init();
            }
            else
            {
                Logger.Info("Initialised as foreign Player");
            }
            
            Logger.Info("2");
        }

        public void Init()
        {
            var roles = GetComponent<ServerRoles>();
            roles.CmdServerSignatureComplete("synapse-client-authentication", AuthPatches.synapseSessionToken, "", false);
        }
    }
}