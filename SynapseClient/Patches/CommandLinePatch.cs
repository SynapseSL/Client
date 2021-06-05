using System;
using System.IO;
using HarmonyLib;
using RemoteAdmin;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using String = Il2CppSystem.String;

namespace SynapseClient.Patches
{
    public class CommandLinePatch
    {
        [HarmonyPatch(typeof(GameCore.Console), nameof(GameCore.Console.TypeCommand))]
        [HarmonyPrefix]
        public static bool OnStart(GameCore.Console __instance, string cmd, CommandSender sender)
        {
            if (cmd == "abtest")
            {
                var go = GameObject.Instantiate(SynapseClientPlugin.aidkit, ReferenceHub.LocalHub.playerMovementSync.GetRealPosition(), Quaternion.identity);
                return false;
            }
            return true;
        }
        
        [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.CmdSendEncryptedQuery))]
        [HarmonyPrefix]
        public static bool OnCmdQuery(QueryProcessor __instance, Il2CppStructArray<byte> query)
        {
            Logger.Info("Sending Encrypted Query");
            return true;
        }
        
        [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.EcdsaSign))]
        [HarmonyPrefix]
        public static bool OnStart(QueryProcessor __instance, string message, int counter)
        {
            Logger.Info("ECDSA Sign");
            return true;
        }
        
        [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.CallCmdSendEncryptedQuery))]
        [HarmonyPrefix]
        public static bool OnCmdQueryCall(QueryProcessor __instance, Il2CppStructArray<byte> query)
        {
            Logger.Info("Encrypted Query Call");
            return true;
        }
    }
}