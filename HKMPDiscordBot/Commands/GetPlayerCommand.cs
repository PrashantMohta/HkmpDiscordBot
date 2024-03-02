using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webhooks;

namespace HKMPDiscordBot.Commands
{
    internal class GetPlayerCommand : Command
    {
        public override List<string> AliasNames => new() { "?player" , "./player" };

        public override void Execute(BotInstance botInstance, List<string> args, SocketMessage message)
        {
            Program.Instance.SendToHKMPAddon(botInstance, new WebhookData
            {
                UserName = message.Author.Username,
                CurrentScene = message.Channel.Name,
                Message = $"/player {args[1]} {args[2]}",
                IsSystem = true,
                ServerId = botInstance.ServerId
            });
        }

        public override string Help()
        {
            return $"Get Player Command \n get the player's (authkey|ip). \n Usage : \n ```?player (authkey|ip) <username>```\n";
        }
    }
}
