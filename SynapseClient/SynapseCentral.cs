using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using SynapseClient.API;
using SynapseClient.Models;

namespace SynapseClient
{
    public class SynapseCentral
    {
        internal SynapseCentral() { }

        public static SynapseCentral Get => Client.Get.Central;

        private const int RsaKeySize = 2048;

        public string CachedSession { get; internal set; }
        
        private AsymmetricCipherKeyPair GetKeyPair()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var secureRandom = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom, RsaKeySize);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }
        
        public async void ConnectCentralServer()
        {
            Logger.Info("Connecting to Synapse Central-Server");
            var cert = Path.Combine(Computer.Get.ApplicationDataDir, "certificate.pub");
            var user = Path.Combine(Computer.Get.ApplicationDataDir, "user.dat");
            try
            {
                if (File.Exists(user) && File.Exists(cert))
                {
                    //Logged in
                    Logger.Info(File.ReadAllText(cert));
                    Client.Get.IsLoggedIn = true;
                }
                else if (File.Exists(user))
                {
                    await Certificate();
                    Client.Get.IsLoggedIn = true;
                }
                else
                {
                    Client.Get.IsLoggedIn = false;
                    var thread = new Thread(DoRegisterAsync);
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public async void DoRegisterAsync()
        {
            try
            {
                await Register();
                await Certificate();
                Client.Get.IsLoggedIn = true;
                Logger.Info("Central-Authentication / Registration complete");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        private void Generate()
        {    
            var idf = Path.Combine(Computer.Get.ApplicationDataDir, "id_rsa");
            var pubf = Path.Combine(Computer.Get.ApplicationDataDir, "id_rsa.pub");
            var pair = GetKeyPair();
            TextWriter textWriter1 = new StringWriter();
            var pemWriter1 = new PemWriter(textWriter1);
            pemWriter1.WriteObject((RsaKeyParameters) pair.Public);
            pemWriter1.Writer.Flush();
            var pub = textWriter1.ToString();

            TextWriter textWriter2 = new StringWriter();
            var pemWriter2 = new PemWriter(textWriter2);
            pemWriter2.WriteObject((RsaKeyParameters) pair.Private);
            pemWriter2.Writer.Flush();
            var id = textWriter2.ToString();

            File.WriteAllText(idf, id);
            File.WriteAllText(pubf, pub);
        }

        private string[] Read()
        {
            var id = Path.Combine(Computer.Get.ApplicationDataDir, "id_rsa");
            var pub = Path.Combine(Computer.Get.ApplicationDataDir, "id_rsa.pub");
            if (!File.Exists(id) || !File.Exists(pub))
            {
                Generate();
            }

            var privatekey = File.ReadAllText(id);
            var publickey = File.ReadAllText(pub);
            var pr = Base64.ToBase64String(Encoding.UTF8.GetBytes(privatekey));
            var pu = Base64.ToBase64String(Encoding.UTF8.GetBytes(publickey));
            return new[] {pu, pr};
        }


        public async Task Register()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Content-Type", "application/json");
            var responseString = await webClient.UploadStringTaskAsync(ClientBepInExPlugin.CentralServer + "/user/register",
                JsonConvert.SerializeObject(
                    new RegistrationRequest
                    {
                        Name = Client.Get.PlayerName,
                        PublicKey = Read()[0],
                        Mac = Computer.Get.Mac,
                        PcName =  Computer.Get.PcName
                    }));

            var registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseString);
            File.WriteAllText(Path.Combine(Computer.Get.ApplicationDataDir, "user.dat"), registrationResponse.Uuid);
        }

        public async Task Certificate()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Content-Type", "application/json");
            var responseString = await webClient.UploadStringTaskAsync(new Uri(ClientBepInExPlugin.CentralServer + "/user/certificate"),
                JsonConvert.SerializeObject(
                    new CertificateRequest()
                    {
                        Name = Client.Get.PlayerName,
                        Uuid = File.ReadAllText(Path.Combine(Computer.Get.ApplicationDataDir, "user.dat")),
                        PublicKey = Read()[0],
                        PrivateKey = Read()[1],
                        Mac = Computer.Get.Mac,
                        PcName = Computer.Get.PcName
                    }));
            File.WriteAllText(Path.Combine(Computer.Get.ApplicationDataDir, "certificate.dat"), responseString);
        }

        public async Task<string> Session(string targetAddress)
        {
            var cert = File.ReadAllText(Path.Combine(Computer.Get.ApplicationDataDir, "certificate.dat"));
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("X-Target-Server", targetAddress);
            var responseString = await webClient.UploadStringTaskAsync(ClientBepInExPlugin.CentralServer + "/user/session", cert);
            CachedSession = responseString;
            return responseString;
        }

        //Yes, just a session with admin audience which is not cached
        public async Task<string> AdminSession()
        {
            var cert = File.ReadAllText(Path.Combine(Computer.Get.ApplicationDataDir, "certificate.dat"));
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("X-Target-Server", "Admin");
            var responseString = await webClient.UploadStringTaskAsync(ClientBepInExPlugin.CentralServer + "/user/session", cert);
            return responseString;
        }

        /// <summary>
        /// Sends a Report in the name of the Player to the Central Server
        /// </summary>
        /// <param name="targetUserId">The UserID (123@Synapse) of the Player that should be reported</param>
        /// <param name="reason">The Reason for reporting this player</param>
        /// <returns></returns>
        public async Task Report(string targetUserId, string reason)
        {
            var adminSession = AdminSession();
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Authorization", $"Bearer {adminSession}"); 
            await webClient.UploadStringTaskAsync(new Uri(ClientBepInExPlugin.CentralServer + $"/public/{targetUserId}/report"), reason);
        }

        public async Task<StrippedUser> Resolve(string uid)
        {
            var webclient = new WebClient();
            webclient.Headers["Authorization"] = $"Bearer {CachedSession}";
            var url = ClientBepInExPlugin.CentralServer + $"/public/{uid}";
            Logger.Info(url);
            var response = await webclient.DownloadStringTaskAsync(url);
            Logger.Info(response);
            var user = JsonConvert.DeserializeObject<StrippedUser>(response);
            return user;
        }
    }
    
    public class StrippedUser {
        
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }
        
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }
        
        [Newtonsoft.Json.JsonProperty("groups")]
        public List<GlobalSynapseGroup> Groups { get; set; }
    }
    
    public class GlobalSynapseGroup
    {
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("color")]
        public string Color { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("hidden")]
        public bool Hidden { get; set; } = false;

        [Newtonsoft.Json.JsonProperty("remoteAdmin")]
        public bool RemoteAdmin { get; set; } = false;

        [Newtonsoft.Json.JsonProperty("permissions")]
        public List<string> Permissions { get; set; } = new List<string>() { };

        [Newtonsoft.Json.JsonProperty("kickable")]
        public bool Kickable { get; set; } = true;

        [Newtonsoft.Json.JsonProperty("bannable")]
        public bool Bannable { get; set; } = true;

        [Newtonsoft.Json.JsonProperty("kick")]
        public bool Kick { get; set; } = false;

        [Newtonsoft.Json.JsonProperty("ban")]
        public bool Ban { get; set; } = false;

        [Newtonsoft.Json.JsonProperty("staff")]
        public bool Staff { get; set; } = false;
    }
}