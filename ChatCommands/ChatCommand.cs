using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Interfaces;
using log4net;

namespace BattleBitMinigames.ChatCommands;

public class ChatCommand : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public string Usage { get; }
    public PlayerRoles MinimumRequiredRole { get; set; }
    public Action<string[], BattleBitPlayer>? Action { get; set; }
    protected BattleBitServer Server { get; set; }
    protected  ILog Logger => Program.Logger;
        
    protected ChatCommand(string name, string description, string usage = "", PlayerRoles minimumRequiredRole = PlayerRoles.Default ,Action<string[], BattleBitPlayer>? action = null)
    {
        Name = name;
        Description = description;
        Usage = usage;
        Action = action;
        Server = Program.Server;
        MinimumRequiredRole = minimumRequiredRole;
    }
        
    public override string ToString()
    {
        return $"{Name} - {Description}";
    }
    
    public bool CanExecute(BattleBitPlayer player)
    {
        var canExec = player.PlayerRoles.Any(role => role >= MinimumRequiredRole);
        
        if (canExec)
            player.SetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.LastMessageSentTime, DateTime.UtcNow.ToUniversalTime().ToString());
        
        return canExec;
    }
}