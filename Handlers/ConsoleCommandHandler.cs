using BattleBitMinigames.ConsoleCommands;

namespace BattleBitMinigames.Handlers;

public static class ConsoleCommandHandler
{
    public static void Listen()
    {
        while (true)
        {
            var command = Console.ReadLine();
            if (command is null)
            {
                Program.Logger.Warn("No stdin stream available.");
                Program.Logger.Error("Exiting...");
                return;
            }
            
            if (Program.Server is null && command != "help")
            {
                Program.Logger.Warn("Server is not initialized.");
                continue;
            }
            
            var commandParts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length == 0) continue;
            
            var commandName = commandParts[0];
            
            var consoleCommand = ConsoleCommandList.Commands.FirstOrDefault(c => c.Name == commandName);
            if (consoleCommand == null)
            {
                Program.Logger.Error($"Command {commandName} not found.");
                continue;
            }
            
            consoleCommand.Action?.Invoke(commandParts.Skip(1).ToArray());
        }
    }
}