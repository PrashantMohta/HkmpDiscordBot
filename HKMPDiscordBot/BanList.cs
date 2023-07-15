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
        public static SettingsLoader<BanList> BanListLoader;
        public void Save() { 
            BanList.BanListLoader.Save(Instance);
        }
        public static void Initialise()
        {
            BanList.BanListLoader = new SettingsLoader<BanList>("banlist.json");
            BanList.Instance = BanList.BanListLoader.Load();
        }
    }
}
