using Hkmp.Api.Command.Server;
using System;

namespace DiscordIntegrationAddon
{
    public class RedactLocations : IServerCommand
    {
        public RedactLocations()
        {
        }
        public bool AuthorizedOnly => false;

        public string Trigger => "/location";

        public string[] Aliases => new string[] { "/locations" };

        public void Execute(ICommandSender commandSender, string[] arguments)
        {
            if(arguments.Length == 1)
            {
                Settings.Instance.Locations = !Settings.Instance.Locations;
            }
            else if(arguments[1].ToLower() == "true")
            {
                Settings.Instance.Locations = false;
            } else 
            {
                Settings.Instance.Locations = true;
            }
            var state = Settings.Instance.Locations ? "visible" : "hidden" ;
            commandSender.SendMessage($"Locations are now {state}");
        }
    }
}
