using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using SynapseClient.Models;

namespace SynapseClient
{
    public static class SynapseCentralAuth
    {
        private const int RsaKeySize = 2048;

        private static AsymmetricCipherKeyPair GetKeyPair()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var secureRandom = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom, RsaKeySize);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }

        private static void generate()
        {    
            var idf = Path.Combine(SynapseClient.ApplicationDataDir(), "id_rsa");
            var pubf = Path.Combine(SynapseClient.ApplicationDataDir(), "id_rsa.pub");
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

        private static string[] read()
        {
            var id = Path.Combine(SynapseClient.ApplicationDataDir(), "id_rsa");
            var pub = Path.Combine(SynapseClient.ApplicationDataDir(), "id_rsa.pub");
            if (!File.Exists(id) || !File.Exists(pub))
            {
                generate();
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
            var responseString = webClient.UploadString("https://central.synapsesl.xyz/user/register",
                JsonConvert.SerializeObject(
                    new RegistrationRequest
                    {
                        name = SynapseClient.name,
                        publicKey = read()[0],
                        mac = GetMac(),
                        pcName = GetPcName()
                    }));

            var registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseString);
            File.WriteAllText(Path.Combine(SynapseClient.ApplicationDataDir(), "user.dat"), registrationResponse.uuid);
        }

        public static void Certificate()
        {
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("Content-Type", "application/json");
            var responseString = webClient.UploadString("https://central.synapsesl.xyz/user/certificate",
                JsonConvert.SerializeObject(
                    new CertificateRequest()
                    {
                        name = SynapseClient.name,
                        uuid = File.ReadAllText(Path.Combine(SynapseClient.ApplicationDataDir(), "user.dat")),
                        publicKey = read()[0],
                        privateKey = read()[1],
                        mac = GetMac(),
                        pcName = GetPcName()
                    }));
            File.WriteAllText(Path.Combine(SynapseClient.ApplicationDataDir(), "certificate.dat"), responseString);
        }

        public static string Session(string targetAddress)
        {
            var cert = File.ReadAllText(Path.Combine(SynapseClient.ApplicationDataDir(), "certificate.dat"));
            var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "SynapseClient");
            webClient.Headers.Add("X-Target-Server", targetAddress);
            var responseString = webClient.UploadString("https://central.synapsesl.xyz/user/session", cert);
            return responseString;
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
    }
}