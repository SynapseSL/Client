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

            File.WriteAllText("id_rsa", id);
            File.WriteAllText("id_rsa.pub", pub);
        }

        private static string[] read()
        {
            if (!File.Exists("id_rsa") || !File.Exists("id_rsa.pub"))
            {
                generate();
            }

            var privatekey = File.ReadAllText("id_rsa");
            var publickey = File.ReadAllText("id_rsa.pub");
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
                        name = SynapseClientPlugin.name,
                        publicKey = read()[0],
                        mac = GetMac(),
                        pcName = GetPcName()
                    }));

            var registrationResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseString);
            File.WriteAllText("user.dat", registrationResponse.uuid);
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
                        name = SynapseClientPlugin.name,
                        uuid = File.ReadAllText("user.dat"),
                        publicKey = read()[0],
                        privateKey = read()[1],
                        mac = GetMac(),
                        pcName = GetPcName()
                    }));
            File.WriteAllText("certificate.dat", responseString);
        }

        public static string Session(string targetAddress)
        {
            var cert = File.ReadAllText("certificate.dat");
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