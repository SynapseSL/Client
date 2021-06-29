using Newtonsoft.Json;

namespace SynapseClient.Models
{
    public class SynapseServerEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("onlinePlayers")]
        public int OnlinePlayers { get; set; }

        [JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }

        [JsonProperty("pastebin")]
        public string Pastebin { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("whitelist")]
        public bool Whitelist { get; set; } = false;

        [JsonProperty("modded")]
        public bool Modded { get; set; } = true;

        [JsonProperty("friendlyFire")]
        public bool FriendlyFire { get; set; } = true;

        [JsonProperty("officialCode")]
        public byte OfficialCode { get; set; } = byte.MinValue;
    }
}
