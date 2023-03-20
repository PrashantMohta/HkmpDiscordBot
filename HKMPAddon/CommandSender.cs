using Hkmp.Api.Command.Server;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

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
                    { "Username","HKMP" },
                    { "CurrentScene","Server" },
                    { "Message", message },
                    { "IsSystem", IsSystem ? "true" : "false"}
                }
            );
        }
    }
}
