using System;
using UnityEngine;

namespace SynapseClient
{
    public class SynapseBackgroundWorker : MonoBehaviour
    {
        public SynapseBackgroundWorker(IntPtr intPtr) : base(intPtr) {}

        //ReferenceHub.LocalHub.nicknameSync.UpdateNickname("Helight");
        public void Update()
        {
           
        }

        public void OnEnable()
        {
            
        }
    }
}