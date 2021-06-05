namespace SynapseClient.Models
{
    public class CertificateRequest
    {
        public string name { get; set; }
        public string uuid { get; set; }
        public string publicKey { get; set; }
        public string privateKey { get; set; }
        public string pcName { get; set; }
        public string mac { get; set; }
    }
}