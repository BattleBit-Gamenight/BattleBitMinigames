using BattleBitApi.Helpers;

namespace BattleBitApi.ConsoleCommands;
    
public class SendAnnouncement : ConsoleCommand
{
    public SendAnnouncement() : base(
        name: "an",
        description: "Announces a message to all players.",
        usage: "an [l/s] <message>"
    )
    {
        Action = args =>
        {
            if (args.Length < 1)
            {
                Logger.Error("You must provide a message.");
                return;
            }
            
            string length = args[0];
            if (length != "s" && length != "l")
            {
                Logger.Error("You must provide a valid length.");
                return;
            }
            
            args = args.Skip(1).ToArray();
            string message = string.Join(" ", args);
            
            MessageHelper.ToAnnouncement(message, length, Server);
        };
    }
}