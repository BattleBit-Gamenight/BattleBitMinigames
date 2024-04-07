using BattleBitApi.ConsoleCommands;

namespace BattleBitApi.Handlers;

public class ConsoleCommandHandler
{
    public void Listen()
    {
        while (true)
        {
            string? command = Console.ReadLine();
            if (command is null)
            {
                Program.Logger.Warn("No std in stream available.");
                Program.Logger.Error("Exiting...");
                return;
            }
            
            if (Program.Server is null && command != "help")
            {
                Program.Logger.Warn("Server is not initialized.");
                continue;
            }
            
            string[] commandParts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (commandParts.Length == 0) continue;
            
            string commandName = commandParts[0];
            
            ConsoleCommand? consoleCommand = ConsoleCommandList.Commands.FirstOrDefault(c => c.Name == commandName);
            if (consoleCommand == null)
            {
                Program.Logger.Error($"Command {commandName} not found.");
                continue;
            }
            
            consoleCommand.Action?.Invoke(commandParts.Skip(1).ToArray());
        }
    }
}