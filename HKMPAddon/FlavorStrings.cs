using Hkmp.Api.Server;
using System;
using System.Collections.Generic;

namespace DiscordIntegrationAddon
{
    internal static class FlavorStrings
    {
        public static Random random = new Random();
        public static List<string> ConnectMessages = new List<string> {
            "{player.Username} has joined.",
            "{player.Username} has connected.",
            "This Server is graced by the pale being {player.Username}!",
            "{player.Username} has risen from the Abyss.",
            "{player.Username} has dashed their way in.",
            "{player.Username} didn't say Shaw when connecting!",
            "{player.Username} just arrived at {player.CurrentScene}!",
            "{player.Username} escaped from the collector's jar!",
            "Is it {player.Username} or is it Nosk Among us?"
        };

        public static Dictionary<string,List<string>> SceneConnectMessages = new Dictionary<string, List<string>>
        {
            {"Town",new List<string>{
                "{player.Username} might just settle down in Dirtmouth."
            }},
            {"Fungus1_15",new List<string>{
                "{player.Username} is painting with Sheo."
            }},
            {"Room_Colosseum_01",new List<string>{
                "The Colosseum of Fools welcomes it's newest challenger - {player.Username}.",
                "The Colosseum crowd laughs at {player.Username}'s nail swings!"
            }},
            {"Room_Colosseum_02",new List<string>{
                "The Colosseum of Fools welcomes it's newest challenger - {player.Username}.",
                "The Colosseum crowd laughs at {player.Username}'s nail swings!"
            }},
            {"Room_Colosseum_Bronze",new List<string>{
                "The Colosseum of Fools welcomes it's newest challenger - {player.Username}.",
                "The Colosseum crowd laughs at {player.Username}'s nail swings!"
            }},
            {"Room_Colosseum_Silver",new List<string>{
                "The Colosseum of Fools welcomes it's newest challenger - {player.Username}.",
                "The Colosseum crowd laughs at {player.Username}'s nail swings!"
            }},
            {"Room_Colosseum_Gold",new List<string>{
                "The Colosseum of Fools welcomes it's newest challenger - {player.Username}.",
                "The Colosseum crowd laughs at {player.Username}'s nail swings!"
            }},
            {"Waterways_03",new List<string>{
                "{player.Username} has found an endless supply of EGGs!"
            }},
            {"Room_Ouiji",new List<string>{
                "{player.Username} brings an EGG to JiJi."
            }},
            {"GG_Atrium",new List<string>{
                "{player.Username} has entered Godhome."
            }}
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
            var AllValidConnectMessages = new List<string>();
            AllValidConnectMessages.AddRange(ConnectMessages);
            if(SceneConnectMessages.TryGetValue(player.CurrentScene, out var sceneMessages))
            {
                AllValidConnectMessages.AddRange(sceneMessages);
            }
            var message = AllValidConnectMessages[random.Next(0, AllValidConnectMessages.Count)];
            var playerScene = Settings.Instance.Locations ? BetterRoomNames.GetRoomName(player.CurrentScene) : "████████████████████";
            return message.Replace("{player.CurrentScene}", playerScene).Replace("{player.Username}", player.Username);
        }
        public static string GetBotLocationMessage()
        {
            var message = BotLocations[random.Next(0, BotLocations.Count)];
            return message;
        }
    }
}
