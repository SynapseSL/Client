using System;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Security;

namespace SynapseClient.ClientModule.Crypto;

public class Client
{
    public static void Main()
    {
        var random = new SecureRandom();
        var kpGenerator = new X25519KeyPairGenerator();
        var alice = kpGenerator.GenerateKeyPair();
        var bob = kpGenerator.GenerateKeyPair();

        var x25519 = new X25519Agreement();
        x25519.Init(alice.Private);
        
        var bytes = new byte[x25519.AgreementSize];
        x25519.CalculateAgreement(bob.Public, bytes, 0);
        
        Console.WriteLine(Convert.ToBase64String(bytes));

    }
}