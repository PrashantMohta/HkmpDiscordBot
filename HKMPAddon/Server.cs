using Hkmp.Api.Eventing.ServerEvents;
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

        public IServerApi serverApi { get; private set; }
        public Server()
        {
            Instance = this;
        }

        public IServerPlayer GetPlayer(string username)
        {
            foreach (var player in ServerApi.ServerManager.Players)
            {
                if (player.Username == username)
                {
                    return player;
                }
            }
            return null;
        }

        public override void Initialize(IServerApi serverApi)
        {
            Settings.Initialise();
            Instance = this;
            this.serverApi = serverApi;
            webhookClient = new WebhookClient(Settings.Instance.DiscordBotWebhook);
            ServerApi.ServerManager.PlayerChatEvent += ServerManager_PlayerChatEvent;
            ServerApi.CommandManager.RegisterCommand(new RedactLocations());
            ServerApi.CommandManager.RegisterCommand(new GetPlayer());
            ServerApi.CommandManager.RegisterCommand(new ReportPlayer());
            //start webhook server
            var url = $"http://{Settings.Instance.HostName}:{Settings.Instance.Port}/";
            var webHookServer = new WebhookServer(url,webhookCallback);
            webHookServer.ExceptionHandler = this.webhookExceptionHandler;
            //on another thread
            Thread thread1 = new Thread(webHookServer.Start);
            thread1.Start();
            webhookClient.Send(new WebhookData
            {
                Message = Constants.EVENT_SERVER_STARTED,
                ServerId = Settings.Instance.ServerId,
                IsSystem = true
            });

            ServerApi.ServerManager.PlayerConnectEvent += ServerManager_PlayerConnectEvent;
            ServerApi.ServerManager.PlayerDisconnectEvent += ServerManager_PlayerDisconnectEvent;
        }

        private void ServerManager_PlayerChatEvent(IPlayerChatEvent chatEvent)
        {
            IServerPlayer player = chatEvent.Player;
            string message = chatEvent.Message;
            if (player == null)
            {
                return;
            }

            Console.WriteLine($"{player.Username} in {player.CurrentScene} says {message}");
            webhookClient.Send(new WebhookData
            {
                UserName = player.Username,
                CurrentScene = Settings.Instance.Locations ? player.CurrentScene : "",
                Message = message,
                ServerId = Settings.Instance.ServerId
            });
        }

        private void ServerManager_PlayerDisconnectEvent(IServerPlayer player)
        {
            if (player == null)
            {
                return;
            }
            webhookClient.Send(new WebhookData
            {
                UserName = player.Username,
                CurrentScene = Settings.Instance.Locations ? player.CurrentScene : "",
                Message = Constants.EVENT_USER_DISCONNECT,
                ServerId = Settings.Instance.ServerId,
                IsSystem = true
            });
        }

        private void webhookExceptionHandler(HttpListenerContext ctx, Exception ex)
        {
            webhookClient.Send(new WebhookData
            {
                UserName = "❗️ERROR❗️",
                CurrentScene = ex.Message,
                Message = ex.StackTrace,
                IsSystem = true,
                ServerId = Settings.Instance.ServerId
            });
        }

        private void webhookCallback(HttpListenerContext ctx, WebhookData data)
        {
            if (data != null && data.UserName != null)
            {

                if(data.Message.StartsWith("/ban"))
                {
                    var report = data.Message.Replace("/ban","/report");
                    Server.Instance.TryRunCommand(report, data.IsSystem,true);
                }
                if (data.Message.StartsWith("/kick"))
                {
                    var report = data.Message.Replace("/kick", "/report");
                    Server.Instance.TryRunCommand(report, data.IsSystem, true);
                }

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
                UserName = player.Username,
                CurrentScene = player.CurrentScene,
                Message = Constants.EVENT_USER_CONNECT,
                ServerId = Settings.Instance.ServerId,
                IsSystem = true
            });
        }


        internal bool TryRunCommand(string message,bool isSystem, bool isSilent = false)
        {
            commandSender.IsSystem = isSystem;
            commandSender.IsSilent = isSilent;
            //Reflection into HKMP to run the command
            Type ServerCommandManagerR = typeof(IServerApi).Assembly.GetType("Hkmp.Game.Command.Server.ServerCommandManager");
            object[] parametersArray = new object[] { commandSender, message };
            return (bool)ServerCommandManagerR.GetMethod("ProcessCommand", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ServerApi.CommandManager, parametersArray);

        }

        internal void Broadcast(string v)
        {
            ServerApi.ServerManager.BroadcastMessage(v);
        }

    }
}
