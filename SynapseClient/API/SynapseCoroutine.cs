using System;
using System.Collections;
using MelonLoader.Support;
using UnityEngine;

namespace SynapseClient.API
{
    public class SynapseCoroutine
    {
        private SynapseCoroutine(object cor) => handle = cor;

        public static SynapseCoroutine StartCoroutine(IEnumerator enumerator) => new SynapseCoroutine(Coroutines.Start(enumerator));
        
        public static SynapseCoroutine Delay(Action action, TimeSpan delay) => new SynapseCoroutine(Coroutines.Start(_delay(action,delay)));
        
        public static SynapseCoroutine Synchronize(Action action) => new SynapseCoroutine(Coroutines.Start(_sync(action)));

        public static void StopCoroutine(SynapseCoroutine coroutine) => Coroutines.Stop((IEnumerator)coroutine.handle);

        private readonly object handle;

        private static IEnumerator _delay(Action action, TimeSpan span)
        {
            yield return new WaitForSeconds(Convert.ToSingle(span.TotalSeconds));
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static IEnumerator _sync(Action action)
        {
            yield return new WaitForSeconds(0);
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Logger.Info(e);
            }
        }
    }
}
