using BattleBitApi.Enums;
using BattleBitApi.Helpers;

namespace BattleBitApi.ChatCommands;

public class SendAnnouncement : ChatCommand
{
    public SendAnnouncement() : base(
        name: "an",
        description: "Announces a message to all players.",
        usage: "an [l/s] <message>",
        minimumRequiredRole: PlayerRoles.Moderator
    )
    {
        Action = (args, player) =>
        {
            if (!CanExecute(player))
            {
                player.Message("You do not have permission to execute this command.");
                return;
            }
            
            if (args.Length < 1)
            {
                player.Message($"Invalid arguments. Usage: {Usage}");
                return;
            }
            
            string length = args[0];
            if (length != "s" && length != "l")
            {
                player.Message("Invalid length. Must be either \"s\" or \"l\".");
                return;
            }
            
            args = args.Skip(1).ToArray();
            string message = string.Join(" ", args);
            
            MessageHelper.ToAnnouncement(message, length, Server);
        };
    }
}