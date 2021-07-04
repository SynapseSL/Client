using SynapseClient.API.UI.Abstract;
using SynapseClient.API.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SynapseClient.API.UI.Presets
{
    public class Snackbar : WrapComponent
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Color? Color { get; set; }
        public bool FullWidth { get; set; } = false;
        public int Duration { get; set; }

        public override void Build(IUiComponent parent)
        {
            Child = new Container
            {
                Width = FullWidth ? 1920 : 960, Height = 140, Padding = Offsets.All(16),
                Child = new Column
                {
                    Children = new IUiComponent[]
                    {
                        new Container
                        {
                            Child = UiText.Of(Title, color: UnityEngine.Color.white, fontSize: 24),
                            Height = 24 + 8, Alignment = TextAnchor.UpperLeft
                        },
                        UiText.Of(Message, 16, UnityEngine.Color.white.WithOpacity(0.9f), alignment: TextAlignmentOptions.TopLeft),
                    },
                    ControlChildHeight = false, ControlChildWidth = true, ExpandHeight = false,
                    Alignment = TextAnchor.UpperLeft
                },
                Color = Color ?? UnityEngine.Color.black.WithOpacity(0.9f)
            };
            base.Build(parent);
        }
    }
}