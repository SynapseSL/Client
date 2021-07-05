using UnityEngine;

namespace SynapseClient.API.UI.Components
{
    public abstract class WrapComponent :  ISingleChildContainer
    {
        public GameObject Component { get; set; }
        public virtual void Build(IUiComponent parent)
        {
            Child.Build(parent);
            Component = new GameObject();
        }

        public void Activate()
        {
            Child.Activate();
        }

        public void Deactivate()
        {
            Child.Deactivate();
        }

        public void Destroy()
        {
            Child.Destroy();
        }

        public IUiComponent Child { get; set; }

        public void SetChild(IUiComponent uiComponent)
        {
            Child = uiComponent;
        }
    }
}