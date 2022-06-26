using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;


namespace IdentityProtocol
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            TestSigning();
            TestDiffieHellman();
            TestCbcCipher();
        }

        public static void TestCbcCipher()
        {
            var secureRandom = new SecureRandom();
            var key = new byte[32];
            secureRandom.NextBytes(key);
            var keyParameter = new KeyParameter(key);

            var bytes = Encoding.UTF8.GetBytes("Hello World!");
            
            var aesEngine = new AesEngine();
            var cbc = new CbcBlockCipher(aesEngine);
            var cipher = new PaddedBufferedBlockCipher(cbc);

            Console.WriteLine(Encoding.UTF8.GetString(bytes));
            Console.WriteLine();
            cipher.Init(true, keyParameter);
            var encrypted = cipher.DoFinal(bytes);
            Console.WriteLine(Encoding.UTF8.GetString(encrypted));
            Console.WriteLine();
            cipher.Init(false, keyParameter);
            var decrypted = cipher.DoFinal(encrypted);
            Console.WriteLine(Encoding.UTF8.GetString(decrypted));            
            Console.WriteLine();
        }
        
        public static void TestSigning()
        {
            var random = new SecureRandom();
            var kpGenerator = new Ed25519KeyPairGenerator();
            kpGenerator.Init(new Ed25519KeyGenerationParameters(random));
            var pair = kpGenerator.GenerateKeyPair();

            var payload = Encoding.UTF8.GetBytes("Hello World!");
            
            var signer = new Ed25519Signer();
            signer.Init(true, pair.Private);
            signer.BlockUpdate(payload, 0, payload.Length);
            var signature = signer.GenerateSignature();
            Console.WriteLine(Convert.ToBase64String(signature));

            var verifier = new Ed25519Signer();
            verifier.Init(false, pair.Public);
            verifier.BlockUpdate(payload, 0, payload.Length);
            Console.WriteLine(verifier.VerifySignature(signature));
        }
        
        public static void TestDiffieHellman()
        {
            var random = new SecureRandom();
            var kpGenerator = new X25519KeyPairGenerator();
            kpGenerator.Init(new X25519KeyGenerationParameters(random));
            var alice = kpGenerator.GenerateKeyPair();
            var bob = kpGenerator.GenerateKeyPair();

            Console.WriteLine(Convert.ToBase64String(CalculateSharedSecret(alice.Private, bob.Public)));
            Console.WriteLine(Convert.ToBase64String(CalculateSharedSecret(bob.Private, alice.Public)));
        }

        public static byte[] CalculateSharedSecret(ICipherParameters privateKey, ICipherParameters publicKey)
        {
            var x25519 = new X25519Agreement();
            x25519.Init(privateKey);
            var bytes = new byte[x25519.AgreementSize];
            x25519.CalculateAgreement(publicKey, bytes, 0);
            return bytes;
        }
    }
}