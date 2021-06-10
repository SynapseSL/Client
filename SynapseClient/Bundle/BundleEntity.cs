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
            var loc = Path.Combine("bundles", descriptor.Bundle);
            SynapseClient.Logger.Info(loc);
            if (!File.Exists(loc) && descriptor.Source != null)
            {
                var client = new WebClient();
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                client.DownloadFile("https://github.com/SynapseSL/ClientPackages/raw/main/" + descriptor.Source,loc);
                SynapseClient.Logger.Info($"Downloaded bundle via GitHub {descriptor.Source}");
            }
            else if (!File.Exists(loc))
            {
                SynapseClient.Logger.Error($"Bundle {descriptor.Bundle} not found!");
                return;
            }
            var stream = File.OpenRead(loc);
            bundle = Il2CppAssetBundleManager.LoadFromStream(stream);
        }

        public void LoadPrefabs()
        {
            foreach (var field in GetType().GetFields())
            {
                var ad = field.GetCustomAttribute(typeof(AssetDescriptor)) as AssetDescriptor;
                if (ad == null) continue;
                switch (ad.Type)
                {
                    case AssetType.Prefab:
                        field.SetValue(this, bundle.LoadAsset<GameObject>(ad.Asset));
                        break;
                    case AssetType.AudioClip:
                        field.SetValue(this, bundle.LoadAsset<AudioClip>(ad.Asset));
                        break;
                    case AssetType.Texture:
                        field.SetValue(this, bundle.LoadAsset<Texture2D>(ad.Asset));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
        public AssetType Type { get; set; } = AssetType.Prefab;
    }

    public enum AssetType
    {
        Prefab,
        AudioClip,
        Texture
    }
}