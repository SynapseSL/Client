using System;
using System.Collections.Generic;
using SynapseClient.API.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Components
{
    public class Column : AbstractMultiChildComponent, IUiComponentBuilder<Column>
    {
        public TextAnchor Alignment { get; set; } = TextAnchor.UpperLeft;
        public bool ExpandWidth { get; set; } = true;
        public bool ExpandHeight { get; set; } = true;
        public bool ControlChildWidth { get; set; } = false;
        public bool ControlChildHeight { get; set; } = false;
        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            var layoutGroup = Component.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = Alignment;
            layoutGroup.childForceExpandHeight = ExpandHeight;
            layoutGroup.childForceExpandWidth = ExpandWidth;
            layoutGroup.childControlHeight = ControlChildHeight;
            layoutGroup.childControlWidth = ControlChildWidth;
            this.RunBuilderChain();
            this.SetComponentName("Column");
            FinalizeBuild(parent);
        }

        public List<Action<Column>> BuilderChain { get; set; } = new List<Action<Column>>();

        public Action<Column> Builder
        {
            set
            {
             BuilderChain.Add(value);   
            }
        }
    }
}