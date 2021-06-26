using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using MS.Internal.Xml.XPath;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Swan.Formatters;
using SynapseClient.Models;
using SynapseClient.Patches;

namespace SynapseClient
{
    public static class SynapseCentral
    {
        private const int RsaKeySize = 2048;

        public static string CachedSession { get; internal set; }
        
        private static AsymmetricCipherKeyPair GetKeyPair()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var secureRandom = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom, RsaKeySize);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }
        
        public static void ConnectCentralServer()
        {
            Logger.Info("Connecting to Synapse Central-Server");
            var cert = Path.Combine(Client.ApplicationDataDir(), "certificate.pub");
            var user = Path.Combine(Client.ApplicationDataDir(), "user.dat");
            try
            {
                if (File.Exists(user) && File.Exists(cert))
                {
                    //Logged in
                    Logger.Info(File.ReadAllText(cert));
                    Client.isLoggedIn = true;
                }
                else if (File.Exists(user))
                {
                    SynapseCentral.Certificate();
                    Client.isLoggedIn = true;
                }
                else
                {
                    Client.isLoggedIn = false;
                    var thread = new Thread(DoRegisterAsync);
                    thread.Start();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public static void DoRegisterAsync()
        {
            try
            {
                SynapseCentral.Register();
                SynapseCentral.Certificate();
                Client.isLoggedIn = true;
                Logger.Info("Central-Authentication / Registration complete");
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        private static void Generate()
        {    
            var idf = Path.Combine(Client.ApplicationDataDir(), "id_rsa");
            var pubf = Path.Combine(Client.ApplicationDataDir(), "id_rsa.pub");
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

        private static string[] Read()
        {
            var id = Path.Combine(Client.ApplicationDataDir(), "id_rsa");
            var pub = Path.Combine(Client.ApplicationDataDir(), "id_rsa.pub");
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


        public static void Register()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Content-Type", "application/json");
            var responseString = webClient.UploadString(Client.CentralServer + "/user/register",
                JsonConvert.SerializeObject(
                    new RegistrationRequest
                    {
                        name = Client.name,
                        publicKey = Read()[0],
                        mac = GetMac(),
                        pcName = GetPcName()
                    }));

            var registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseString);
            File.WriteAllText(Path.Combine(Client.ApplicationDataDir(), "user.dat"), registrationResponse.uuid);
        }

        public static void Certificate()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Content-Type", "application/json");
            var responseString = webClient.UploadString(Client.CentralServer + "/user/certificate",
                JsonConvert.SerializeObject(
                    new CertificateRequest()
                    {
                        name = Client.name,
                        uuid = File.ReadAllText(Path.Combine(Client.ApplicationDataDir(), "user.dat")),
                        publicKey = Read()[0],
                        privateKey = Read()[1],
                        mac = GetMac(),
                        pcName = GetPcName()
                    }));
            File.WriteAllText(Path.Combine(Client.ApplicationDataDir(), "certificate.dat"), responseString);
        }

        public static string Session(string targetAddress)
        {
            var cert = File.ReadAllText(Path.Combine(Client.ApplicationDataDir(), "certificate.dat"));
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("X-Target-Server", targetAddress);
            var responseString = webClient.UploadString(Client.CentralServer + "/user/session", cert);
            CachedSession = responseString;
            return responseString;
        }

        //Yes, just a session with admin audience which is not cached
        public static string AdminSession()
        {
            var cert = File.ReadAllText(Path.Combine(Client.ApplicationDataDir(), "certificate.dat"));
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("X-Target-Server", "Admin");
            var responseString = webClient.UploadString(Client.CentralServer + "/user/session", cert);
            return responseString;
        }

        public static void Report(string targetId, string reason)
        {
            var adminSession = AdminSession();
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Authorization", $"Bearer {adminSession}"); 
            webClient.UploadString(Client.CentralServer + $"/public/{targetId}/report", reason);
        }
        
        private static string GetMac()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where( nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select( nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault() ?? "Unknown";
        }

        private static string GetPcName()
        {
            return Environment.MachineName ?? "Unknown";
        }

        public static StrippedUser Resolve(string uid)
        {
            var webclient = new WebClient();
            webclient.Headers["Authorization"] = $"Bearer {CachedSession}";
            var url = Client.CentralServer + $"/public/{uid}";
            Logger.Info(url);
            var response = webclient.DownloadString(url);
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