﻿using System.Collections;
using MelonLoader.Support;
using UnityEngine;

namespace SynapseClient.API
{
    public class SynapseCoroutine
    {
        private SynapseCoroutine(object cor) => handle = cor;

        public static SynapseCoroutine StartCoroutine(IEnumerator enumerator) => new SynapseCoroutine(Coroutines.Start(enumerator));

        public static void StopCoroutine(SynapseCoroutine coroutine) => Coroutines.Stop((IEnumerator)coroutine.handle);

        public static WaitForSeconds WaitForSeconds(float time) => new WaitForSeconds(time);

        private readonly object handle;
    }
}
