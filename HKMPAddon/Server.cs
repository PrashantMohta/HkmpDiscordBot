using Hkmp.Api.Command.Server;
using Hkmp.Api.Server;
using Hkmp.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace DiscordIntegrationAddon
{
    public class Server : ServerAddon
    {

        public static Server Instance { get; private set; }
        public override bool NeedsNetwork => false;

        protected override string Name => Settings.Name;

        protected override string Version => "v0.2";

        internal static readonly HttpClient httpClient = new HttpClient();

        internal static CommandSender commandSender = new CommandSender();

        public Server()
        {
            Instance = this;
        }

        public override void Initialize(IServerApi serverApi)
        {
            Settings.Load();
            HttpServer.url = $"http://*:{Settings.Instance.Port}/";
            Instance = this;

            //Reflection into HKMP to add our own ILogger so we can inspect the chat via it
            //Cursed but it works.
            Type LoggerR = typeof(IServerApi).Assembly.GetType("Hkmp.Logging.Logger");
            var loggersList = (List<ILogger>)LoggerR.GetField("Loggers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var li = new LogInterceptor();
            loggersList.Add(li);
            li.OnChatMessage += Li_OnChatMessage;
            ServerApi.CommandManager.RegisterCommand(new RedactLocations());
            //Start a HTTP server on another thread
            Thread thread1 = new Thread(HttpServer.Start);
            thread1.Start();
        }

        

        internal bool TryRunCommand(string message)
        {
            //Reflection into HKMP to get the ban command
            Type ServerCommandManagerR = typeof(IServerApi).Assembly.GetType("Hkmp.Game.Command.Server.ServerCommandManager");
            object[] parametersArray = new object[] { commandSender, message };
            return (bool)ServerCommandManagerR.GetMethod("ProcessCommand", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ServerApi.CommandManager, parametersArray);

        }

        internal void Broadcast(string v)
        {
            ServerApi.ServerManager.BroadcastMessage(v);
        }

        private void Li_OnChatMessage(object sender, ChatEventArgs e)
        {
            var player = ServerApi.ServerManager.GetPlayer(e.PlayerId);
            if(player == null)
            {
                return;
            }

            Console.WriteLine($"{player.Username} from {player.CurrentScene} says {e.Message}");
            httpClient.PostAsync(Settings.Instance.DiscordBotWebhook, new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "Username",player.Username },
                { "CurrentScene",Settings.Instance.Locations ? player.CurrentScene : "████████████████████"},
                { "Message", e.Message }
            })));
        }
    }
}
