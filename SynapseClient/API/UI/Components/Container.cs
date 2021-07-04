using System;
using System.Collections.Generic;
using SynapseClient.API.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Components
{
    public class Container : AbstractSingleChildComponent, ISizeableComponent, IUiComponentBuilder<Container>
    {
        public TextAnchor Alignment { get; set; } = TextAnchor.MiddleCenter;
        public float Width { get; set; } = 100;
        public float Height { get; set; } = 100;
        public float Scale { get; set; } = 1f;
        
        public Vector2 Size
        {
            set
            {
                Width = value.x;
                Height = value.y;
            }
        }
        public bool ExpandChild { get; set; } = true;
        public RectOffset Padding { get; set; } = Offsets.Zero;
        
        public Color? Color { get; set; } = null;

        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            var layoutGroup = Component.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = Alignment;
            layoutGroup.childControlHeight = ExpandChild;
            layoutGroup.childControlWidth = ExpandChild;
            layoutGroup.padding = Padding;
            RectTransform.sizeDelta = new Vector2(Width * Scale, Height * Scale);

            if (Color.HasValue)
            {
                var img = Component.AddComponent<Image>();
                img.color = Color.Value;
            }

            this.RunBuilderChain();
            this.SetComponentName("Container");
            FinalizeBuild(parent);
        }
        
        public static Container FullscreenAlign(IUiComponent component, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            return new Container
            {
                Width = 1920, Height = 1080, Child = component, Alignment = alignment, ExpandChild = false
            };
        }
        
        public static Container ConstrainHeight(IUiComponent component, float height)
        {
            return new Container
            {
                Height = height, Child = component, ExpandChild = true
            };
        }
        
        public static Container ConstrainWidth(IUiComponent component, float width)
        {
            return new Container
            {
                Width = width, Child = component, ExpandChild = true
            };
        }

        public List<Action<Container>> BuilderChain { get; set; } = new List<Action<Container>>();

        public Action<Container> Builder
        {
            set
            {
                BuilderChain.Add(value);
            }
        }
    }
}