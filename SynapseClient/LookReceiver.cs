﻿using System;
using UnityEngine;

namespace SynapseClient
{
    public class LookReceiver : MonoBehaviour
    {

        public LookReceiver(IntPtr intPtr) : base(intPtr) {}

        public Action<Vector3> LookReceiveAction { get; set; } = delegate {  };
        
    }
}