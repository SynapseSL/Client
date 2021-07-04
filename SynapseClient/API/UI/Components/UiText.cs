using SynapseClient.API.UI.Abstract;
using TMPro;
using UnityEngine;

namespace SynapseClient.API.UI.Components
{
    public class UiText : AbstractComponent
    {

        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (_textComponent != null) _textComponent.text = value;
            }
        }

        public int FontSize { get; set; } = 14;
        public FontStyles FontStyle { get; set; } = FontStyles.Normal;
        public Color Color { get; set; } = Color.black;
        public TextAlignmentOptions Alignment { get; set; } = TextAlignmentOptions.TopLeft;

        private TextMeshProUGUI _textComponent;

        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            _textComponent = Component.AddComponent<TextMeshProUGUI>();
            _textComponent.text = Text;
            _textComponent.fontSize = FontSize;
            _textComponent.color = Color;
            _textComponent.alignment = Alignment;
            _textComponent.fontStyle = FontStyle;
            this.SetComponentName("UiText");
            FinalizeBuild(parent);
        }

        public static UiText Of(string text, int fontSize = 14, Color? color = null,
            FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            return new UiText
            {
                Text = text,
                FontSize = fontSize,
                Color = color ?? UnityEngine.Color.black,
                FontStyle = style,
                Alignment = alignment
            };
        }
    }
}