using SynapseClient.API.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Components
{
    public class UiImage : AbstractComponent, ISizeableComponent
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
        
        public Sprite Image { get; set; } = Texture2D.blackTexture.ToSprite(4,4);
        
        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            var image = Component.AddComponent<Image>();
            image.sprite = Image;
            RectTransform.sizeDelta = new Vector2(Width * Scale, Height * Scale);
            this.SetComponentName("UiImage");
            parent.SetAsParentOf(this);
        }

        public static UiImage Of(Sprite Image)
        {
            return new UiImage
            {
                Width = Image.rect.width,
                Height = Image.rect.height,
                Image = Image,
            };
        }
        
        public static UiImage Of(Sprite Image, float width, float height)
        {
            return new UiImage
            {
                Width = width,
                Height = height,
                Image = Image,
            };
        }
        
        public static UiImage Of(Texture2D Image, float width, float height)
        {
            return new UiImage
            {
                Width = width,
                Height = height,
                Image = Image.ToSprite(width, height),
            };
        }
    }

    public static class TextureExtensions
    {
        public static Sprite ToSprite(this Texture2D texture, float width, float height)
        {
            return Sprite.Create(texture, new Rect(0f,0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}