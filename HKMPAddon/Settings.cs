using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace HKMPDiscordBot
{
    internal class Settings
    {
        public const string Name = "DiscordBot";
        public static Settings Instance;
        public string DiscordBotWebhook = "http://localhost:3002/";
        public string Port = "3000";

        public static void Load()
        {
            //load from file or give defaults and fail

            var currentDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            var filePath = Path.Combine(currentDirectory, "discord_integration_addon.json");
            Instance = new Settings();
            try
            {
                Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filePath));
            }
            catch
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(Instance));
            }
        }
    }
}
