using System;
using System.Collections.Generic;
using SynapseClient.API.UI.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SynapseClient.API.UI.Components
{
    public class Button : AbstractSingleChildComponent, IUiComponentBuilder<Button>
    {
        public List<Action<Button>> BuilderChain { get; set; } = new List<Action<Button>>();
        
        public Action<Button> Builder
        {
            set => BuilderChain.Add(value);
        }
        
        public Action OnClick { get; set; } = delegate
        {
            Logger.Info("Clicked!");
        };

        public string Text { get; set; } = "";
        public int FontSize { get; set; } = 14;
        public FontStyles FontStyle { get; set; } = FontStyles.Bold;
        public Color Color { get; set; } = Color.black;
        public TextAlignmentOptions Alignment { get; set; } = TextAlignmentOptions.Center;
        
        private UnityEngine.UI.Button _button;

        public override void Build(IUiComponent parent)
        {
            Component = Object.Instantiate(Client.Get.UiManager.UIAssetBundle.Button);
            
            this.RunBuilderChain();
            
            _button = Component.GetComponent<UnityEngine.UI.Button>();
            
            _button.onClick.AddListener(OnClick);

            var label = Component.GetComponentInChildren<TextMeshProUGUI>();
            label.text = Text;
            label.fontSize = FontSize;
            label.color = Color;
            label.alignment = Alignment;
            label.fontStyle = FontStyle;
            
            this.SetComponentName("UiButton");
            FinalizeBuild(parent);
        }
    }
}