using System.Text;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;

namespace BattleBitMinigames.ChatCommands;

public class GamemodeRotation : ChatCommand
{
    public GamemodeRotation() : base(
        name: "gm",
        description: "Add, remove or list current gamemodes.",
        usage: "gm <remove (r), add (a), list (ls), reload (rl)> [gamemode name]",
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
                case "a":
                case "add":
                    if (args.Length < 1)
                    {
                        player.Message($"Invalid arguments. Usage: {Usage}");
                        return;
                    }

                    var gamemodeToAdd = string.Join(" ", args).ToUpperInvariant();
                    if (GamemodeHelper.IsValidGamemode(gamemodeToAdd))
                    {
                        if (!Server.GamemodeRotation.AddToRotation(gamemodeToAdd))
                        {
                            player.Message("Gamemode already exists in the gamemode rotation.");
                            return;
                        }

                        Program.ServerConfiguration.GamemodeRotation.Add(gamemodeToAdd);
                        Program.SaveConfiguration(Program.ServerConfiguration);
                        player.Message(
                            $"Added {RichTextHelper.Bold(true)}{RichTextHelper.FromColorName("Gold")}{gamemodeToAdd}{RichTextHelper.Color()}{RichTextHelper.Bold(false)} to the rotation.");
                        Program.Logger.Info($"Added {gamemodeToAdd} to the rotation.");
                    }
                    else
                    {
                        player.Message("Invalid gamemode.");
                    }

                    break;
                case "r":
                case "remove":
                    if (args.Length < 1)
                    {
                        player.Message($"Invalid arguments. Usage: {Usage}");
                        return;
                    }

                    var gamemodeToRemove = string.Join(" ", args).ToUpperInvariant();
                    if (GamemodeHelper.IsValidGamemode(gamemodeToRemove))
                    {
                        if (!Server.GamemodeRotation.RemoveFromRotation(gamemodeToRemove))
                        {
                            player.Message("Gamemode does not exist in the gamemode rotation.");
                            return;
                        }

                        Program.ServerConfiguration.GamemodeRotation.Remove(gamemodeToRemove);
                        Program.SaveConfiguration(Program.ServerConfiguration);
                        player.Message(
                            $"Removed {RichTextHelper.Bold(true)}{RichTextHelper.FromColorName("Gold")}{gamemodeToRemove}{RichTextHelper.Color()}{RichTextHelper.Bold(false)} from the rotation.");
                        Program.Logger.Info($"Removed {gamemodeToRemove} from the rotation.");
                    }
                    else
                    {
                        player.Message("Invalid gamemode.");
                    }

                    break;
                case "ls":
                case "list":
                    var gamemodes = new StringBuilder();
                    foreach (var gamemode in Server.GamemodeRotation.GetGamemodeRotation())
                    {
                        gamemodes.Append(
                            $"{RichTextHelper.Bold(true)}{RichTextHelper.FromColorName("Gold")}{gamemode}{RichTextHelper.Color()}{RichTextHelper.Bold(false)}, ");
                    }

                    player.Message(
                        $"Current gamemode rotation:{RichTextHelper.NewLine()}{gamemodes.ToString().TrimEnd(',', ' ')}");
                    break;
                case "rl":
                case "reload":
                    Program.ReloadConfiguration();
                    player.Message("Reloaded map rotation.");
                    Program.Logger.Info("Reloaded map rotation.");
                    break;
                default:
                    player.Message($"Invalid action. Usage: {Usage}");
                    break;
            }
        };
    }
}