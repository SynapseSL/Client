using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Il2CppSystem.IO;

namespace UnityEngine
{
    public abstract class BundleEntity
    {
        public Il2CppAssetBundle bundle;
        private List<AssetEntry> _cachedAssetEntries;

        public void LoadBundle()
        {
            if (bundle != null)
            {
                global::Logger.Error("Bundle already loaded");
                return;
            }
            var descriptor = GetType().GetCustomAttribute(typeof(BundleDescriptor)) as BundleDescriptor;
            var loc = Path.Combine("bundles", descriptor.BundleLocation);
            global::Logger.Info(loc);
            if (!File.Exists(loc) && descriptor.Source != null)
            {
                var client = new WebClient();
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                client.DownloadFile("https://github.com/SynapseSL/ClientPackages/raw/main/" + descriptor.Source,loc);
                global::Logger.Info($"Downloaded bundle via GitHub {descriptor.Source}");
            }
            else if (!File.Exists(loc))
            {
                global::Logger.Error($"Bundle {descriptor.BundleLocation} not found!");
                return;
            }
            var stream = File.OpenRead(loc);
            bundle = Il2CppAssetBundleManager.LoadFromStream(stream);
            _cachedAssetEntries = new List<AssetEntry>();
            
            SharedBundleManager.Singleton.Bundles[descriptor.BundleName] = this;
        }

        public virtual void LoadPrefabs()
        {
            _cachedAssetEntries = new List<AssetEntry>();
            foreach (var field in GetType().GetFields())
            {
                if (!(field.GetCustomAttribute(typeof(AssetDescriptor)) is AssetDescriptor ad)) continue;
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

            _cachedAssetEntries = _GetEntries();
        }

        private List<AssetEntry> _GetEntries()
        {
            var list = new List<AssetEntry>();
            var descriptor = GetType().GetCustomAttribute(typeof(BundleDescriptor)) as BundleDescriptor;
            var bundleName = descriptor.BundleName;
            foreach (var field in GetType().GetFields())
            {
                if (!(field.GetCustomAttribute(typeof(AssetDescriptor)) is AssetDescriptor ad)) continue;
                var obj = field.GetValue(this);
                list.Add(new AssetEntry()
                {
                    Bundle = bundleName,
                    Path = ad.Asset,
                    Type = ad.Type,
                    Reference = obj
                });
            }

            return list;
        }

        public IEnumerable<AssetEntry> GetEntries()
        {
            return _cachedAssetEntries;
        }
    }

    public class AssetEntry
    {
        public string Bundle { get; set; }
        public string Path { get; set; }
        public AssetType Type { get; set; }
        public object Reference { get; set; }

        public override string ToString()
        {
            return $"{Type}-Asset: {Bundle}:{Path}. Set: {Reference != null}";
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BundleDescriptor : Attribute
    {
        public string BundleLocation { get; set; }
        public string BundleName { get; set; }
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