using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Webhooks;

namespace HKMPDiscordBot
{
    internal partial class Program
    {
        public static Program Instance;
        private DiscordSocketClient _client;
        private IMessageChannel channel,adminChannel;

        public static Task Main(string[] args) => new Program().MainAsync();
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public Program()
        {
            Instance = this;
        }


        private static WebhookClient webhookClient;
        public async Task MainAsync()
        {
            Settings.Initialise();
            var url = $"http://*:{Settings.Instance.Port}/";
            webhookClient = new WebhookClient(Settings.Instance.HkmpAddonWebhook);

            var webHookServer = new WebhookServer(url, webhookCallback);
            webHookServer.ExceptionHandler = this.webhookExceptionHandler;
            Thread thread1 = new Thread(() => {
                try {
                    webHookServer.Start();
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    SendErrorMessageToAdmin(e.ToString());
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
            SendErrorMessageToAdmin(ex.ToString());
        }

        private void webhookCallback(HttpListenerContext ctx, Webhooks.WebhookData w)
        {
            if (w != null && w.UserName != null)
            {
                //var fileStream = new FileStream(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/godseeker.png", FileMode.Open);
                //var image = new Image(fileStream);
                //await _client.CurrentUser.ModifyAsync(u => u.Avatar = image);
                try
                {
                    if (w.IsSystem != "true")
                    {
                        SendResponseMessageToUsers(w);
                    }
                    else
                    {
                        SendResponseMessageToAdmin(w.Message);
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
            SlashCommands.MuteUnmute(_client);
            return Task.CompletedTask;
        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            if(arg.Channel.Id == Settings.Instance.AdminChannelId && arg.Author.Id != _client.CurrentUser.Id)
            {
                var safeContent = arg.CleanContent;
                if (arg.CleanContent.StartsWith("./"))
                {
                    safeContent = arg.CleanContent.Substring(1);
                }
                Console.WriteLine($"{arg.Author.Username}:{arg.CleanContent}");
                SendToHKMPAddon(new WebhookData
                {
                    UserName = arg.Author.Username,
                    CurrentScene = arg.Channel.Name,
                    Message = safeContent,
                    IsSystem = "true"
                });
            }
            if(arg.Channel.Id == Settings.Instance.ChannelId && arg.Author.Id != _client.CurrentUser.Id)
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
                SendToHKMPAddon(new WebhookData { UserName = arg.Author.Username  , CurrentScene = arg.Channel.Name , Message = safeContent});
            }
            return Task.Delay(0);
        }
    }
}
