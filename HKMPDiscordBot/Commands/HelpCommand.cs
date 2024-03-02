using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKMPDiscordBot.Commands
{
    internal class HelpCommand : Command
    {
        public override List<string> AliasNames => new List<string> { "?help" };

        public override void Execute(BotInstance botInstance, List<string> args, SocketMessage message)
        {
            var m = CommandManager.ListCommands();
            Program.Instance.SendResponseMessageToAdmin(botInstance, $"{m} \n HKMP commands will be passed through if you use ```.``` to prefix it.");
        }

        public override string Help()
        {
            return $"Help Command \n Shows this message \n Usage : \n ```?help```\n";
        }
    }
}
