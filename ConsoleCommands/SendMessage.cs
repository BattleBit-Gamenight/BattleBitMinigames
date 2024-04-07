using BattleBitApi.Helpers;

namespace BattleBitApi.ConsoleCommands;
    
public class SendMessage : ConsoleCommand
{
    public SendMessage() : base(
        name: "say",
        description: "Sends a message to all players.",
        usage: "say <message>"
    )
    {
        Action = args =>
        {
            if (args.Length < 1)
            {
                Logger.Error("You must provide a message.");
                return;
            }
            
            string message = string.Join(" ", args);
            MessageHelper.ConsoleToChat(message, Server);
        };
    }
}