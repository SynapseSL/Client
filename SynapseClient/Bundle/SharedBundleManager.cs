using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public class SharedBundleManager
    {
        public static SharedBundleManager Singleton { get; } = new SharedBundleManager();
        
        public Dictionary<string, BundleEntity> Bundles { get; } = new Dictionary<string, BundleEntity>();

        public T Get<T>(string bundle, string path)
        {
            var b = Bundles[bundle];
            return (T) b.GetEntries().First(x => x.Path == path).Reference;
        }
        
        public T Get<T>(string key)
        {
            var ks = key.Split(':');
            return Get<T>(ks[0], ks[1]);
        }

        public IEnumerable<AssetEntry> All()
        {
            var list = new List<AssetEntry>();

            foreach (var pair in Bundles)
            {
                try
                {
                    list.AddRange(pair.Value.GetEntries());
                }
                catch
                {
                    //ignored
                }
            }
            
            return list;
        }

        public static void LogLoaded()
        {
            SynapseClient.Logger.Info("Currently loaded assets: ");
            foreach (var asset in Singleton.All())
            {
                SynapseClient.Logger.Info("» " + asset);
            }
        }
    }
}