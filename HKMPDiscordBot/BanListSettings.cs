using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace HKMPDiscordBot
{
    internal class BanListSettings
    {
        public BanList BanList = new BanList();

        public static BanListSettings Instance;
        public static SettingsLoader<BanList> BanListLoader;
        public void Save() {
            BanListLoader.Save(Instance.BanList);
        }
        public static void Initialise()
        {
            Instance = new BanListSettings();
            BanListLoader = new SettingsLoader<BanList>("banlistv2.json");
            Instance.BanList = BanListLoader.Load();
        }
    }
}
