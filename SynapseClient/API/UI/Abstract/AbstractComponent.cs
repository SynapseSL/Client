using UnityEngine;

namespace SynapseClient.API.UI.Abstract
{
    public abstract class AbstractComponent : IUiComponent
    {
        public GameObject Component { get; set; }
        public RectTransform RectTransform { get; private set; }
        
        public virtual void Build(IUiComponent parent)
        {
            Component = new GameObject();
            RectTransform = Component.AddComponent<RectTransform>();
        }

        public virtual void FinalizeBuild(IUiComponent parent)
        {
            parent.SetAsParentOf(this);
        }
        
        public virtual void Activate()
        {
            Component.active = true;
        }

        public virtual void Deactivate()
        {
            Component.active = false;
        }

        public virtual void Destroy()
        {
            Object.Destroy(Component);
        }
    }
}