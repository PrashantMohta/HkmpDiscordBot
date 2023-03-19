using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HKMPBot
{
    internal class Program
    {
        private DiscordSocketClient _client;
        private IMessageChannel channel;

        public static Task Main(string[] args) => new Program().MainAsync();
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public async void callback(WebhookData w)
        {
            if(channel == null) { 
                channel = await _client.GetChannelAsync(Settings.Instance.ChannelId) as IMessageChannel;
            }
            await channel!.SendMessageAsync($"{w.UserName} says `{w.Message.Replace("`","\\`")}`");
        }

        private static readonly HttpClient httpClient = new HttpClient();
        public async Task MainAsync()
        {
            Settings.Load();
            HttpServer.url = $"http://*:{Settings.Instance.Port}/";
            Thread thread1 = new Thread(() => {
                HttpServer.Start(callback);
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
            if(arg.Channel.Id == Settings.Instance.ChannelId && arg.Author.Id != _client.CurrentUser.Id)
            {
                Console.WriteLine($"{arg.Author.Username}:{arg.CleanContent}");
                httpClient.PostAsync(Settings.Instance.HkmpAddonWebhook, new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    { "Username",arg.Author.Username },
                    { "CurrentScene", arg.Channel.Name },
                    { "Message", arg.CleanContent }
                })));
            }
            return Task.Delay(0);
        }
    }
}
