using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Il2CppSystem.Linq;
using SynapseClient.API.Events;
using SynapseClient.API.UI.Components;
using SynapseClient.API.UI.Presets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = SynapseClient.API.UI.Components.Button;
using Input = BepInEx.IL2CPP.UnityEngine.Input;
using Object = System.Object;
using Stack = SynapseClient.API.UI.Components.Stack;

namespace SynapseClient.API.UI
{
    public class UiManager
    {
        public UIAssetBundle UIAssetBundle { get; } = new UIAssetBundle();

        public SynapseLoadingScreen LoadingScreen { get; set; }

        public UiManager()
        {
            SynapseEvents.OnMenuLoaded += delegate
            {
                UIAssetBundle.LoadBundle();
                UIAssetBundle.LoadPrefabs();

                SynapseCoroutine.Delay(delegate
                {
                    try
                    {
                        ShowSnackbar("Test", "buiefubiesbuiesfbuiesfuibsefbbuisef");
                        //ShowLoadingScreen();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                }, TimeSpan.FromSeconds(1));
            };
            
            SynapseEvents.OnConnectionSuccessful += delegate
            {
                UIAssetBundle.LoadPrefabs();
            };
        }

        public RootComponent CreateRoot(TextAnchor anchor)
        {
            var uiRoot = new GameObject();
            var canvas = uiRoot.AddComponent<Canvas>();
            var scaler = uiRoot.AddComponent<CanvasScaler>();
            uiRoot.AddComponent<GraphicRaycaster>();
            var lay = uiRoot.AddComponent<VerticalLayoutGroup>();
            var rect = uiRoot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1920, 1080);
            lay.childControlHeight = false;
            lay.childControlWidth = false;
            lay.childAlignment = anchor;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 255;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            uiRoot.name = "UiRoot" + uiRoot.GetInstanceID();
            return new RootComponent(uiRoot);
        }

        public void ShowError(string message, string title = "Something went wrong", int duration = 5)
        {
            ShowSnackbar(title, message, "#d63031".ParseHexColor(), duration: duration);
        }

        public SynapseCoroutine ShowLoadingScreen()
        {
            return SynapseCoroutine.Synchronize(delegate
            {
                Client.Get.UiManager.UIAssetBundle.LoadPrefabs(); //Somehow this breaks everytime if I don't do it.
                var root = CreateRoot(TextAnchor.MiddleCenter);
                var overlay = new LoadingScreenComponent();
                root.WithChild(overlay).Build(null);
                root.Show();
                var handle = SynapseCoroutine.StartCoroutine(LoadingIndicatorRoutine(overlay._text));
                LoadingScreen = new SynapseLoadingScreen
                {
                    Screen = root,
                    CoroutineHandle = handle
                };
            });
        }

        public SynapseCoroutine HideLoadingScreen()
        {
            if (LoadingScreen != null)
            {
                return SynapseCoroutine.Synchronize(delegate
                {
                    SynapseCoroutine.StopCoroutine(LoadingScreen.CoroutineHandle);
                    LoadingScreen.Screen.Destroy();
                });
            }

            return null;
        }

        private IEnumerator LoadingIndicatorRoutine(UiText text)
        {
            var baseString = "-------------------------------";
            var length = 32;
            var pos = 0;
            while (true)
            {
                var cur = baseString.Clone() as string;
                cur = cur.Insert(pos, "=");

                if (pos == length - 1) pos = 0;
                else pos += 1;

                text.Text = cur;
                yield return new WaitForSeconds(0.05f);
            }
        }


        public void ShowSnackbar(string title, string message, Color? color = null, bool fullWidth = false,
            int duration = 5)
        {
            Logger.Info("1");
            SynapseCoroutine.Synchronize(delegate
            {
                Logger.Info("2");
                var root = CreateRoot(TextAnchor.UpperCenter);
                Logger.Info("3");
                var popup = new Snackbar
                {
                    Color = color, Message = message, Title = title, FullWidth = fullWidth
                };
                Logger.Info("4");
                root.WithChild(popup).Build(null);
                Logger.Info("5");
                root.Show();
                Logger.Info("6");
                SynapseCoroutine.Delay(delegate
                {
                    
                    Logger.Info("7");
                    root.Destroy();
                    Logger.Info("8");
                }, TimeSpan.FromSeconds(duration));
            });
        }
    }

    public class SynapseLoadingScreen
    {
        public RootComponent Screen { get; set; }
        public SynapseCoroutine CoroutineHandle { get; set; }
    }

    public static class Offsets
    {
        public static RectOffset Zero = new RectOffset
        {
            top = 0, bottom = 0, left = 0, right = 0
        };

        public static RectOffset All(int offset)
        {
            return new RectOffset
            {
                top = offset,
                bottom = offset,
                left = offset,
                right = offset
            };
        }

        public static RectOffset Of(int top = 0, int bottom = 0, int left = 0, int right = 0)
        {
            return new RectOffset
            {
                top = top,
                bottom = bottom,
                left = left,
                right = right
            };
        }

        public static RectOffset Symmetrical(int vertical, int horizontal)
        {
            return new RectOffset
            {
                top = vertical,
                bottom = vertical,
                left = horizontal,
                right = horizontal
            };
        }
    }

    public interface IUiComponentBuilder<T> where T : IUiComponent, IUiComponentBuilder<T>
    {
        List<Action<T>> BuilderChain { get; set; }
        Action<T> Builder { set; }
    }

    public interface IUiComponent
    {
        GameObject Component { get; set; }
        void Build(IUiComponent parent);
        void Activate();
        void Deactivate();
        void Destroy();
    }

    public static class UiComponentExtension
    {
        public static void SetAsParentOf(this IUiComponent parent, IUiComponent child)
        {
            var c = child.Component.GetComponent<RectTransform>();
            var p = parent.Component.GetComponent<RectTransform>();
            c.SetParent(p, false);
        }

        public static ISingleChildContainer WithChild(this ISingleChildContainer container, IUiComponent child)
        {
            container.SetChild(child);
            return container;
        }

        public static IMultiChildContainer WithChild(this IMultiChildContainer container, IUiComponent child)
        {
            container.AddChild(child);
            return container;
        }

        public static IUiComponent Out(this IUiComponent component, out IUiComponent output)
        {
            output = component;
            return component;
        }

        public static IUiComponent Out<K>(this IUiComponent component, out K output)
        {
            output = (K) component;
            return component;
        }

        public static void SetComponentName(this IUiComponent component, string prefix = "Component")
        {
            component.Component.name = prefix + "#" + component.Component.GetInstanceID();
        }

        public static void RunBuilderChain<T>(this T component) where T : IUiComponent, IUiComponentBuilder<T>
        {
            component.BuilderChain.ForEach(x => x.Invoke(component));
        }

        public static Color WithOpacity(this Color color, float opacity)
        {
            return new Color(color.r, color.g, color.b, opacity);
        }

        //https://stackoverflow.com/questions/2109756/how-do-i-get-the-color-from-a-hexadecimal-color-code-using-net
        public static Color ParseHexColor(this string s)
        {
            var colorcode = s.ToUpper();
            colorcode = colorcode.TrimStart('#');

            Color col; // from System.Drawing or System.Windows.Media
            if (colorcode.Length == 6)
                col = new Color(
                    int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber) / 255f,
                    int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber) / 255f,
                    int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber) / 255f
                );
            else // assuming length of 8
                col = new Color(
                    int.Parse(colorcode.Substring(0, 2), NumberStyles.HexNumber) / 255f,
                    int.Parse(colorcode.Substring(2, 2), NumberStyles.HexNumber) / 255f,
                    int.Parse(colorcode.Substring(4, 2), NumberStyles.HexNumber) / 255f
                );
            return col;
        }
    }

    public interface ISizeableComponent : IUiComponent
    {
        float Width { get; set; }
        float Height { get; set; }
        float Scale { get; set; }
        Vector2 Size { set; }
    }

    public interface ISingleChildContainer : IUiComponent
    {
        IUiComponent Child { set; }
        void SetChild(IUiComponent uiComponent);
    }

    public interface IMultiChildContainer : IUiComponent
    {
        void AddChild(IUiComponent uiComponent);
    }
}