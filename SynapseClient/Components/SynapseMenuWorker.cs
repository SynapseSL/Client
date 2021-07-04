using System;
using MelonLoader.Support;
using SynapseClient.API;
using UnityEngine;

namespace SynapseClient.Components
{
    public class SynapseMenuWorker : MonoBehaviour
    {
        public SynapseMenuWorker(IntPtr intPtr) : base(intPtr) {}

        //ReferenceHub.LocalHub.nicknameSync.UpdateNickname("Helight");
        public void Update()
        {
            try
            {
                Client.Get.DoQueueTick();
                Coroutines.Process();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void FixedUpdate()
        {
            try
            {
                Coroutines.ProcessWaitForFixedUpdate();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void LateUpdate()
        {
            try
            {
                Coroutines.ProcessWaitForEndOfFrame();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void OnEnable()
        {
            
        }
    }
}