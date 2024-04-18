using Discord;
using System;
using Webhooks;

namespace HKMPDiscordBot
{
    internal partial class Program
    {
        public string LimitTo(string s,int limit = 4000)
        {
            return s.Length > limit ? s.Substring(0,limit) : s;
        }

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
                .WithDescription(LimitTo(error.Replace("*", "\\*")));
            try
            {
                bot.adminChannel ??= await _client.GetChannelAsync(bot.AdminChannelId) as IMessageChannel;
                await bot.adminChannel!.SendMessageAsync(null, false, embed.WithCurrentTimestamp().Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async void SendResponseMessageToAdmin(BotInstance bot, string message)
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithAuthor($"{bot.BotName} Says")
                .WithDescription(LimitTo(message.Replace("*","\\*")));

            try
            {
                bot.adminChannel ??= await _client.GetChannelAsync(bot.AdminChannelId) as IMessageChannel;
                await bot.adminChannel!.SendMessageAsync(null, false, embed.WithCurrentTimestamp().Build());
            }
            catch (Exception ex)
            {
                SendErrorMessageToAdmin(bot, ex.ToString());
            }

        }

        public async void SendResponseMessageToUsers(BotInstance bot, WebhookData w)
        {
            if (bot.IsMuted) { return; }
            var embed = new EmbedBuilder()
                .WithColor(new Color(0, 0, 0))
                .WithAuthor($"{w.UserName} Says")
                .WithDescription(LimitTo(w.Message.Replace("*", "\\*")))
                .WithFooter($"From {w.CurrentScene}");
            try {
                bot.mainChannel ??= await _client.GetChannelAsync(bot.ChannelId) as IMessageChannel;
                await bot.mainChannel!.SendMessageAsync(null, false, embed.Build());
            } catch (Exception ex) {
                SendErrorMessageToAdmin(bot, ex.ToString());
            }
        }

    }
}
