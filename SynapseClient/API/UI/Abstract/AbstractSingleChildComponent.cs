using UnityEngine;

namespace SynapseClient.API.UI.Abstract
{
    public abstract class AbstractSingleChildComponent : ISingleChildContainer
    {
        public IUiComponent Child { get; set; }
        public GameObject Component { get; set; }
        public RectTransform RectTransform { get; private set; }
        
        public void SetChild(IUiComponent uiComponent)
        {
            Child = uiComponent;
        }
        
        public virtual void Build(IUiComponent parent)
        {
            Component = new GameObject();
            RectTransform = Component.AddComponent<RectTransform>();
        }

        public void FinalizeBuild(IUiComponent parent)
        {
            parent.SetAsParentOf(this);
            Child.Build(this);
        }

        public virtual void Activate()
        {
            Component.active = true;
            Child.Activate();
        }

        public virtual void Deactivate()
        {
            Component.active = false;
            Child.Deactivate();
        }

        public virtual void Destroy()
        {
            Child.Destroy();
            Object.Destroy(Component);
        }
    }
}