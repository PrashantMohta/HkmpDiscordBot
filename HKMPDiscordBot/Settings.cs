using Utils;

namespace HKMPDiscordBot
{
    internal class Settings 
    {

        public ulong ChannelId = 0;
        public ulong AdminChannelId = 0;
        public string HkmpAddonWebhook = "http://localhost:3000/";
        public string Token = "";
        public string HostName = "localhost";
        public string Port = "3002";
        public string Name = "BotSeeker";
        public bool IsMuted = false;
        public ulong GuildId = 0;

        public static Settings Instance;
        public static void Initialise()
        {
            Settings.Instance = new SettingsLoader<Settings>("discord_bot.json").Load();
        }
    }
}
