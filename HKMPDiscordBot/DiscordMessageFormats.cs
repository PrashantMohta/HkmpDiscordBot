﻿using Discord;
using System;
using Webhooks;

namespace HKMPDiscordBot
{
    internal partial class Program
    {

        public void SendToHKMPAddon(WebhookData Payload)
        {
            if (Settings.Instance.IsMuted) { return; }
            try
            {
                webhookClient.Send(Payload);
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
                .WithFooter($"From {w.CurrentScene}");
            await channel!.SendMessageAsync(null, false, embed.Build());
        }

    }
}
