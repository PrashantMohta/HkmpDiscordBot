using DiscordIntegrationAddon;
using Hkmp.Api.Command.Server;
using Hkmp.Api.Server;
using Hkmp.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace HKMPDiscordBot
{
    internal class Server : ServerAddon
    {

        public static Server Instance { get; private set; }
        public override bool NeedsNetwork => false;

        protected override string Name => Settings.Name;

        protected override string Version => "v0.1";

        internal static readonly HttpClient httpClient = new HttpClient();

        internal static CommandSender commandSender = new CommandSender();

        internal static Dictionary<string, IServerCommand> Commands;

        private string[] GetArguments(string message)
        {
            var argList = new List<string>();

            var argRegex = new Regex("([^\"\\s]\\S*|\".*?\")");
            var matches = argRegex.Matches(message);

            foreach (var match in matches)
            {
                argList.Add(match.ToString().Replace("\"", ""));
            }

            return argList.ToArray();
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

            serverApi.ServerManager.PlayerConnectEvent += ServerManager_PlayerConnectEvent;

            
            //Start a HTTP server on another thread
            Thread thread1 = new Thread(HttpServer.Start);
            thread1.Start();
        }

        private void ServerManager_PlayerConnectEvent(IServerPlayer obj)
        {
            if(Commands == null) { 
                //Reflection into HKMP to get the ban command
                Type ServerCommandManagerR = typeof(IServerApi).Assembly.GetType("Hkmp.Game.Command.Server.ServerCommandManager");

                var Commands = (Dictionary<string, IServerCommand>)ServerCommandManagerR.GetField("Commands", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ServerApi.CommandManager);
                foreach (var command in Commands)
                {
                    Console.WriteLine($"{command.Key}");
                }
            }

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
                { "CurrentScene",player.CurrentScene },
                { "Message", e.Message }
            })));
        }
    }
}
