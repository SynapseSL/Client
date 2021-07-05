using SynapseClient.API.UI.Components;
using TMPro;
using UnityEngine;

namespace SynapseClient.API.UI.Presets
{
    public class LoadingScreenComponent : WrapComponent
    {
        public UiText _text;
        
        public override void Build(IUiComponent parent)
        {
            Child = new Stack
            {
                Children = new IUiComponent[]
                {
                    UiImage.Of(Client.Get.UiManager.UIAssetBundle.LoadingBackground, 1920, 1080),
                    Container.FullscreenAlign( new Container
                    {
                        Width = 1920, Child = UiText.Of("", 38, Color.white, alignment: TextAlignmentOptions.Bottom).Out(out UiText text)
                    }, TextAnchor.LowerCenter),
                    Container.FullscreenAlign(new Container
                    {
                        Width = 1920, Padding = Offsets.Of(left: 16, top: 16),
                        Child = UiText.Of("Loading...", 48, Color.white),
                    }, TextAnchor.UpperLeft)
                }
            };
            _text = text;
            base.Build(parent);
        }
    }
}