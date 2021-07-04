using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SynapseClient.API.UI.Abstract
{
    public abstract class AbstractMultiChildComponent : IMultiChildContainer
    {
        public List<IUiComponent> _children = new List<IUiComponent>();

        public IUiComponent[] Children
        {
            get => _children.ToArray();
            set => _children = value.ToList();
        }

        public GameObject Component { get; set; }
        public RectTransform RectTransform { get; private set; }

        public virtual void Build(IUiComponent parent)
        {
            Component = new GameObject();
            RectTransform = Component.AddComponent<RectTransform>();
        }

        public void FinalizeBuild(IUiComponent parent)
        {
            parent.SetAsParentOf(this);
            _children.ForEach(x => x.Build(this));
        }

        public virtual void Activate()
        {
            Component.active = true;
            foreach (var child in Children)
            {
                child.Activate();
            }
        }

        public virtual void Deactivate()
        {
            Component.active = false;
            foreach (var child in Children)
            {
                child.Deactivate();
            }
        }

        public virtual void Destroy()
        {
            Object.Destroy(Component);
            foreach (var child in Children)
            {
                child.Destroy();
            }
        }

        public void AddChild(IUiComponent uiComponent)
        {
            _children.Add(uiComponent);
        }
    }
}