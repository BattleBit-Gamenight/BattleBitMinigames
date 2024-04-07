using BattleBitApi.Api;
using log4net;

namespace BattleBitApi.ConsoleCommands;

public class ConsoleCommand : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public string Usage { get; }
    public Action<string[]>? Action { get; set; }
    protected BattleBitServer Server { get; set; }
    protected  ILog Logger => Program.Logger;
        
    protected ConsoleCommand(string name, string description, string usage = "", Action<string[]>? action = null)
    {
        Name = name;
        Description = description;
        Usage = usage;
        Action = action;
        Server = Program.Server;
    }
        
    public override string ToString()
    {
        return $"{Name} - {Description}";
    }
}