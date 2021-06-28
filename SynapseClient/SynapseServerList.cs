using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using SynapseClient.API;

namespace SynapseClient
{
    public class SynapseServerList
    {
        private readonly WebClient _webClient = new WebClient();
        
        public List<SynapseServerEntry> ServerCache { get; internal set; }= new List<SynapseServerEntry>();


        public void Download()
        {
            var response = _webClient.DownloadString(Client.ServerListServer + "/serverlist");
            ServerCache = JsonConvert.DeserializeObject<List<SynapseServerEntry>>(response);
        }

        public SynapseServerEntry ResolveIdAddress(string address)
        {
            var uid = address.Replace(":0", "");
            return ServerCache.First(x => x.Id == uid);
        }
        
        public static void AddServer(ServerFilter filter, SynapseServerEntry entry)
        {
            var leakingObject = new LeakingObject<ServerListItem>
            {
                decorated = new ServerListItem
                {
                    ip = (String)entry.Id,
                    port = 0000,
                    players = (String)(entry.OnlinePlayers + "/" + entry.MaxPlayers),
                    info = (String)entry.Info,
                    pastebin = (String)entry.Pastebin,
                    version = (String)entry.Version,
                    whitelist = entry.Whitelist,
                    modded = entry.Modded,
                    friendlyFire = entry.FriendlyFire,
                    officialCode = entry.OfficialCode
                }
            };
            filter.FilteredListItems.Add(leakingObject.decorated);
            leakingObject.Dispose();
        }

        
    }
    
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