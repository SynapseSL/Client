using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

namespace SynapseClient.Patches
{
    public class LeakingObject<K> : Il2CppSystem.Object
    {
        private List<Object> List = new Il2CppSystem.Collections.Generic.List<Object>();

        public K decorated;

        /// <summary>
        /// Creates an self referencing object usable for integration in il2cpp domain
        /// Since I'm directly setting a field in an referenced Object, it's normally not collected
        /// ----
        /// You have more knowledge about this topic than me or a better idea?
        /// => Rant in the commit commit discussions and make a pr with a better solution.
        /// 
        /// Sincerely,
        /// your C++ Noob Helight
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