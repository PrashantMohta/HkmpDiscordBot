using Hkmp.Api.Command;
using Hkmp.Api.Command.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webhooks;

namespace DiscordIntegrationAddon
{
    internal class ReportPlayer : IServerCommand
    {
        public ReportPlayer()
        {
        }
        public bool AuthorizedOnly => false;

        public string Trigger => "/report";

        public string[] Aliases => new string[] { "/report-players" };

        public void Execute(ICommandSender commandSender, string[] arguments)
        {
            var reporter = "Console";
            if(commandSender.Type == CommandSenderType.Player)
            {
                var playerCommandSender = commandSender as IPlayerCommandSender;
                var player = Server.Instance.serverApi.ServerManager.GetPlayer(playerCommandSender.Id);
                if(player != null)
                {
                    reporter = player.Username;
                }
            }
            var reportedPlayer = Server.Instance.GetPlayer(arguments[1]);
            if (reportedPlayer != null) { 
                var auth = reportedPlayer.AuthKey;
                var ip = reportedPlayer.IpAddressString;

                Server.webhookClient.Send(new WebhookData
                {
                    Message = $"Player `{arguments[1]}` has been reported by {reporter}.\n AuthKey: `{auth}`\n Ip: `{ip}`. ```{String.Join(" ", arguments)}```",
                    CurrentScene = Constants.BOTSEEKER_LOCATION,
                    IsSystem = true,
                    ServerId = Settings.Instance.ServerId
                });
                commandSender.SendMessage($"Player `{arguments[1]}` has been reported.");
            } else
            {
                commandSender.SendMessage($"Player `{arguments[1]}` not found.");
            }

        }
    }
}
