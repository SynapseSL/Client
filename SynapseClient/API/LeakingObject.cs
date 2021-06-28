using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

namespace SynapseClient.API
{
    public class LeakingObject<T> : Il2CppSystem.Object
    {
        private List<Object> List = new Il2CppSystem.Collections.Generic.List<Object>();
        
        // Seriously @Dimenzio
        // Look at this, why did you refactor it?
        // V V V V V V V V
        public T decorated;

        /// <summary>
        /// Creates an self referencing object usable for integration in il2cpp domain
        /// Since I'm directly setting a field in an referenced Object, it's normally not collected
        /// ----
        /// You have more knowledge about this topic than me or a better idea?
        /// => Rant in the commit discussions and make a pr with a better solution.
        /// 
        /// Sincerely,
        /// your C++ Noob Helight
        /// ---- Edit 1:
        /// @Dimenzio you really needed to refactor this code up here did you? 
        /// </summary>
        public LeakingObject()
        {
            List.Add(this);
        }

        public void Dispose()
        {
            decorated = default;
            List.Clear();
            List = null;
        }
    }
}