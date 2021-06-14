using System;
using MelonLoader.Support;
using UnityEngine;

namespace SynapseClient
{
    public class SynapseMenuWorker : MonoBehaviour
    {
        public SynapseMenuWorker(IntPtr intPtr) : base(intPtr) {}

        //ReferenceHub.LocalHub.nicknameSync.UpdateNickname("Helight");
        public void Update()
        {
            SynapseClient.DoQueueTick();
            Coroutines.Process();
        }

        public void FixedUpdate()
        {
            Coroutines.ProcessWaitForFixedUpdate();
        }

        public void LateUpdate()
        {
            Coroutines.ProcessWaitForEndOfFrame();
        }

        public void OnEnable()
        {
            
        }
    }
}