﻿using Hkmp.Api.Command.Server;
using HKMPDiscordBot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace DiscordIntegrationAddon
{
    internal class CommandSender : ICommandSender
    {
        public bool IsAuthorized => true;

        public CommandSenderType Type => CommandSenderType.Console;

        public void SendMessage(string message)
        {
            Server.httpClient.PostAsync(Settings.Instance.DiscordBotWebhook, new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "Username","HKMP" },
                { "CurrentScene","Server" },
                { "Message", message },
                { "IsSystem", "true" }
            })));
        }
    }
}