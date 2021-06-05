using Il2CppSystem;
using Il2CppSystem.Collections.Generic;

namespace SynapseClient.Patches
{
    public class LeakingObject<K> : Il2CppSystem.Object
    {
        private List<Object> List = new Il2CppSystem.Collections.Generic.List<Object>();

        public K decorated;

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