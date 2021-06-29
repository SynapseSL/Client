using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using SynapseClient.Models;

namespace SynapseClient.API
{
    public class SynapseServerList
    {
        public static SynapseServerList Get => Client.Get.SynapseServerList;

        internal SynapseServerList() { }

        private readonly WebClient _webClient = new WebClient();
        
        public List<SynapseServerEntry> ServerCache { get; internal set; }= new List<SynapseServerEntry>();

        public void Download()
        {
            var response = _webClient.DownloadString(ClientBepInExPlugin.Get.ServerListServer + "/serverlist");
            ServerCache = JsonConvert.DeserializeObject<List<SynapseServerEntry>>(response);
        }

        public SynapseServerEntry ResolveIdAddress(string address)
        {
            var uid = address.Replace(":0", "");
            return ServerCache.First(x => x.Id == uid);
        }
        
        public void AddServer(ServerFilter filter, SynapseServerEntry entry)
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
}