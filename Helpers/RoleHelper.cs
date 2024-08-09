using BattleBitMinigames.Api;

namespace BattleBitMinigames.Helpers;

public class RoleHelper
{
    public static List<ulong> Admins = new()
    {
        76561198395073327,
        76561198035784951,
        76561198855127416
    };

    public static List<ulong> Moderators = new()
    {
        76561198173566107,
        76561198051546518,
        76561199056414354,
        76561198833659544
    };
    
    public static List<ulong> Vips = new() { };
    
    public static List<ulong> Specials = new() { };
    
    public static void SetPlayerRoles(BattleBitPlayer player)
    {
        if (Admins.Contains(player.SteamID))
        {
            Program.Logger.Info(player.AddPlayerRole(Enums.PlayerRoles.Admin)
                ? $"Successfully added roles for {player.Name} ({player.SteamID})"
                : $"User {player.Name} ({player.SteamID}) already has the Role.");
        }
        else if (Moderators.Contains(player.SteamID))
        {
            Program.Logger.Info(player.AddPlayerRole(Enums.PlayerRoles.Moderator)
                ? $"Successfully added roles for {player.Name} ({player.SteamID})"
                : $"User {player.Name} ({player.SteamID}) already has the Role.");
        }
        else if (Vips.Contains(player.SteamID))
        {
            Program.Logger.Info(player.AddPlayerRole(Enums.PlayerRoles.Vip)
                ? $"Successfully added roles for {player.Name} ({player.SteamID})"
                : $"User {player.Name} ({player.SteamID}) already has the Role.");
        }
        else if (Specials.Contains(player.SteamID))
        {
            Program.Logger.Info(player.AddPlayerRole(Enums.PlayerRoles.Special)
                ? $"Successfully added roles for {player.Name} ({player.SteamID})"
                : $"User {player.Name} ({player.SteamID}) already has the Role.");
        }
    }
}