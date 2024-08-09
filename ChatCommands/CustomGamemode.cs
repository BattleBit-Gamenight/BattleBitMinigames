using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;

namespace BattleBitMinigames.ChatCommands;

public class CustomGamemode : ChatCommand
{
    public CustomGamemode() : base(
        name: "cgm",
        description: "Set or reset custom gamemodes",
        usage: "cgm <reset (rs), set (s)> [custom gamemode name]",
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

            string action = args[0];
            args = args.Skip(1).ToArray();

            switch (action)
            {
                case "rs":
                case "reset":
                    Server.ResetEvents();
                    player.Message($"Disabled custom gamemodes");

                    break;
                case "s":
                case "set":
                    if (args.Length < 1)
                    {
                        player.Message($"Invalid arguments. Usage: {Usage}");
                        return;
                    }

                    var customGamemodeToAdd = string.Join(" ", args).ToUpperInvariant();
                    if (CustomGamemodeHelper.IsValidCustomGamemode(customGamemodeToAdd))
                    {
                        CustomGamemodeHelper.SetCustomGameMode(customGamemodeToAdd, Server);

                        player.Message(
                            $"Starting gamemode {RichTextHelper.Bold(true)}{RichTextHelper.FromColorName("Gold")}{customGamemodeToAdd}{RichTextHelper.Color()}{RichTextHelper.Bold(false)}.");
                    }
                    else
                    {
                        player.Message("Invalid gamemode.");
                    }

                    break;
                default:
                    player.Message($"Invalid action. Usage: {Usage}");
                    break;
            }
        };
    }
}