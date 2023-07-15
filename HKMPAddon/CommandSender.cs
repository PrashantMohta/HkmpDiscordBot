using Hkmp.Api.Command.Server;
using Webhooks;

namespace DiscordIntegrationAddon
{
    internal class CommandSender : ICommandSender
    {
        public bool IsAuthorized => IsSystem;

        public CommandSenderType Type => CommandSenderType.Console;

        public bool IsSystem = true;

        public void SendMessage(string message)
        {
            Server.webhookClient.Send(new WebhookData { 
                UserName = Settings.Instance.Name,
                CurrentScene = FlavorStrings.GetBotLocationMessage(),
                Message = message,
                IsSystem = IsSystem,
                ServerId = Settings.Instance.ServerId
            });
        }
    }
}
