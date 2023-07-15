﻿using Discord;
using System;
using Webhooks;

namespace HKMPDiscordBot
{
    internal partial class Program
    {


        public void SendToHKMPAddon(BotInstance bot, WebhookData Payload)
        {
            if (bot.IsMuted) { return; }
            try
            {
                bot.webhookClient.Send(Payload);
            }
            catch (Exception ex)
            {
                SendErrorMessageToAdmin(bot,ex.ToString());
            }
        }

        public async void SendErrorMessageToAdmin(BotInstance bot, string error)
        {

            var embed = new EmbedBuilder()
                .WithColor(new Color(50, 0, 0))
                .WithAuthor($"{bot.BotName} Says")
                .WithDescription(error);
            if (bot.adminChannel == null)
            {
                bot.adminChannel = await _client.GetChannelAsync(bot.AdminChannelId) as IMessageChannel;
            }
            await bot.adminChannel!.SendMessageAsync(null, false, embed.WithCurrentTimestamp().Build());

        }
        public async void SendResponseMessageToAdmin(BotInstance bot, string message)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithAuthor($"{bot.BotName} Says")
                .WithDescription(message);

            if (bot.adminChannel == null)
            {
                bot.adminChannel = await _client.GetChannelAsync(bot.AdminChannelId) as IMessageChannel;
            }
            await bot.adminChannel!.SendMessageAsync(null, false, embed.WithCurrentTimestamp().Build());

        }

        public async void SendResponseMessageToUsers(BotInstance bot, WebhookData w)
        {
            if (bot.IsMuted) { return; }
            if (bot.mainChannel == null)
            {
                bot.mainChannel = await _client.GetChannelAsync(bot.ChannelId) as IMessageChannel;
            }
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithAuthor($"{w.UserName} Says")
                .WithDescription(w.Message)
                .WithFooter($"From {w.CurrentScene}");
            await bot.mainChannel!.SendMessageAsync(null, false, embed.Build());
        }

    }
}
