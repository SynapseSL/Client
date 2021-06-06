using System;
using System.Net;
using System.Reflection;
using Il2CppSystem.IO;

namespace UnityEngine
{
    public abstract class BundleEntity
    {
        public Il2CppAssetBundle bundle;

        public void LoadBundle()
        {
            if (bundle != null)
            {
                SynapseClient.Logger.Error("Bundle already loaded");
                return;
            }
            var descriptor = GetType().GetCustomAttribute(typeof(BundleDescriptor)) as BundleDescriptor;
            var loc = "bundles/" + descriptor.Bundle;
            if (!System.IO.File.Exists(loc) && descriptor.Source != null)
            {
                var client = new WebClient();
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                client.DownloadFile("https://github.com/SynapseSL/ClientPackages/raw/main/" + descriptor.Source,loc);
                SynapseClient.Logger.Info($"Downloaded bundle via GitHub {descriptor.Source}");
            }
            else
            {
                SynapseClient.Logger.Error($"Bundle {descriptor.Bundle} not found!");
                return;
            }
            var stream = File.OpenRead(descriptor.Bundle);
            bundle = Il2CppAssetBundleManager.LoadFromStream(stream);
        }

        public void LoadPrefabs()
        {
            foreach (var field in GetType().GetFields())
            {
                var ad = field.GetCustomAttribute(typeof(AssetDescriptor)) as AssetDescriptor;
                if (ad == null) continue;
                field.SetValue(this, bundle.LoadAsset<GameObject>(ad.Asset));
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BundleDescriptor : Attribute
    {
        public string Bundle { get; set; }
        public string Source { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetDescriptor : Attribute
    {
        public string Asset { get; set; }
    }
}