using MelonLoader;
using SynapseClient.API;
using UnhollowerRuntimeLib;

namespace SynapseClient.Components
{
    public class ComponentHandler
    {
        internal ComponentHandler() { }

        public static ComponentHandler Get => Client.Get.ComponentHandler;

        public void RegisterTypes()
        {
            Logger.Info("Registering Types for Il2Cpp use...");
            UnhollowerSupport.Initialize();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseMenuWorker>();
            ClassInjector.RegisterTypeInIl2Cpp<LocalPlayer>();
            ClassInjector.RegisterTypeInIl2Cpp<SynapseSpawned>();
            ClassInjector.RegisterTypeInIl2Cpp<LookReceiver>();
            ClassInjector.RegisterTypeInIl2Cpp<CreditsHook>();
        }
    }
}
