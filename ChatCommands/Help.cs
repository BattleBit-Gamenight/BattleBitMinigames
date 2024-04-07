using System.Text;
using BattleBitApi.Api;
using BattleBitApi.Enums;

namespace BattleBitApi.ChatCommands;

public class Help : ChatCommand
{
    public Help() : base("help", "Shows a list of commands")
    {
        Action = (args, player) =>
        {
            List<ChatCommand> displayedCommands = new();
            
            // add admin, moderator, and regular commands
            if (player.PlayerRoles.Any(role => role >= PlayerRoles.Admin))
                displayedCommands.AddRange(ChatCommandList.AdminCommands);
            if (player.PlayerRoles.Any(role => role >= PlayerRoles.Moderator))
                displayedCommands.AddRange(ChatCommandList.ModeratorCommands);
            displayedCommands.AddRange(ChatCommandList.Commands);
            
            int page = 1;
            if (args.Length > 0)
            {
                if (!int.TryParse(args[0], out page))
                {
                    player.Message("Invalid page number.");
                    return;
                }
            }
            
            int maxPages = (int) Math.Ceiling(displayedCommands.Count / 10f);
            if (page > maxPages)
            {
                player.Message($"Page {page} does not exist.");
                return;
            }
            
            StringBuilder message = new();
            message.AppendLine($"Showing page {page} of {maxPages}.");
            message.AppendLine("Commands:");
            
            int startIndex = (page - 1) * 10;
            int endIndex = Math.Min(startIndex + 10, displayedCommands.Count);
            
            for (int i = startIndex; i < endIndex; i++)
            {
                ChatCommand command = displayedCommands[i];
                message.AppendLine($"{i + 1}. {command.Name} - {command.Description}");
            }
            
            player.Message(message.ToString());
        };
    }
}