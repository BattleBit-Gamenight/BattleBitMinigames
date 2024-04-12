using BattleBitAPI;
using BattleBitApi.Enums;

namespace BattleBitApi.Api;
public class BattleBitApiPlayer : Player<BattleBitApiPlayer>
{
    public bool Debug = false;
    
    public PlayerRoles[] PlayerRoles = {
        Enums.PlayerRoles.Default
    };

    public bool AddPlayerRole(PlayerRoles playerRole)
    {
        if (PlayerRoles.Contains(playerRole)) return false;
        
        var updatedRoles = new PlayerRoles[PlayerRoles.Length + 1];

        for (var i = 0; i < PlayerRoles.Length; i++)
        {
            updatedRoles[i] = PlayerRoles[i];
        }

        updatedRoles[PlayerRoles.Length] = playerRole;
        PlayerRoles = updatedRoles;

        return true;
    }

    public PlayerRoles GetHighestRole()
    {
        return PlayerRoles.Length == 0 ? Enums.PlayerRoles.Default : PlayerRoles.Max();
    }
}