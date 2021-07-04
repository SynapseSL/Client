using SynapseClient.API.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Components
{
    public class ColorBox : AbstractComponent
    {
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
        
        public Color Color { get; set; } = Color.red;
        
        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            var image = Component.AddComponent<Image>();
            image.color = Color;
            RectTransform.sizeDelta = new Vector2(Width * Scale, Height * Scale);
            this.SetComponentName("UiColorBox");
            FinalizeBuild(parent);
        }
    }
}