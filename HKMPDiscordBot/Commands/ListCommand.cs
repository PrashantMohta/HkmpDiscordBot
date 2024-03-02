using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webhooks;

namespace HKMPDiscordBot.Commands
{
    internal class ListCommand : Command
    {
        public override List<string> AliasNames => new List<string> { "?list", "./list" };

        public override void Execute(BotInstance botInstance, List<string> args, SocketMessage message)
        {
            Program.Instance.SendToHKMPAddon(botInstance, new WebhookData
            {
                UserName = message.Author.Username,
                CurrentScene = message.Channel.Name,
                Message = "/list",
                IsSystem = true,
                ServerId = botInstance.ServerId
            });
        }

        public override string Help()
        {
            return $"List Command : \n" +
                $"Lists the currently connected players" +
                $" Usage : ```" +
                $"Type ./list or ?list" +
                $"```\n";
        }
    }
}
