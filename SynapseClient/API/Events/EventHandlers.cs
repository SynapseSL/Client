using System;
using System.Text;
using Jwt;
using RemoteAdmin;
using Steamworks;
using SynapseClient.Components;
using SynapseClient.Models;
using SynapseClient.Patches;
using SynapseClient.Pipeline;
using SynapseClient.Pipeline.Packets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SynapseClient.API.Events
{
    public class EventHandlers
    {
        internal EventHandlers()
        {
            ClientPipeline.DataReceivedEvent += MainReceivePipelineData;
            SceneManager.add_sceneLoaded(new System.Action<Scene, LoadSceneMode>(OnSceneLoaded));

            SynapseEvents.OnCreateCreditsEvent += CreateCredits;
        }

        private void CreateCredits(CreditsHook ev)
        {
            // Synapse Client Credits
            ev.CreateCreditsCategory("Synapse Client");
            ev.CreateCreditsEntry("Helight", "Maintainer", "Synapse Client", CreditColors.Red600);
            ev.CreateCreditsEntry("Dimenzio", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Wholesome", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Mika", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Cubuzz", "Developer", "Synapse Client", CreditColors.Blue100);
            ev.CreateCreditsEntry("Flo0205", "Developer", "Synapse Client", CreditColors.Blue100);

            // Synapse Server Credits
            ev.CreateCreditsCategory(("Synapse Server"));
            ev.CreateCreditsEntry("Dimenzio", "Creator, Maintainer", "Synapse Server", CreditColors.Red600);
            ev.CreateCreditsEntry("Helight", "Maintainer", "Synapse Server", CreditColors.Red600);
            ev.CreateCreditsEntry("MineTech13", "Useless Sys Admin", "Synapse Server", CreditColors.Yellow300);
            ev.CreateCreditsEntry("moelrobi", "Former-Maintainer", "Synapse Server", CreditColors.Gray);
            ev.CreateCreditsEntry("Mika", "Contributor", "Synapse Server", CreditColors.Blue100);
            ev.CreateCreditsEntry("AlmightyLks", "Contributor", "Synapse Server", CreditColors.Blue100);
            ev.CreateCreditsEntry("TheVoidNebula", "Contributor", "Synapse Server", CreditColors.Blue100);
            ev.CreateCreditsEntry("PintTheDragon", "Contributor", "Synapse Server", CreditColors.Blue100);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Info($"Scene changed to {scene.name}");
            try
            {
                switch (scene.name)
                {
                    case "Facility":
                        {
                            //var gameObject = new GameObject();
                            //gameObject.AddComponent<SynapseBackgroundWorker>();
                            break;
                        }

                    case "Loader":
                        {
                            if (SteamClient.IsLoggedOn)
                            {
                                Client.Get.PlayerName = SteamClient.Name;
                                Logger.Info($"Changed current prefered name to {Client.Get.PlayerName}");
                            }

                            SynapseCentral.Get.ConnectCentralServer();
                            break;
                        }

                    case "NewGameMenu":
                        {
                            Logger.Info("Changed to Game-Menu");
                            CentralAuthManager.InitAuth();
                            CentralAuthManager.Authentication();
                            break;
                        }

                    default: break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            SynapseEvents.InvokeSceneLoad(scene);
        }

        public void MainReceivePipelineData(PipelinePacket packet)
        {
            switch (packet.PacketId)
            {
                case ConnectionSuccessfulPacket.ID:
                    {
                        ConnectionSuccessfulPacket.Decode(packet, out var clientMods);
                        Logger.Info("WelcomePacket: " + packet.AsString());
                        var salt = new byte[32];
                        for (var i = 0; i < 32; i++) salt[i] = 0x00;
                        var jwt = JsonWebToken.DecodeToObject<ClientConnectionData>(AuthPatches.synapseSessionToken, "", false);
                        var sessionBytes = Encoding.UTF8.GetBytes(jwt.Session);
                        var key = new byte[32];
                        for (var i = 0; i < 24; i++) key[i] = sessionBytes[i];
                        for (var i = 24; i < 32; i++) key[i] = 0x00;

                        QueryProcessor.Localplayer.Key = key;
                        QueryProcessor.Localplayer.CryptoManager.ExchangeRequested = true;
                        QueryProcessor.Localplayer.CryptoManager.EncryptionKey = key;
                        QueryProcessor.Localplayer.Salt = salt;
                        QueryProcessor.Localplayer.ClientSalt = salt;
                        ClientPipeline.Invoke(PipelinePacket.From(1, "Client connected successfully"));
                        ClientBepInExPlugin.Get.ModLoader.ActivateForServer(clientMods); // Just activate for all for now
                        SynapseEvents.InvokeConnectionSuccessful();
                        SharedBundleManager.LogLoaded();
                        break;
                    }

                case RoundStartPacket.ID:
                    API.Events.SynapseEvents.InvokeRoundStart();
                    break;

                case RedirectPacket.ID:
                    RedirectPacket.Decode(packet, out var target);
                    Client.Get.Redirect(target);
                    break;
            }
        }
    }
}
