using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static HKMPDiscordBot.BetterRoomNames;
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

        public void callback(WebhookData w)
        {
            //var fileStream = new FileStream(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/godseeker.png", FileMode.Open);
            //var image = new Image(fileStream);
            //await _client.CurrentUser.ModifyAsync(u => u.Avatar = image);
            try
            {
                if (w.isSystem != "true")
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
        }

        private static readonly HttpClient httpClient = new HttpClient();
        public async Task MainAsync()
        {
            Settings.Load();
            HttpServer.url = $"http://*:{Settings.Instance.Port}/";
            Thread thread1 = new Thread(() => {
                try { 
                    HttpServer.Start(callback);
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

            // Block this task until the program is closed.
            await Task.Delay(-1);
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
                SendToHKMPAddon(new Dictionary<string, string>
                {
                    { "Username",arg.Author.Username },
                    { "CurrentScene", arg.Channel.Name },
                    { "Message", safeContent },
                    { "IsSystem", "true" }
                });
            }
            if(arg.Channel.Id == Settings.Instance.ChannelId && arg.Author.Id != _client.CurrentUser.Id)
            {
                var safeContent = arg.CleanContent;
                if (arg.CleanContent.StartsWith("/"))
                {
                    safeContent = "//" +arg.CleanContent.Substring(1);
                }
                if (arg.CleanContent.StartsWith("\""))
                {
                    safeContent = "''" + arg.CleanContent.Substring(1);
                }
                if (arg.CleanContent.StartsWith("./list"))
                {
                    safeContent = arg.CleanContent.Substring(1);
                }
                Console.WriteLine($"{arg.Author.Username}:{arg.CleanContent}");
                SendToHKMPAddon(new Dictionary<string, string>
                {
                    { "Username",arg.Author.Username },
                    { "CurrentScene", arg.Channel.Name },
                    { "Message", safeContent }
                });
            }
            return Task.Delay(0);
        }
    }
}
