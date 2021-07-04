using System;
using UnityEngine;

namespace SynapseClient.API.UI
{
    public class RootComponent : IUiComponent, ISingleChildContainer
    {
        public IUiComponent Child { get; set; }
        public GameObject Component { get; set; }

        public RootComponent(GameObject component)
        {
            Component = component;
        }

        public void Build(IUiComponent parent)
        {
            Component.active = false;
            Child.Build(this);
        }

        public void Activate()
        {
            Child.Activate();
        }

        public void Deactivate()
        {
            Child.Deactivate();
        }

        public void Show()
        {
            Component.active = true;
            try
            {
                Activate();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Hide()
        {
            Component.active = false;
            try
            {
                Deactivate();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Destroy()
        {
            try
            {
                Child.Destroy();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            UnityEngine.Object.Destroy(Component);
        }

        public void SetChild(IUiComponent uiComponent)
        {
            Child = uiComponent;
        }
    }
}