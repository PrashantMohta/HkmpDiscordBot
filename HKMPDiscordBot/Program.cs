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
            BanListSettings.Initialise();
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
            Console.WriteLine($"webhookCallback {ctx.Request.Url.AbsolutePath}");
            if(ctx.Request.HttpMethod == "GET" && ctx.Request.Url.AbsolutePath == "/banlist")
            {
               var response = BanListSettings.BanListLoader.GetString(BanListSettings.Instance.BanList);
                ctx.Respond(response,200);
            } 
            else if (w != null && w.UserName != null)
            {
                if(w.CurrentScene == Constants.BOTSEEKER_LOCATION || w.CurrentScene == "")
                {
                    w.CurrentScene = FlavorStrings.GetBotLocationMessage();
                } else
                {
                    w.CurrentScene = BetterRoomNames.GetRoomName(w.CurrentScene);
                }
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
                        SendResponseMessageToUsers(bot, w);
                    }
                    else
                    {
                        if (w.Message == Constants.EVENT_USER_CONNECT)
                        {
                            if (!w.IsSystem)
                            {
                                SendResponseMessageToUsers(bot, w);
                            }
                            else if (w.Message != Constants.EVENT_USER_CONNECT)
                            {
                                SendResponseMessageToAdmin(bot, w.Message);
                            }
                            SendResponseMessageToUsers(bot, new WebhookData
                            {
                                UserName = bot.BotName,
                                CurrentScene = w.CurrentScene != "" ? BetterRoomNames.GetRoomName(w.CurrentScene) : "████████████████████",
                                Message = FlavorStrings.GetConnectMessage(w),
                                ServerId = bot.ServerId,
                                IsSystem = false
                            });
                        } else if (w.Message == Constants.EVENT_SERVER_STARTED) {

                            SendResponseMessageToUsers(bot, new WebhookData
                            {
                                UserName = bot.BotName,
                                CurrentScene = FlavorStrings.GetBotLocationMessage(),
                                Message = "I'm online! 🤖 \n Check the connected players using the `?list` command.",
                                ServerId = bot.ServerId
                            });
                        } else if (w.Message == Constants.EVENT_USER_DISCONNECT) {
                            SendResponseMessageToUsers(bot, new WebhookData
                            {
                                UserName = bot.BotName,
                                CurrentScene = w.CurrentScene != "" ? BetterRoomNames.GetRoomName(w.CurrentScene) : "████████████████████",
                                Message = FlavorStrings.GetDisconnectMessage(w),
                                ServerId = bot.ServerId
                            });
                        } else {
                            SendResponseMessageToAdmin(bot, w.Message);
                        }
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
            var content = arg.Content;
            if (botInstance == null)
            {
                return Task.Delay(0);
            } else { 
                if (arg.Channel.Id == botInstance.AdminChannelId && arg.Author.Id != _client.CurrentUser.Id)
                {
                    var safeContent = content;
                    if (content.StartsWith("?list"))
                    {
                        safeContent = "/list";
                    }
                    if (content.StartsWith("./"))
                    {
                        safeContent = content.Substring(1);
                    }
                    if (content.StartsWith("?help"))
                    {
                        var message = $"Commands that botseeker supports : \n\n" +
                            $"1.General :\n" +
                            $"```?help```\n Show this message.\n\n" +
                            $"```?list```\n show a list of connected users.\n" +
                            $"```?player (authkey|ip) <username>```\n get the player's (authkey|ip).\n" +
                            $"2.Manage Banned Phrases list :\n" +
                            $"```?get phrases```\n get a list of all banned phrases.\n" +
                            $"```?add phrase <phrase>```\n add a new phrase to the banned phrases.\n" +
                            $"```?remove phrase <phrase>```\n remove a phrase from the banned phrases.\n" +
                            $"3. Hkmp commands can be used by adding a `.` at the begining.\n" +
                            $" for example ```./unban <authKey>``` will unban a user.\n";
                        SendResponseMessageToAdmin(botInstance, $"{message}");
                    }
                    if (content.StartsWith("?player"))
                    {
                        safeContent = content.Replace("?player","/player");
                    }
                    if (content.StartsWith("?get phrases"))
                    {
                        var message = $"Banned Phrases list : \n ```{BanListSettings.Instance.BanList.ListPhrases()}``` \n";
                        SendResponseMessageToAdmin(botInstance, $"{message}");
                    }
                    if (content.StartsWith("?add phrase"))
                    {
                        var phrase = content.Replace("?add phrase", "").Trim();
                        try
                        { 
                            if(BanListSettings.Instance.BanList.Contains(phrase))
                            {
                                throw new Exception($"Banlist Already contains the phrase `{phrase}`");
                            }
                            BanListSettings.Instance.BanList.Add(phrase,false);
                            BanListSettings.Instance.Save();
                            var message = $"Added `{phrase}` to Banned Phrases list.";
                            SendResponseMessageToAdmin(botInstance, $"{message}");
                        }
                        catch (Exception e)
                        {
                            SendErrorMessageToAdmin(botInstance, e.Message);
                        }
                    }
                    if (content.StartsWith("?hardban phrase"))
                    {
                        var phrase = content.Replace("?hardban phrase", "").Trim();
                        try
                        {
                            var foundPhrase = BanListSettings.Instance.BanList.Find(phrase);
                            if (foundPhrase != null)
                            {
                                foundPhrase.IsHardFilter = true;
                                BanListSettings.Instance.Save();
                                var message = $"Added `{phrase}` to Hard Banned Phrases list.";
                                SendResponseMessageToAdmin(botInstance, $"{message}");
                            } else
                            {
                                throw new Exception($"Banlist does not contain the phrase `{phrase}`");
                            }
                           
                        }
                        catch (Exception e)
                        {
                            SendErrorMessageToAdmin(botInstance, e.Message);
                        }
                    }
                    if (content.StartsWith("?softban phrase"))
                    {
                        var phrase = content.Replace("?softban phrase", "").Trim();
                        try
                        {
                            var foundPhrase = BanListSettings.Instance.BanList.Find(phrase);
                            if (foundPhrase != null)
                            {
                                foundPhrase.IsHardFilter = false;
                                BanListSettings.Instance.Save();
                                var message = $"Added `{phrase}` to Soft Banned Phrases list.";
                                SendResponseMessageToAdmin(botInstance, $"{message}");
                            }
                            else
                            {
                                throw new Exception($"Banlist does not contain the phrase `{phrase}`");
                            }

                        }
                        catch (Exception e)
                        {
                            SendErrorMessageToAdmin(botInstance, e.Message);
                        }
                    }
                    if (content.StartsWith("?remove phrase"))
                    {
                        var phrase = content.Replace("?remove phrase", "").Trim();
                        try
                        {
                            BanListSettings.Instance.BanList.Remove(phrase);
                            BanListSettings.Instance.Save();
                            var message = $"Removed `{phrase}` from Banned Phrases list.";
                            SendResponseMessageToAdmin(botInstance, $"{message}");
                        }
                        catch (Exception e)
                        {
                            SendErrorMessageToAdmin(botInstance, e.Message);
                        }
                    }
                    Console.WriteLine($"{arg.Author.Username}:{content}");
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
                    var safeContent = content;
                    if (content.StartsWith("/"))
                    {
                        safeContent = "//" +content.Substring(1);
                    } else if (content.StartsWith("\""))
                    {
                        safeContent = "''" + content.Substring(1);
                    } else if (content.StartsWith("?list"))
                    {
                        safeContent = "/list";
                    } else if (content.StartsWith("./list"))
                    {
                        safeContent = content.Substring(1);
                    }
                    Console.WriteLine($"{arg.Author.Username}:{content}");
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
