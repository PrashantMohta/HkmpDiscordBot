using Hkmp.Api.Command.Server;
using Webhooks;

namespace DiscordIntegrationAddon
{
    internal class CommandSender : ICommandSender
    {
        public bool IsAuthorized => IsSystem;

        public CommandSenderType Type => CommandSenderType.Console;

        public bool IsSystem = true;
        public bool IsSilent = false;
        public void SendMessage(string message)
        {
            if (!IsSilent) { 
                Server.webhookClient.Send(new WebhookData
                {
                    Message = message,
                    CurrentScene = Constants.BOTSEEKER_LOCATION,
                    IsSystem = IsSystem,
                    ServerId = Settings.Instance.ServerId
                });
            }
        }
    }
}
