using System;
using System.Net.PeerToPeer;
using System.Text.RegularExpressions;

namespace DiscordIntegrationAddon
{

    internal class ContentFilter
    {
        internal static string SoftEvaluator(Match match)
        {
            var replacement = "";
            for(var i =0; i < match.Length;i++)
            {
                replacement += "*";
            }
            return replacement;
        }
        internal static bool CheckFilter(string text, out string offendingPhrase, out string censoredPhrase, out Boolean isHardFilter)
        {
            censoredPhrase = text;
            offendingPhrase = "";
            isHardFilter = false;
            Console.WriteLine($"CachedBanList.Current {CachedBanList.Current.Count}");
            foreach (var banListItem in CachedBanList.Current) {
                var match = Regex.Match(text, banListItem.Phrase, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine($"match.Success {banListItem.Phrase}");
                    censoredPhrase = Regex.Replace(text, banListItem.Phrase, SoftEvaluator, RegexOptions.IgnoreCase);
                    offendingPhrase = banListItem.Phrase;
                    isHardFilter = banListItem.IsHardFilter;
                    return true;
                }
            }
            return false;
        }

    }
}