namespace SynapseClient.Models
{
    public class RegistrationRequest
    {
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [Newtonsoft.Json.JsonProperty("pcName")]
        public string PcName { get; set; }

        [Newtonsoft.Json.JsonProperty("mac")]
        public string Mac { get; set; }
    }
}