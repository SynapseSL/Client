using UnityEngine;

namespace SynapseClient.API.UI
{
    [BundleDescriptor(BundleLocation = "uitoolkit.bundle", BundleName = "UiToolkit")]
    public class UIAssetBundle : BundleEntity
    {

        [AssetDescriptor(Asset = "Assets/Prefabs/InputField.prefab", Type = AssetType.Prefab)]
        public GameObject InputField;
        
        [AssetDescriptor(Asset = "Assets/Prefabs/Button.prefab", Type = AssetType.Prefab)]
        public GameObject Button;

        [AssetDescriptor(Asset = "Assets/Banners/AmbienceBanner.png", Type = AssetType.Texture)]
        public Texture2D BannerAmbience;

        [AssetDescriptor(Asset = "Assets/Textures/LoadingBackground.png", Type = AssetType.Texture)]
        public Texture2D LoadingBackground;

    }
}