namespace SynapseClient.Models
{
    public class CertificateRequest
    {
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("uuid")]
        public string Uuid { get; set; }

        [Newtonsoft.Json.JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [Newtonsoft.Json.JsonProperty("privateKey")]
        public string PrivateKey { get; set; }

        [Newtonsoft.Json.JsonProperty("pcName")]
        public string PcName { get; set; }

        [Newtonsoft.Json.JsonProperty("mac")]
        public string Mac { get; set; }
    }
}