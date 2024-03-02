using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace HKMPDiscordBot.Commands
{
    internal class PhrasesCommand : Command
    {
        public override List<string> AliasNames => new List<string> { "?phrases" , "./phrases"};


        public void AddPhrase(BotInstance botInstance,string phrase) {
            if (BanListSettings.Instance.BanList.Contains(phrase))
            {
                throw new Exception($"Banlist Already contains the phrase `{phrase}`");
            }
            BanListSettings.Instance.BanList.Add(phrase, false);
            BanListSettings.Instance.Save();
            var message = $"Added `{phrase}` to Banned Phrases list.";
            Program.Instance.SendResponseMessageToAdmin(botInstance, $"{message}");
        }
        public void RemovePhrase(BotInstance botInstance, string phrase)
        {
            BanListSettings.Instance.BanList.Remove(phrase);
            BanListSettings.Instance.Save();
            var message = $"Removed `{phrase}` from Banned Phrases list.";
            Program.Instance.SendResponseMessageToAdmin(botInstance, $"{message}");
        }
        public void SoftBan(BotInstance botInstance,string phrase)
        {
            var foundPhrase = BanListSettings.Instance.BanList.Find(phrase);
            if (foundPhrase != null)
            {
                foundPhrase.IsHardFilter = false;
                BanListSettings.Instance.Save();
                var message = $"Added `{phrase}` to Soft Banned Phrases list.";
                Program.Instance.SendResponseMessageToAdmin(botInstance, $"{message}");
            }
            else
            {
                throw new Exception($"Banlist does not contain the phrase `{phrase}`");
            }
        }
        public void HardBan(BotInstance botInstance,string phrase)
        {
            var foundPhrase = BanListSettings.Instance.BanList.Find(phrase);
            if (foundPhrase != null)
            {
                foundPhrase.IsHardFilter = true;
                BanListSettings.Instance.Save();
                var message = $"Added `{phrase}` to Hard Banned Phrases list.";
                Program.Instance.SendResponseMessageToAdmin(botInstance, $"{message}");
            }
            else
            {
                throw new Exception($"Banlist does not contain the phrase `{phrase}`");
            }
        }

        public override void Execute(BotInstance botInstance, List<string> args, SocketMessage message)
        {
            switch (args[1])
            {
                case "get":
                    Program.Instance.SendResponseMessageToAdmin(botInstance,
                        $"Banned Phrases list : \n ```{BanListSettings.Instance.BanList.ListPhrases()}``` \n");
                    break;
                case "add":
                    AddPhrase(botInstance, args[2]);
                    break;
                case "remove":
                    RemovePhrase(botInstance, args[2]);
                    break;
                case "hardban":
                    HardBan(botInstance, args[2]);
                    break;
                case "softban":
                    SoftBan(botInstance, args[2]);
                    break;
                default:
                    break;
            }
        }

        public override string Help()
        {
            return $"Phrases Command\n Manage the content filter phrases\n" +
                $"Usage :\n" +
                $"```?phrases get```" +
                $"```?phrases (add|remove|hardban|softban) <phrase>```\n";
        }
    }
}
