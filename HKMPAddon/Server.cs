using Hkmp.Api.Server;
using Hkmp.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using Webhooks;

namespace DiscordIntegrationAddon
{
    public class Server : ServerAddon
    {

        public static Server Instance { get; private set; }
        public override bool NeedsNetwork => false;

        protected override string Name => Settings.AddonName;

        protected override string Version => "v0.2";

        internal static WebhookClient webhookClient;

        internal static CommandSender commandSender = new CommandSender();

        public Server()
        {
            Instance = this;
        }

        public override void Initialize(IServerApi serverApi)
        {
            Settings.Initialise();
            Instance = this;
            webhookClient = new WebhookClient(Settings.Instance.DiscordBotWebhook);

            //Reflection into HKMP to add our own ILogger so we can inspect the chat via it
            //Cursed but it works.
            Type LoggerR = typeof(IServerApi).Assembly.GetType("Hkmp.Logging.Logger");
            var loggersList = (List<ILogger>)LoggerR.GetField("Loggers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var li = new LogInterceptor();
            loggersList.Add(li);
            li.OnChatMessage += Li_OnChatMessage;
            ServerApi.CommandManager.RegisterCommand(new RedactLocations());

            //start webhook server
            var url = $"http://{Settings.Instance.HostName}:{Settings.Instance.Port}/";
            var webHookServer = new WebhookServer(url,webhookCallback);
            webHookServer.ExceptionHandler = this.webhookExceptionHandler;
            //on another thread
            Thread thread1 = new Thread(webHookServer.Start);
            thread1.Start();

            webhookClient.Send(new WebhookData
            {
                UserName = Settings.Instance.Name,
                CurrentScene = FlavorStrings.GetBotLocationMessage(),
                Message = "I'm online! \n Check the connected players using the `?list` command."
            });

            ServerApi.ServerManager.PlayerConnectEvent += ServerManager_PlayerConnectEvent;
        }

        private void webhookExceptionHandler(HttpListenerContext ctx, Exception ex)
        {
            webhookClient.Send(new WebhookData
            {
                UserName = "ERROR",
                CurrentScene = ex.Message,
                Message = ex.StackTrace,
                IsSystem = true
            });
        }

        private void webhookCallback(HttpListenerContext ctx, WebhookData data)
        {
            if (data != null && data.UserName != null)
            {
                Server.Instance.TryRunCommand(data.Message, data.IsSystem);
                if (!data.IsSystem)
                {
                    var message = $"{data.UserName} in {data.CurrentScene}:{data.Message}";
                    if (message.Length > 250)
                    {
                        message = message.Substring(0, 250);
                        Server.Instance.Broadcast(message + "...");
                    }
                    else
                    {
                        Server.Instance.Broadcast(message);
                    }
                }
                ctx.Respond("ok", 200);
            }
            else
            {
                ctx.Respond("ERROR", 500);
            }
        }

        private void ServerManager_PlayerConnectEvent(IServerPlayer player)
        {
            if(player == null)
            {
                return;
            }
            webhookClient.Send(new WebhookData
            {
                UserName = Settings.Instance.Name,
                CurrentScene = Settings.Instance.Locations ? BetterRoomNames.GetRoomName(player.CurrentScene) : "████████████████████",
                Message = FlavorStrings.GetConnectMessage(player)
            });
        }


        internal bool TryRunCommand(string message,bool isSystem)
        {
            commandSender.IsSystem = isSystem;
            //Reflection into HKMP to run the command
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
            if (player == null)
            {
                return;
            }

            Console.WriteLine($"{player.Username} in {player.CurrentScene} says {e.Message}");
            webhookClient.Send(new WebhookData
            {
                UserName = player.Username,
                CurrentScene = Settings.Instance.Locations ? BetterRoomNames.GetRoomName(player.CurrentScene) : "████████████████████",
                Message = e.Message
            });
        }
    }
}
