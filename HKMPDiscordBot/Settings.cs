using Discord;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utils;
using Webhooks;

namespace HKMPDiscordBot
{
    public class BotInstance
    {
        public string ServerAlias = "HKMP Server";
        public int ServerId = 0;
        public string HkmpAddonWebhook = "http://localhost:3000/";

        public ulong GuildId = 0;
        public ulong ChannelId = 0;
        public ulong AdminChannelId = 0;

        public string BotName = "BotSeeker";
        public bool IsMuted = false;

        [JsonIgnore]
        internal IMessageChannel mainChannel { get; set; }
        [JsonIgnore]
        internal IMessageChannel adminChannel { get; set; }
        [JsonIgnore]
        internal WebhookClient webhookClient { get; set; }
        [JsonIgnore]
        internal SlashCommands slashCommands { get; set; }
    }
    internal class Settings 
    {
        public List<BotInstance> bots = new List<BotInstance> { new BotInstance() };
        public string Token = "";
        public string HostName = "localhost";
        public string Port = "3002";

        public static Settings Instance;
        public static void Initialise()
        {
            Settings.Instance = new SettingsLoader<Settings>("discord_bot.json").Load();
        }

        public BotInstance GetBotInstanceByServerId(int ServerId)
        {
            return bots.Find(x => x.ServerId == ServerId);
        }
        public BotInstance GetBotInstanceByChannelId(ulong ChannelId)
        {
            return bots.Find(x => x.ChannelId == ChannelId || x.AdminChannelId == ChannelId);
        }
    }
}
