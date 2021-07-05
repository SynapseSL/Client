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
        
        public static SynapseCoroutine CallDelayed(float delay, Action action) => new SynapseCoroutine(Coroutines.Start(Delay(action,delay)));

        public static SynapseCoroutine CallDelayed(TimeSpan span, Action action) => new SynapseCoroutine(Coroutines.Start(Delay(action, span)));

        public static SynapseCoroutine Synchronize(Action action) => new SynapseCoroutine(Coroutines.Start(Sync(action)));

        public static void StopCoroutine(SynapseCoroutine coroutine) => Coroutines.Stop((IEnumerator)coroutine.handle);

        public static WaitForSeconds WaitForSeconds(float time) => new WaitForSeconds(time);



        private readonly object handle;



        private static IEnumerator Delay(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private static IEnumerator Delay(Action action, TimeSpan span)
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

        private static IEnumerator Sync(Action action)
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
