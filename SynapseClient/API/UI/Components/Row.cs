using System;
using System.Collections.Generic;
using SynapseClient.API.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Components
{
    public class Row : AbstractMultiChildComponent, IUiComponentBuilder<Row>
    {
        public TextAnchor Alignment { get; set; } = TextAnchor.UpperLeft;
        public bool ExpandWidth { get; set; } = true;
        public bool ExpandHeight { get; set; } = true;
        public bool ControlChildWidth { get; set; } = false;
        public bool ControlChildHeight { get; set; } = false;

        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            var layoutGroup = Component.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = Alignment;
            layoutGroup.childForceExpandHeight = ExpandHeight;
            layoutGroup.childForceExpandWidth = ExpandWidth;
            layoutGroup.childControlHeight = ControlChildHeight;
            layoutGroup.childControlWidth = ControlChildWidth;
            this.RunBuilderChain();
            this.SetComponentName("Row");
            FinalizeBuild(parent);
        }

        public List<Action<Row>> BuilderChain { get; set; } = new List<Action<Row>>();

        public Action<Row> Builder
        {
            set
            {
                BuilderChain.Add(value);
            }
        }
    }
}