using System.IO;
using System.Threading;
using GameCore;
using HarmonyLib;
using LiteNetLib.Utils;
using Mirror;
using Org.BouncyCastle.Utilities.Encoders;
using UnhollowerBaseLib;
using Byte = System.Byte;
using DateTimeOffset = System.DateTimeOffset;
using Encoding = Il2CppSystem.Text.Encoding;
using Random = System.Random;
using String = Il2CppSystem.String;

namespace SynapseClient.Patches
{
    public class AuthPatches
    {
        
        private static int injectionStep = 0;
        private static bool hasInjectedByte = false;
        private static bool isAuth = false;
        private static string targetAddress = "";
        public static string synapseSessionToken = "";
        
        [HarmonyPatch(typeof(CentralAuthManager), nameof(CentralAuthManager.Sign))]
        [HarmonyPrefix]
        public static bool OnCentralAuthManagerSign(ref string __result, string ticket)
        {
            Logger.Info($"Central AuthManager trying to sign Ticket: {ticket}");
            __result = "";
            return false;
        }
        
        
        [HarmonyPatch(typeof(NetDataWriter), nameof(NetDataWriter.Put), typeof(byte))]
        [HarmonyPrefix]
        public static bool OnNetDataWriterByte(ref byte value)
        {
            if (!hasInjectedByte)
            {
                hasInjectedByte = true;
                value = 5;
            }
            else
            {
                return false;
            }
            Logger.Info("NetWriter Write: Byte " + value);
            injectionStep = 0;
            return true;
        }

        [HarmonyPatch(typeof(NetDataWriter), nameof(NetDataWriter.Put), typeof(string))]
        [HarmonyPrefix]
        public static bool OnNetDataWriterString(NetDataWriter __instance, String value)
        {
            injectionStep += 1;

            if (injectionStep == 1)
            {
                Logger.Info("Beginning own Body");
                
                while (!SynapseClientPlugin.isLoggedIn)
                {
                    Thread.Sleep(5); //Not pretty but works
                }
                
                synapseSessionToken = SynapseCentralAuth.Session(targetAddress);
                
                var str = File.ReadAllText("user.dat");
                var random = new Random();
                byte[] bytes = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    bytes[i] = (byte) random.Next(0x41, 0x4b);
                }

                var nonce = str + "#" + Encoding.UTF8.GetString(bytes);
                
                hasInjectedByte = false;
                __instance.Put(str.Length);
                __instance.Put(Encoding.UTF8.GetBytes(str));
                __instance.Put(synapseSessionToken.Length);
                __instance.Put(Encoding.UTF8.GetBytes(synapseSessionToken));
                __instance.Put(nonce.Length);
                __instance.Put(Encoding.UTF8.GetBytes(nonce));
                Logger.Info("==> Body complete");
                PlayerPrefsSl.Set("nickname", nonce);
                isAuth = false;
                Logger.Info("==> Updated NickName to include nonce");
                return false;
            }

            Logger.Info("NetWriter Write: String " + value);
            
            return true;
        }
        
        [HarmonyPatch(typeof(NetDataWriter), nameof(NetDataWriter.Put), typeof(Il2CppStructArray<byte>))]
        [HarmonyPrefix]
        public static bool OnNetDataWriterByteArray(NetDataWriter __instance, Il2CppStructArray<byte> data)
        {
            if (hasInjectedByte) return false;
            Logger.Info("NetWriter Write: ByteArray " + Base64.ToBase64String(data));
            hasInjectedByte = false;
            return true;
        }
        
        [HarmonyPatch(typeof(NetDataWriter), nameof(NetDataWriter.Put), typeof(int))]
        [HarmonyPrefix]
        public static bool OnNetDataWriterInt(int value)
        {
            if (hasInjectedByte) return false;
            Logger.Info("NetWriter Write: Int " + value);
            return true;
        }

        [HarmonyPatch(typeof(CentralAuthManager), nameof(CentralAuthManager.Authentication))]
        [HarmonyPrefix]
        public static bool OnAuth()
        {
            Logger.Info("Faking Central Server Data");
            CentralAuthManager.ApiToken = "";
            CentralAuthManager.Authenticated = true;
            CentralAuthManager.AuthStatusType = AuthStatusType.Success;
            CentralAuthManager.Nonce = "MyRandomNonceNotGonnaUseThis";
            CentralAuthManager.Platform = DistributionPlatform.Dedicated;
            CentralAuthManager.PreauthToken = new CentralAuthPreauthToken();
            CentralAuthManager.PreauthToken.Country = "";
            CentralAuthManager.PreauthToken.Expiration = DateTimeOffset.Now.ToUnixTimeSeconds() + 1000;
            CentralAuthManager.PreauthToken.Flags = Byte.MinValue;
            CentralAuthManager.PreauthToken.Signature = "";
            CentralAuthManager.PreauthToken.UserId = "";

            return true;
        }

        [HarmonyPatch(typeof(NetworkClient), nameof(NetworkClient.Connect))]
        [HarmonyPrefix]
        public static bool OnClientConnect(NetworkClient __instance, string address)
        {
            Logger.Info($"Connecting via Network Client with address {address}");
            isAuth = true;
            targetAddress = address;
            return true;
        }
        
        [HarmonyPatch(typeof(NetworkClient), nameof(NetworkClient.OnConnected))]
        [HarmonyPrefix]
        public static bool OnClientConnected(NetworkClient __instance)
        {
            Logger.Info("Finished Connecting");
            return true;
        }
    }
}