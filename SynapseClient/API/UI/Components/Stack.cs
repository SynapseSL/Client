using System.Collections.Generic;
using System.Linq;
using SynapseClient.API.UI.Abstract;
using UnityEngine;

namespace SynapseClient.API.UI.Components
{
    public class Stack : AbstractMultiChildComponent, IMultiChildContainer
    {
        public override void Build(IUiComponent parent)
        {
            base.Build(parent);
            this.SetComponentName("Stack");
            FinalizeBuild(parent);
        }
    }
}