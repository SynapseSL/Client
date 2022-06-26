using TMPro;
using UnityEngine;

namespace SynapseClient.ClientModule.Utils;

public static class GuiUtils
{

    public static GameObject Create(string name, out RectTransform transform)
    {
        var obj = new GameObject(name);
        transform = obj.AddComponent<RectTransform>();
        return obj;
    }

    public static TextMeshProUGUI AddText(this RectTransform transform, string name)
    {
        var child = Create(name, out var childTransform);
        var gui = child.AddComponent<TextMeshProUGUI>();
        childTransform.SetParent(transform);
        return gui;
    }
    
}