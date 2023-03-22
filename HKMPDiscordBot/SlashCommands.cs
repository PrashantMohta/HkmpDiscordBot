using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace HKMPDiscordBot
{
    internal class SlashCommands
    {
        public static async void MuteUnmute(DiscordSocketClient client)
        {
            // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
            var guild = client.GetGuild(Settings.Instance.GuildId);
            if (guild == null)
            {
                return;
            }
            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
            var guildCommand = new SlashCommandBuilder();

            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            guildCommand.WithName("mute-unmute");

            // Descriptions can have a max length of 100.
            guildCommand.WithDescription("Mute the bot on this server!");
            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                await guild.CreateApplicationCommandAsync(guildCommand.Build());

            }
            catch (HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }

            client.SlashCommandExecuted += SlashCommandHandler;
        }

        private static Task SlashCommandHandler(SocketSlashCommand command)
        {
            Settings.Instance.IsMuted = !Settings.Instance.IsMuted;
            Program.Instance.SendResponseMessageToAdmin(Settings.Instance.IsMuted ? "Muted" : "Unmuted");
            return command.RespondAsync($"You executed {command.Data.Name}");
        }
    }
}
