using System.Collections.Generic;
using Swan.Formatters;
using HarmonyLib;

namespace SynapseClient.Patches
{
    public static class GlobalPermissionPatches
    {
        [HarmonyPatch(typeof(ServerRoles),nameof(ServerRoles.Update))]
        [HarmonyPrefix]
        public static bool OnUpdate(ServerRoles __instance)
        {
            if (__instance.CurrentColor == null) return false;

            if (!string.IsNullOrEmpty(__instance.FixedBadge) && __instance.MyText != __instance.FixedBadge)
            {
                __instance.SetText(__instance.FixedBadge);
                __instance.SetColor("silver");
                return false;
            }
            if (!string.IsNullOrEmpty(__instance.FixedBadge) && __instance.CurrentColor.Name != "silver")
            {
                __instance.SetColor("silver");
                return false;
            }

            if (__instance.GlobalBadge != __instance._prevBadge)
            {
                __instance._prevBadge = __instance.GlobalBadge;

                //Implement Later to gt
                var group = new GlobalSynapseGroup
                {
                    Staff = true,
                    Permissions = new System.Collections.Generic.List<string> { "*" },
                    RemoteAdmin = true,
                    Name = "[Synapse Creator]",
                    Color = "blue"
                };

                if (group == null)
                {
                    __instance._bgc = null;
                    __instance._bgt = null;
                    __instance._authorizeBadge = false;
                    __instance._prevColor += ".";
                    __instance._prevText += ".";
                    return false;
                }

                if (group.Color == "(none)" || group.Name == "(none)")
                {
                    __instance._bgc = null;
                    __instance._bgt = null;
                    __instance._authorizeBadge = false;
                }
                else
                {
                    __instance.NetworkMyText = group.Name;
                    __instance._bgt = group.Name;

                    __instance.NetworkMyColor = group.Color;
                    __instance._bgc = group.Color;

                    __instance._authorizeBadge = true;
                }
            }

            if (__instance._prevColor == __instance.MyColor && __instance._prevText == __instance.MyText) return false;

            if (__instance.CurrentColor.Restricted && (__instance.MyText != __instance._bgt || __instance.MyColor != __instance._bgc))
            {
                GameCore.Console.AddLog($"TAG FAIL 1 - {__instance.MyText} - {__instance._bgt} /-/ {__instance.MyColor} - {__instance._bgc}", UnityEngine.Color.gray, false);
                __instance._authorizeBadge = false;
                __instance.NetworkMyColor = "default";
                __instance.NetworkMyText = null;
                __instance._prevColor = "default";
                __instance._prevText = null;
                PlayerList.UpdatePlayerRole(__instance.gameObject);
                return false;
            }

            if (__instance.MyText != null && __instance.MyText != __instance._bgt && (__instance.MyText.Contains("[") || __instance.MyText.Contains("]") || __instance.MyText.Contains("<") || __instance.MyText.Contains(">")))
            {
                GameCore.Console.AddLog($"TAG FAIL 2 - {__instance.MyText} - {__instance._bgt} /-/ {__instance.MyColor} - {__instance._bgc}", UnityEngine.Color.gray, false);
                __instance._authorizeBadge = false;
                __instance.NetworkMyColor = "default";
                __instance.NetworkMyText = null;
                __instance._prevColor = "default";
                __instance._prevText = null;
                PlayerList.UpdatePlayerRole(__instance.gameObject);
                return false;
            }

            __instance._prevColor = __instance.MyColor;
            __instance._prevText = __instance.MyText;
            __instance._prevBadge = __instance.GlobalBadge;
            PlayerList.UpdatePlayerRole(__instance.gameObject);

            return false;
        }

        public class GlobalSynapseGroup
        {
            [JsonProperty("name")]
            public string Name { get; set; } = "";

            [JsonProperty("color")]
            public string Color { get; set; } = "";

            [JsonProperty("hidden")]
            public bool Hidden { get; set; } = false;

            [JsonProperty("remoteAdmin")]
            public bool RemoteAdmin { get; set; } = false;

            [JsonProperty("permissions")]
            public List<string> Permissions { get; set; } = new List<string>() { };

            [JsonProperty("kickable")]
            public bool Kickable { get; set; } = true;

            [JsonProperty("bannable")]
            public bool Bannable { get; set; } = true;

            [JsonProperty("kick")]
            public bool Kick { get; set; } = false;

            [JsonProperty("ban")]
            public bool Ban { get; set; } = false;

            [JsonProperty("staff")]
            public bool Staff { get; set; } = false;
        }
    }
}
