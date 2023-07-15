using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Webhooks;

namespace HKMPDiscordBot
{
    internal partial class Program
    {
        public static Program Instance;
        public static AutoBan autoBan = new AutoBan();
        private DiscordSocketClient _client;

        public static Task Main(string[] args) => new Program().MainAsync();
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public Program()
        {
            Instance = this;
            Settings.Initialise();
            BanList.Initialise();
        }

        public async Task MainAsync()
        {
            var url = $"http://{Settings.Instance.HostName}:{Settings.Instance.Port}/";
            if (Settings.Instance != null)
            {

                Console.WriteLine(Settings.Instance.ToString());
            }
            foreach (var item in Settings.Instance.bots)
            {
                item.webhookClient = new WebhookClient(item.HkmpAddonWebhook);
            }
            var webHookServer = new WebhookServer(url, webhookCallback);
            webHookServer.ExceptionHandler = this.webhookExceptionHandler;
            Thread thread1 = new Thread(() => {
                try {
                    webHookServer.Start();
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    foreach(var item in Settings.Instance.bots)
                    {
                        SendErrorMessageToAdmin(item,e.ToString());
                    }
                }
            });
            thread1.Start();

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);

            _client.Log += Log;


            await _client.LoginAsync(TokenType.Bot, Settings.Instance.Token);
            await _client.StartAsync();

            _client.MessageReceived += _client_MessageReceived;
            _client.Ready += _client_Ready;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private void webhookExceptionHandler(HttpListenerContext ctx, Exception ex)
        {

            foreach (var item in Settings.Instance.bots)
            {
                SendErrorMessageToAdmin(item, ex.ToString());
            }
        }

        private async void webhookCallback(HttpListenerContext ctx, Webhooks.WebhookData w)
        {
            if (w != null && w.UserName != null)
            {
                //todo remember to comment this once the thingy is fixed
                /*var fileStream = new FileStream(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/godseeker.png", FileMode.Open);
                var image = new Image(fileStream);
                await _client.CurrentUser.ModifyAsync(u => {
                    u.Avatar = image;
                    //u.Username = "BotSeeker";
                });
                fileStream.Dispose();*/

                var bot = Settings.Instance.GetBotInstanceByServerId(w.ServerId);
                try
                {
                    if (!w.IsSystem)
                    {
                        var processed = autoBan.Process(bot,w);
                        if(processed != null) { 
                            SendResponseMessageToUsers(bot, processed);
                        }
                    }
                    else
                    {
                        SendResponseMessageToAdmin(bot, w.Message);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                ctx.Respond("ok");
            }
            else
            {
                ctx.Respond("ERROR",500);
            }
            
        }

        private Task _client_Ready()
        {
            foreach (var item in Settings.Instance.bots)
            {
                if(item.slashCommands == null) { 
                    item.slashCommands = new SlashCommands(item);
                }
                item.slashCommands.MuteUnmute(_client);
            }
            return Task.CompletedTask;
        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            var botInstance = Settings.Instance.GetBotInstanceByChannelId(arg.Channel.Id);
            if(botInstance == null)
            {
                return Task.Delay(0);
            } else { 
                if (arg.Channel.Id == botInstance.AdminChannelId && arg.Author.Id != _client.CurrentUser.Id)
                {
                    var safeContent = arg.CleanContent;
                    if (arg.CleanContent.StartsWith("./"))
                    {
                        safeContent = arg.CleanContent.Substring(1);
                    }
                    Console.WriteLine($"{arg.Author.Username}:{arg.CleanContent}");
                    SendToHKMPAddon(botInstance, new WebhookData
                    {
                        UserName = arg.Author.Username,
                        CurrentScene = arg.Channel.Name,
                        Message = safeContent,
                        IsSystem = true,
                        ServerId = botInstance.ServerId
                    });
                }
                if(arg.Channel.Id == botInstance.ChannelId && arg.Author.Id != _client.CurrentUser.Id)
                {
                    var safeContent = arg.CleanContent;
                    if (arg.CleanContent.StartsWith("/"))
                    {
                        safeContent = "//" +arg.CleanContent.Substring(1);
                    } else if (arg.CleanContent.StartsWith("\""))
                    {
                        safeContent = "''" + arg.CleanContent.Substring(1);
                    } else if (arg.CleanContent.StartsWith("?list"))
                    {
                        safeContent = "/list";
                    } else if (arg.CleanContent.StartsWith("./list"))
                    {
                        safeContent = arg.CleanContent.Substring(1);
                    }
                    Console.WriteLine($"{arg.Author.Username}:{arg.CleanContent}");
                    SendToHKMPAddon(botInstance, new WebhookData {
                        UserName = arg.Author.Username  ,
                        CurrentScene = arg.Channel.Name ,
                        Message = safeContent,
                        ServerId = botInstance.ServerId
                        }
                    );
                }
            }
            return Task.Delay(0);
        }
    }
}
