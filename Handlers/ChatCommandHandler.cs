using BattleBitApi.Api;
using BattleBitApi.ChatCommands;

namespace BattleBitApi.Handlers;

public class ChatCommandHandler
{
    public static Task<bool> Run(string message, BattleBitApiPlayer player)
    {
        string chatCommandPrefix = Program.ServerConfiguration.ChatCommandPrefix ?? "!";
        if (!message.StartsWith(chatCommandPrefix)) return Task.FromResult(true);

        string[] args = message[chatCommandPrefix.Length..].Split(' ');
        string commandName = args[0];

        ChatCommand? command = GetCommandFromName(commandName);
        if (command == null)
        {
            player.Message($"Command {commandName} not found.");
            return Task.FromResult(false);
        }

        try
        {
            command.Action?.Invoke(args.Skip(1).ToArray(), player);
            Program.Logger.Info($"Command {commandName} with args \"{string.Join(" ", args.Skip(1).ToArray())}\" executed by \"{player.Name}\""); 
        }
        catch (Exception e)
        {
            player.Message($"An error occurred while executing command \"{commandName}\"");
            Program.Logger.Error($"An error occurred while executing command \"{commandName}\" with args \"{string.Join(" ", args.Skip(1).ToArray())}\" by \"{player.Name}\"", e);
            return Task.FromResult(false);
        }
        
        return Task.FromResult(false);
    }

    private static ChatCommand? GetCommandFromName(string name)
    {
        ChatCommand? command = ChatCommandList.Commands.FirstOrDefault(c => c.Name == name);
        if (command != null) return command;
        
        command = ChatCommandList.AdminCommands.FirstOrDefault(c => c.Name == name);
        if (command != null) return command;
        
        command = ChatCommandList.ModeratorCommands.FirstOrDefault(c => c.Name == name);
        return command ?? null;
    }
}