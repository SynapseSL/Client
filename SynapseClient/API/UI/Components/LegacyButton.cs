using System;
using System.Collections.Generic;
using SynapseClient.API.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Components
{
    public class LegacyButton : AbstractSingleChildComponent, IUiComponentBuilder<LegacyButton>
    {
        public Action OnClick { get; set; } = delegate
        {
            Logger.Info("Clicked!");
        };

        public List<Action<LegacyButton>> BuilderChain { get; set; } = new List<Action<LegacyButton>>();
        public Action<LegacyButton> Builder
        {
            set => BuilderChain.Add(value);
        }

        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            this.RunBuilderChain();
            var button = Component.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(OnClick);
            this.SetComponentName("UiButton");
            FinalizeBuild(parent);
        }

        public void SetChild(IUiComponent uiComponent)
        {
            Child = uiComponent;
        }
    }
}