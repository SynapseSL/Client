using System;
using System.Collections.Generic;
using SynapseClient.API.UI.Abstract;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SynapseClient.API.UI.Components
{
    public class TextField : AbstractSingleChildComponent, IUiComponentBuilder<TextField>
    {
        public List<Action<TextField>> BuilderChain { get; set; } = new List<Action<TextField>>();
        
        public Action<TextField> Builder
        {
            set => BuilderChain.Add(value);
        }
        
        public Action<String> OnValue = delegate(string s)
        {
            Logger.Info($"Content: {s}");
        };


        private TMP_InputField _inputField;
        
        public override void Build(IUiComponent parent)
        {
            Component = Object.Instantiate(Client.Get.UiManager.UIAssetBundle.InputField);
            
            this.RunBuilderChain();
            
            _inputField = Component.GetComponent<TMP_InputField>();
            
            Action<string> textCallback = DoReceiveValue;
            _inputField.onValueChanged.AddListener(textCallback);
            
            this.SetComponentName("UiTextField");
            FinalizeBuild(parent);
        }

        private void DoReceiveValue(string s)
        {
            var text = _inputField.text;
            OnValue?.Invoke(text);
        }
    }
}