using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static HKMPDiscordBot.BetterRoomNames;

namespace HKMPDiscordBot
{
    internal partial class Program
    {

        public void SendToHKMPAddon(Dictionary<string, string> Payload)
        {
            if (Settings.Instance.IsMuted) { return; }
            try
            {
                httpClient.PostAsync(Settings.Instance.HkmpAddonWebhook, new StringContent(JsonConvert.SerializeObject(Payload)));
            }
            catch (Exception ex)
            {
                SendErrorMessageToAdmin(ex.ToString());
            }
        }

        public async void SendErrorMessageToAdmin(string error)
        {

            var embed = new EmbedBuilder()
                .WithColor(new Color(50, 0, 0))
                .WithAuthor($"{Settings.Instance.Name} Says")
                .WithDescription(error);
            if (adminChannel == null)
            {
                adminChannel = await _client.GetChannelAsync(Settings.Instance.AdminChannelId) as IMessageChannel;
            }
            await adminChannel!.SendMessageAsync(null, false, embed.WithCurrentTimestamp().Build());

        }
        public async void SendResponseMessageToAdmin(string message)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithAuthor($"{Settings.Instance.Name} Says")
                .WithDescription(message);

            if (adminChannel == null)
            {
                adminChannel = await _client.GetChannelAsync(Settings.Instance.AdminChannelId) as IMessageChannel;
            }
            await adminChannel!.SendMessageAsync(null, false, embed.WithCurrentTimestamp().Build());

        }

        public async void SendResponseMessageToUsers(WebhookData w)
        {
            if (Settings.Instance.IsMuted) { return; }
            if (channel == null)
            {
                channel = await _client.GetChannelAsync(Settings.Instance.ChannelId) as IMessageChannel;
            }
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithAuthor($"{w.UserName} Says")
                .WithDescription(w.Message)
                .WithFooter($"From {GetRoomName(w.CurrentScene)}");
            await channel!.SendMessageAsync(null, false, embed.Build());
        }

    }
}
