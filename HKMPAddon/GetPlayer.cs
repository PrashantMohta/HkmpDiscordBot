using Hkmp.Api.Command.Server;
using Hkmp.Api.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordIntegrationAddon
{
    internal class GetPlayer : IServerCommand
    {
        public GetPlayer()
        {
        }
        public bool AuthorizedOnly => true;

        public string Trigger => "/player";

        public string[] Aliases => new string[] { "/players" };

        public void Execute(ICommandSender commandSender, string[] arguments)
        {
            IServerPlayer player;
            if (arguments.Length == 3 && arguments[1].ToLower() == "authkey")
            {
                player = Server.Instance.GetPlayer(arguments[2]);
                var auth = player.AuthKey;
                commandSender.SendMessage($"Player `{arguments[2]}` has AuthKey `{auth}`");
            } else if (arguments.Length == 3 && arguments[1].ToLower() == "ip")
            {
                player = Server.Instance.GetPlayer(arguments[2]);
                var ip = player.IpAddressString;
                commandSender.SendMessage($"Player `{arguments[2]}` has IP `{ip}`");
            } else
            {
                commandSender.SendMessage($"Invalid arguments: {String.Join(",", arguments)}");
            }

        }
    }
}
