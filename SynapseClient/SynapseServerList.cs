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
        private WebClient _webClient = new WebClient();
        
        public List<SynapseServerEntry> ServerCache { get; internal set; }= new List<SynapseServerEntry>();


        public void Download()
        {
            var response = _webClient.DownloadString(Client.ServerListServer + "/serverlist");
            ServerCache = JsonConvert.DeserializeObject<List<SynapseServerEntry>>(response);
        }

        public SynapseServerEntry ResolveIdAddress(string address)
        {
            var uid = address.Replace(":0", "");
            return ServerCache.First(x => x.id == uid);
        }
        
        public static void AddServer(ServerFilter filter, SynapseServerEntry entry)
        {
            var leakingObject = new LeakingObject<ServerListItem>();
            leakingObject.decorated = new ServerListItem
            {
                ip = (String) entry.id,
                port = 0000,
                players = (String) (entry.onlinePlayers + "/" + entry.maxPlayers),
                info = (String) entry.info,
                pastebin = (String) entry.pastebin,
                version = (String) entry.version,
                whitelist = entry.whitelist,
                modded = entry.modded,
                friendlyFire = entry.friendlyFire,
                officialCode = entry.officialCode
            };
            filter.FilteredListItems.Add(leakingObject.decorated);
            leakingObject.Dispose();
        }

        
    }
    
    public class SynapseServerEntry
    {
        public string id { get; set; }
        public string address { get; set; }
        public int onlinePlayers { get; set; }
        public int maxPlayers { get; set; }
        public string info { get; set; }
        public string pastebin { get; set; }
        public string version { get; set; }
        public bool whitelist { get; set; } = false;
        public bool modded { get; set; } = true;
        public bool friendlyFire { get; set; } = true;
        public byte officialCode { get; set; } = byte.MinValue;
            
    }
}