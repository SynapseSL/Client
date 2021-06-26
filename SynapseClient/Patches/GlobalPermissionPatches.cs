﻿using System.Collections.Generic;
using Swan.Formatters;
using HarmonyLib;
using Swan;

namespace SynapseClient.Patches
{
    public static class GlobalPermissionPatches
    {
        [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.Update))]
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

                if (string.IsNullOrEmpty(__instance.GlobalBadge))
                {
                    __instance._bgc = null;
                    __instance._bgt = null;
                    __instance._authorizeBadge = false;
                    __instance._prevColor += ".";
                    __instance._prevText += ".";
                    return false;
                }

                Logger.Info(__instance._hub.characterClassManager.UserId);
                var su = SynapseCentral.Resolve(__instance._hub.characterClassManager.UserId);

                if (su.Groups == null || su.Groups.Count < 1)
                {
                    __instance._bgc = null;
                    __instance._bgt = null;
                    __instance._authorizeBadge = false;
                    __instance._prevColor += ".";
                    __instance._prevText += ".";
                    return false;
                }

                var group = su.Groups[0];
                group.Hidden = true;


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

            if (__instance.CurrentColor.Restricted &&
                (__instance.MyText != __instance._bgt || __instance.MyColor != __instance._bgc))
            {
                GameCore.Console.AddLog(
                    $"TAG FAIL 1 - {__instance.MyText} - {__instance._bgt} /-/ {__instance.MyColor} - {__instance._bgc}",
                    UnityEngine.Color.gray, false);
                __instance._authorizeBadge = false;
                __instance.NetworkMyColor = "default";
                __instance.NetworkMyText = null;
                __instance._prevColor = "default";
                __instance._prevText = null;
                PlayerList.UpdatePlayerRole(__instance.gameObject);
                return false;
            }

            if (__instance.MyText != null && __instance.MyText != __instance._bgt && (__instance.MyText.Contains("[") ||
                __instance.MyText.Contains("]") || __instance.MyText.Contains("<") || __instance.MyText.Contains(">")))
            {
                GameCore.Console.AddLog(
                    $"TAG FAIL 2 - {__instance.MyText} - {__instance._bgt} /-/ {__instance.MyColor} - {__instance._bgc}",
                    UnityEngine.Color.gray, false);
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
    }
}
