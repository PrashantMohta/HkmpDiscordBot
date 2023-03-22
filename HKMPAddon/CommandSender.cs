using Hkmp.Api.Command.Server;
using System.Collections.Generic;

namespace DiscordIntegrationAddon
{
    internal class CommandSender : ICommandSender
    {
        public bool IsAuthorized => IsSystem;

        public CommandSenderType Type => CommandSenderType.Console;

        public bool IsSystem = true;

        public void SendMessage(string message)
        {
            Server.Instance.SendToDiscord(
                new Dictionary<string, string>
                {
                    { "Username", Settings.Instance.Name},
                    { "CurrentScene",FlavorStrings.GetBotLocationMessage() },
                    { "Message", message },
                    { "IsSystem", IsSystem ? "true" : "false"}
                }
            );
        }
    }
}
