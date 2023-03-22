using Hkmp.Api.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordIntegrationAddon
{
    internal static class FlavorStrings
    {
        public static Random random = new Random();
        public static List<string> ConnectMessages = new List<string> {
            "{player.Username} has joined",
            "{player.Username} has connected",
            "This Server is graced by the pale being {player.Username}",
            "{player.Username} has risen from the Abyss",
            "{player.Username} has dashed their way in",
        };

        public static List<string> BotLocations = new List<string> {
            "The WaterWastes",
            "The BotHome",
            "The Hive",
            "The Washer's Spire",
            "MenderBot's Home"
        };


        public static string GetConnectMessage(IServerPlayer player)
        {
            var message = ConnectMessages[random.Next(0, ConnectMessages.Count)];
            return message.Replace("{player.Username}", player.Username);
        }
        public static string GetBotLocationMessage()
        {
            var message = BotLocations[random.Next(0, BotLocations.Count)];
            return message;
        }
    }
}
