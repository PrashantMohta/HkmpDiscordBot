using Utils;

namespace DiscordIntegrationAddon
{
    internal class Settings
    {

        public static string AddonName = "DiscordBot";
        public string Name = "BotSeeker";
        public string DiscordBotWebhook = "http://localhost:3002/";
        public string Port = "3000";

        public bool Locations = true;

        public static Settings Instance;
        public static void Initialise()
        {
            Settings.Instance = new SettingsLoader<Settings>("discord_integration_addon.json").Load();
        }
    }
}
