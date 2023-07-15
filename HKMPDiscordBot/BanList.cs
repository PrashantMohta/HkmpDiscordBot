using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace HKMPDiscordBot
{
    internal class BanList
    {
        public List<string> phrases = new List<string>();

        public static BanList Instance;

        public static void Initialise()
        {
            BanList.Instance = new SettingsLoader<BanList>("banlist.json").Load();
        }
    }
}
