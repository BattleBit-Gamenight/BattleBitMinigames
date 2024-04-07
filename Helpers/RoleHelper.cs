using BattleBitApi.Api;

namespace BattleBitApi.Helpers;

public class RoleHelper
{
    public static List<ulong> Admins = new()
    {
        76561198395073327
    };

    public static List<ulong> Moderators = new() { };
    
    public static List<ulong> Vips = new() { };
    
    public static List<ulong> Specials = new() { };
    
    public static void SetPlayerRoles(BattleBitApiPlayer player)
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