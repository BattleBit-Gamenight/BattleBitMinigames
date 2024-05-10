using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Events;
using PlayerRoles = BattleBitMinigames.Events.PlayerRoles;
using PlayerStats = BattleBitAPI.Common.PlayerStats;
using ServerSettings = BattleBitMinigames.Events.ServerSettings;

namespace BattleBitMinigames.Api;

public class BattleBitServer : GameServer<BattleBitPlayer>
{
    private readonly List<Event> events = new();
    
    public BattleBitServer()
    {
        //AddEvent(new ServerSettings(), this);
        AddEvent(new PlayerRoles(), this);
        AddEvent(new ChatCommandListener(), this);
        //AddEvent(new RegionManager(), this);
        AddEvent(new ZombiesGamemode(), this);
        //AddEvent(new HideAndSeekGamemode(), this);
        //AddEvent(new GunGameGamemode(), this);
    }
    
    public void ClearAllPlayerProperties()
    {
        foreach (var player in AllPlayers)
        {
            player.ClearAllPlayerProperties();
        }
    }

    private void AddEvent(Event @event, BattleBitServer server)
    {
        @event.Server = server;
        
        events.Add(@event);
    }

    public void RemoveEvent(Event @event)
    {
        if (!events.Contains(@event))
            return;

        events.Remove(@event);
    }

    public override async Task OnConnected()
    {
        foreach (var @event in events)
            await @event.OnConnected();
    }

    public override async Task OnTick()
    {
        foreach (var @event in events)
            await @event.OnTick();
    }

    public override async Task OnDisconnected()
    {
        foreach (var @event in events)
            await @event.OnDisconnected();
    }

    public override async Task OnPlayerConnected(BattleBitPlayer player)
    {
        foreach (var @event in events)
            await @event.OnPlayerConnected(player);
    }

    public override async Task OnPlayerDisconnected(BattleBitPlayer player)
    {
        foreach (var @event in events)
            await @event.OnPlayerDisconnected(player);
    }

    public override async Task<bool> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
    {
        return await RunEventWithBoolReturn(@event => @event.OnPlayerTypedMessage(player, channel, msg));
    }

    public override async Task OnPlayerJoiningToServer(ulong steamID, PlayerJoiningArguments args)
    {
        foreach (var @event in events)
            await @event.OnPlayerJoiningToServer(steamID, args);
    }

    public override async Task OnSavePlayerStats(ulong steamID, PlayerStats stats)
    {
        foreach (var @event in events)
            await @event.OnSavePlayerStats(steamID, stats);
    }

    public override async Task<bool> OnPlayerRequestingToChangeRole(BattleBitPlayer player, GameRole requestedRole)
    {
        return await RunEventWithBoolReturn(@event => @event.OnPlayerRequestingToChangeRole(player, requestedRole));
    }

    public override async Task<bool> OnPlayerRequestingToChangeTeam(BattleBitPlayer player, Team requestedTeam)
    {
        return await RunEventWithBoolReturn(@event => @event.OnPlayerRequestingToChangeTeam(player, requestedTeam));
    }

    public override async Task OnPlayerChangedRole(BattleBitPlayer player, GameRole role)
    {
        foreach (var @event in events)
            await @event.OnPlayerChangedRole(player, role);
    }

    public override async Task OnPlayerJoinedSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        foreach (var @event in events)
            await @event.OnPlayerJoinedSquad(player, squad);
    }

    public override async Task OnSquadLeaderChanged(Squad<BattleBitPlayer> squad, BattleBitPlayer newLeader)
    {
        foreach (var @event in events)
            await @event.OnSquadLeaderChanged(squad, newLeader);
    }

    public override async Task OnPlayerLeftSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        foreach (var @event in events)
            await @event.OnPlayerLeftSquad(player, squad);
    }
    
    public override async Task OnPlayerChangeTeam(BattleBitPlayer player, Team team)
    {
        foreach (var @event in events)
            await @event.OnPlayerChangeTeam(player, team);
    }
    
    public override async Task OnSquadPointsChanged(Squad<BattleBitPlayer> squad, int newPoints)
    {
        foreach (var @event in events)
            await @event.OnSquadPointsChanged(squad, newPoints);
    }
    
    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        var returnRequest = await RunEventWithOnPlayerSpawnArgumentsReturn((@event, oldRequest) => @event.OnPlayerSpawning(player, oldRequest), request);
        return returnRequest;
    }
    
    public override async Task OnPlayerSpawned(BattleBitPlayer player)
    {
        foreach (var @event in events)
            await @event.OnPlayerSpawned(player);
    }
    
    public override async Task OnPlayerDied(BattleBitPlayer player)
    {
        foreach (var @event in events)
            await @event.OnPlayerDied(player);
    }
    
    public override async Task OnPlayerGivenUp(BattleBitPlayer player)
    {
        foreach (var @event in events)
            await @event.OnPlayerGivenUp(player);
    }
    
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        foreach (var @event in events)
            await @event.OnAPlayerDownedAnotherPlayer(args);
    }
    
    public override async Task OnAPlayerRevivedAnotherPlayer(BattleBitPlayer from, BattleBitPlayer to)
    {
        foreach (var @event in events)
            await @event.OnAPlayerRevivedAnotherPlayer(from, to);
    }
    
    public override async Task OnPlayerReported(BattleBitPlayer from, BattleBitPlayer to, ReportReason reason, string additional)
    {
        foreach (var @event in events)
            await @event.OnPlayerReported(from, to, reason, additional);
    }
    
    public override async Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        foreach (var @event in events)
            await @event.OnGameStateChanged(oldState, newState);
    }
    
    public override async Task OnRoundStarted()
    {
        foreach (var @event in events)
            await @event.OnRoundStarted();
    }
    
    public override async Task OnRoundEnded()
    {
        foreach (var @event in events)
            await @event.OnRoundEnded();
    }
    
    public override async Task OnSessionChanged(long oldSessionID, long newSessionID)
    {
        foreach (var @event in events)
            await @event.OnSessionChanged(oldSessionID, newSessionID);
    }
    
    private async Task<bool> RunEventWithBoolReturn(Func<Event, Task<bool>> func)
    {
        bool returnValue = true;
        
        foreach (var @event in events)
        {
            if (!await func(@event))
            {
                returnValue = false;
            }
        }
        
        return returnValue;
    }
    
    private async Task<OnPlayerSpawnArguments?> RunEventWithOnPlayerSpawnArgumentsReturn(Func<Event, OnPlayerSpawnArguments, Task<OnPlayerSpawnArguments?>> func, OnPlayerSpawnArguments request)
    {
        OnPlayerSpawnArguments? returnRequest = request;
        OnPlayerSpawnArguments oldRequest = request;
        
        foreach (var @event in events)
        {
            var returnResult = await func(@event, oldRequest);
            if (returnRequest != null)
                oldRequest = returnRequest.Value;
            
            if (returnRequest != null && returnResult != null)
                returnRequest = returnResult.Value;
            else
                returnRequest = null;
        }
        
        return returnRequest;
    }
}