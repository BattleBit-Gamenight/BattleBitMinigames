using BattleBitAPI.Common;
using BattleBitAPI.Server;

namespace BattleBitApi.Api;

public class Event
{
    public BattleBitServer Server { get; set; }
    
    public virtual async Task OnConnected()
    {
    }

    public virtual async Task OnTick()
    {
    }

    public virtual async Task OnDisconnected()
    {
    }

    public virtual async Task OnPlayerConnected(BattleBitApiPlayer player)
    {
    }

    public virtual async Task OnPlayerDisconnected(BattleBitApiPlayer player)
    {
    }

    public virtual async Task<bool> OnPlayerTypedMessage(BattleBitApiPlayer player, ChatChannel channel, string msg)
    {
        return true;
    }

    public virtual async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
    }

    public virtual async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
    }

    public virtual async Task<bool> OnPlayerRequestingToChangeRole(BattleBitApiPlayer player, GameRole requestedRole)
    {
        return true;
    }

    public virtual async Task<bool> OnPlayerRequestingToChangeTeam(BattleBitApiPlayer player, Team requestedTeam)
    {
        return true;
    }

    public virtual async Task OnPlayerChangedRole(BattleBitApiPlayer player, GameRole role)
    {
    }

    public virtual async Task OnPlayerJoinedSquad(BattleBitApiPlayer player, Squad<BattleBitApiPlayer> squad)
    {
    }

    public virtual async Task OnSquadLeaderChanged(Squad<BattleBitApiPlayer> squad, BattleBitApiPlayer newLeader)
    {
    }

    public virtual async Task OnPlayerLeftSquad(BattleBitApiPlayer player, Squad<BattleBitApiPlayer> squad)
    {
    }

    public virtual async Task OnPlayerChangeTeam(BattleBitApiPlayer player, Team team)
    {
    }

    public virtual async Task OnSquadPointsChanged(Squad<BattleBitApiPlayer> squad, int newPoints)
    {
    }

    public virtual async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitApiPlayer player, OnPlayerSpawnArguments request)
    {
        return request;
    }

    public virtual async Task OnPlayerSpawned(BattleBitApiPlayer player)
    {
    }

    public virtual async Task OnPlayerDied(BattleBitApiPlayer player)
    {
    }

    public virtual async Task OnPlayerGivenUp(BattleBitApiPlayer player)
    {
    }

    public virtual async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitApiPlayer> args)
    {
    }

    public virtual async Task OnAPlayerRevivedAnotherPlayer(BattleBitApiPlayer from, BattleBitApiPlayer to)
    {
    }

    public virtual async Task OnPlayerReported(BattleBitApiPlayer from, BattleBitApiPlayer to, ReportReason reason,
        string additional)
    {
    }

    public virtual async Task OnGameStateChanged(GameState oldState, GameState newState)
    {
    }

    public virtual async Task OnRoundStarted()
    {
    }

    public virtual async Task OnRoundEnded()
    {
    }

    public virtual async Task OnSessionChanged(long oldSessionID, long newSessionID)
    {
    }
}