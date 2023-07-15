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
        public override WebhookData Process(BotInstance bot,WebhookData w)
        {
            var message = w.Message;
            
            foreach (var phrase in BanList.Instance.phrases)
            {
                if (message.Contains(phrase))
                {
                    Program.Instance.SendResponseMessageToAdmin(bot,$"✨ No voice to cry suffering ✨\n Banishing vessel `{w.UserName}`  into the Abyss.\n for use of phrase `{phrase}` in message : \n || {message} ||");
                    Program.Instance.SendToHKMPAddon(bot, new WebhookData
                    {
                        UserName = $"{bot.BotName}",
                        CurrentScene = "Abyss",
                        Message = $"/ban {w.UserName}",
                        IsSystem = true,
                        ServerId = bot.ServerId
                    });
                    return new WebhookData{
                        UserName = $"{bot.BotName}",
                        CurrentScene = "Abyss",
                        Message = $"✨ No voice to cry suffering ✨\n Banishing vessel `{w.UserName}` into the Abyss.",
                        IsSystem = false,
                        ServerId = bot.ServerId
                    };
                }
            }

            return w;
        }
    }
}
