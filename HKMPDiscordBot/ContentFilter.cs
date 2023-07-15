using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webhooks;

namespace HKMPDiscordBot
{
    internal abstract class ContentFilter
    {
        public abstract WebhookData Process(BotInstance bot, WebhookData message);
    }
    internal class AutoBan : ContentFilter
    {
        public bool shouldFilter(string username, string message, out bool isUsername, out string offendingPhrase)
        {
            isUsername = false;
            foreach (var phrase in BanList.Instance.phrases)
            {
                if (username.Contains(phrase))
                {
                    isUsername = true;
                    offendingPhrase = phrase;
                    return username.Contains(phrase);

                }
                if (message.Contains(phrase))
                {
                    offendingPhrase = phrase;
                    return message.Contains(phrase);
                }
            }
            offendingPhrase = "";
            return false;
        }
        public override WebhookData Process(BotInstance bot,WebhookData w)
        {
            string message = w.Message;
            string username = w.UserName;
            string remark = "No mind to think";
            string phrase;
            bool isUsername;
            string penaltyCommand = "ban";
            string location = "The Abyss";
            if (this.shouldFilter(username, message, out isUsername , out phrase))
            {
                if (isUsername) {
                    penaltyCommand = "kick";
                    location = "The Howling Cliffs";
                    Program.Instance.SendResponseMessageToAdmin(bot, $"✨ {remark} ✨\n Banishing vessel `{w.UserName}`  into the Howling Cliffs.\n for use of phrase `{phrase}` in username : \n || {username} ||");
                } else
                {
                    remark = "No voice to cry suffering";
                    Program.Instance.SendResponseMessageToAdmin(bot, $"✨ {remark} ✨\n Banishing vessel `{w.UserName}`  into the Abyss.\n for use of phrase `{phrase}` in message : \n || {message} ||");
                }

                Program.Instance.SendToHKMPAddon(bot, new WebhookData
                {
                    UserName = $"{bot.BotName}",
                    CurrentScene = $"{location}",
                    Message = $"/{penaltyCommand} {username}",
                    IsSystem = true,
                    ServerId = bot.ServerId
                });

                return new WebhookData
                {
                    UserName = $"{bot.BotName}",
                    CurrentScene = $"{location}",
                    Message = $"✨ {remark} ✨\n Banishing vessel `{username}` into {location}.",
                    IsSystem = false,
                    ServerId = bot.ServerId
                };
            }
            return w;
        }
    }
}
