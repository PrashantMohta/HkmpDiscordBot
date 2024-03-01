using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordIntegrationAddon
{ 

    internal static class CachedBanList
    {
        private static int TTL = 60;
        private static DateTime _lastLoaded = DateTime.MinValue;
        private static List<BanListItem> _cache = new List<BanListItem>();
        public static List<BanListItem> Current
        {
            get
            {
                if(DateTime.Now > _lastLoaded.AddSeconds(TTL))
                {
                    LoadBanList();
                }
                return _cache;
            }
        }

        public static async void LoadBanList()
        {
            var response = await Server.webhookClient.Request("banlist/get");
            var contents = await response.Content.ReadAsStringAsync();
            try { 
                var res = JsonConvert.DeserializeObject<BanList>(contents, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                });
                _cache = res.Phrases;
                _lastLoaded = DateTime.Now;
            } catch (Exception ex) { 
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
