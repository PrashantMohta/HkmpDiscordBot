using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace HKMPBot
{
    internal class Settings
    {
        public static Settings Instance;
        public ulong ChannelId = 0;
        public ulong AdminChannelId = 0;
        public string HkmpAddonWebhook = "http://localhost:3000/";
        public string Token = "";
        public string Port = "3002";

        public static void Load()
        {
            //load from file or give defaults and fail
            
            var currentDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            var filePath = Path.Combine(currentDirectory, "discord_bot.json");
            Instance = new Settings();
            try { 
                Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filePath));
            }
            catch
            {
                File.WriteAllText(filePath,JsonConvert.SerializeObject(Instance));
            }
        }
    }
}
