using Discord.WebSocket;
using HKMPDiscordBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKMPDiscordBot
{
    public abstract class Command
    {
        public abstract List<string> AliasNames { get; }
        public abstract void Execute(BotInstance botInstance, List<string> args, SocketMessage message);
        public abstract string Help();

    }
    public static class CommandManager
    {
        private static Dictionary<string,Command> RegisteredCommands = new();
        public static List<string> ParseCommand(string commandString)
        {
            List<string> args = new();
            var buffer = "";
            var waitFor = ' ';
            for (int i = 0; i < commandString.Length; i++)
            {
                if (commandString[i] == waitFor)
                {
                    waitFor = ' ';
                    if (buffer.Trim().Length > 0)
                    {
                        args.Add(buffer.Trim());
                        buffer = "";
                    }
                }
                else if (commandString[i] == '"')
                {
                    waitFor = '"';
                }
                else
                {
                    buffer += commandString[i];
                }
            }
            if (waitFor == '"')
            {
                buffer = "\"" + buffer;
            }
            if (buffer.Trim().Length > 0)
            {
                args.Add(buffer.Trim());
                buffer = "";
            }
            return args;
        }
        public static void AddCommand(string name, Command command)
        {
            RegisteredCommands[name] = command;    
        }
        public static bool ProcessCommand(BotInstance botInstance, SocketMessage message)
        {
            try
            {
                var args = ParseCommand(message.Content);
                if (args.Count > 0)
                {
                    if (RegisteredCommands.TryGetValue(args[0], out Command cmd))
                    {
                        cmd.Execute(botInstance, args, message);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Program.Instance.SendErrorMessageToAdmin(botInstance, e.ToString());
            }
            return false;
        }
        public static string ListCommands()
        {
            var outstr = "";
            Dictionary<Command, bool> done = new();
            foreach(var cmd in RegisteredCommands.Values) { 
                if(!done.ContainsKey(cmd))
                {
                    outstr += cmd.Help() + "\n";
                    done[cmd] = true;
                }
            }
            return outstr;
        }

        internal static void Initialise()
        {
            List<Command> cmds = new()
            {
                new ListCommand(),
                new HelpCommand(),
                new GetPlayerCommand(),
                new PhrasesCommand(),
            };
            foreach (Command cmd in cmds)
            {
                foreach (var alias in cmd.AliasNames)
                {
                    CommandManager.AddCommand(alias, cmd);
                }
            }
        }
    }
}
