using System.Collections.Generic;
using CommandSystem;
using GameCore;
using HarmonyLib;
using RemoteAdmin;
using UnhollowerBaseLib;
using UnityEngine;
using File = Il2CppSystem.IO.File;

namespace SynapseClient.Patches
{
    public class CommandLinePatch
    {
        public static Dictionary<string, Il2CppAssetBundle> AssetBundles = new Dictionary<string, Il2CppAssetBundle>();
        
        [HarmonyPatch(typeof(GameCore.Console), nameof(GameCore.Console.TypeCommand))]
        [HarmonyPrefix]
        public static bool OnStart(GameCore.Console __instance, string cmd, CommandSender sender)
        {
            if (cmd.StartsWith("redirect "))
            {
                var target = cmd.Replace("redirect ", " ");
                SynapseClient.Redirect(target);
                return false;
            } else if (cmd.StartsWith("bundle load "))
            {
                var target = cmd.Replace("bundle load ", "");
                var stream = File.OpenRead(target);
                AssetBundles[target] = Il2CppAssetBundleManager.LoadFromStream(stream);
                return false;
            } else if (cmd.StartsWith("bundle spawn "))
            {
                var target = cmd.Replace("bundle spawn ", "");
                var split = target.Split(':');
                var prefab = AssetBundles[split[0]].LoadAsset<GameObject>(split[1]);
                UnityEngine.Object.Instantiate(prefab, LocalPlayer.Singleton.transform.position, Quaternion.identity);
                return false;
            }  else if (cmd.StartsWith("bundle rigidbody "))
            {
                var target = cmd.Replace("bundle rigidbody ", "");
                var split = target.Split(':');
                var prefab = AssetBundles[split[0]].LoadAsset<GameObject>(split[1]);
                Logger.Info(prefab.ToString());
                var obj = UnityEngine.Object.Instantiate(prefab, LocalPlayer.Singleton.transform.position, Quaternion.identity);
                Logger.Info(obj.ToString());
                var mesh = obj.GetComponentInChildren<MeshFilter>().mesh;
                var collider = obj.AddComponent<BoxCollider>();
                collider.center = mesh.bounds.center;
                collider.size = mesh.bounds.size;
                collider.extents = mesh.bounds.extents;
                obj.AddComponent<Rigidbody>();
                
                return false;
            }
            
            return true;
        }
        
        [HarmonyPatch(typeof(GameCore.Console), nameof(GameCore.Console.Awake))]
        [HarmonyPrefix]
        public static bool OnStart(GameCore.Console __instance)
        {
            Logger.Info("Starting GameConsole");
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