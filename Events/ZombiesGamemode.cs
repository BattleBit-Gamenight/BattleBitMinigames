using System.Collections.Concurrent;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;
using BattleBitMinigames.Interfaces;

namespace BattleBitMinigames.Events;

public class ZombiesGamemode : Event
{
    // GAMEMODE SETTINGS
    private int RequiredPlayerCountToStart { get; set; } = 1;
    private int SelectingInfectedDuration { get; set; } = 5;
    private int HumanPrepTimeDuration { get; set; } = 15;
    
    // DO NOT CHANGE THESE VALUES
    private InfectedGameStates State { get; set; } = InfectedGameStates.WaitingForPlayers;
    
    // Game Logic
    readonly Random _random = new();

    private int AmountOfPlayersToInfectAtStart()
    {
        return Math.Clamp(Server.CurrentPlayerCount / 16, 1, 8);
    }

    private static void InfectPlayer(BattleBitPlayer player)
    {
        Program.Logger.Info($"Infecting player {player.Name}");
        
        player.Kill();
        player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.IsInfected, "true");
        player.ChangeTeam(Team.TeamB);
    }
    
    private static void CurePlayer(BattleBitPlayer player)
    {
        Program.Logger.Info($"Curing player {player.Name}");
        
        player.Kill();
        player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.IsInfected, "false");
        player.ChangeTeam(Team.TeamA);
    }
    
    private bool IsPlayerInfected(BattleBitPlayer player)
    {
        return player.GetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.IsInfected) == "true";
    }

    private Task ReleaseTheInfected()
    {
        Program.Logger.Info("Releasing the infected!");
        
        // For each player in the server, allow them to deploy
        foreach (var player in Server.AllPlayers)
        {
            try
            {
                player.Modifications.CanDeploy = true;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        
        return Task.CompletedTask;
    }

    private void SetServerSettings()
    {
        Program.Logger.Info("Setting server settings for Infected gamemode");
        
        Server.MapRotation.SetRotation(
            "Construction",
            "Frugis",
            "Kodiak",
            "LonovoRegions",
            "SandySunset",
            "TensaTown",
            "WineParadise",
            "Zalfibay"
        );
        Server.GamemodeRotation.SetRotation("DOMI");
        Server.ExecuteCommand("setsize ultra");
        Server.ExecuteCommand("setspeedhackdetection false");
        Server.ExecuteCommand("setmaxping 999");
    }

    private async void StartGame()
    {
        Program.Logger.Info("Starting Infected gamemode");
        
        State = InfectedGameStates.Starting;
        Server.ClearAllPlayerProperties();
        await SetDefaultPlayerSettings();
        await StartInfectedSelectionCountdown();
        await InfectRandomPlayers(AmountOfPlayersToInfectAtStart());
        await StartHumanPrepTime();
        await InfectNonDeployedPlayers();
        await ReleaseTheInfected();
    }

    private Task InfectNonDeployedPlayers()
    {
        Program.Logger.Info("Infecting non-deployed players");
        
        // Infect any players that haven't been infected yet
        var players = Server.AllPlayers.Where(p => p.Team == Team.TeamA && !p.IsAlive);
        foreach (var player in players)
        {
            try
            {
                InfectPlayer(player);
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        
        return Task.CompletedTask;
    }

    private static void SendPlayerSpawnMessage(BattleBitPlayer player)
    {
        Program.Logger.Info($"Player {player.Name} has spawned");
        
        var message = new StringBuilder();
        message.Append("You are a ");
        message.Append(player.GetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType));
        message.Append(" zombie!");
        player.SayToChat(message.ToString());
    }

    private Task SetDefaultPlayerSettings()
    {
        Program.Logger.Info("Setting default player settings");
        
        // For each player in the server, set their default settings
        foreach (var player in Server.AllPlayers)
        {
            try
            {
                Program.Logger.Info($"Setting default settings for player {player.Name}");
                player.Modifications.CanDeploy = false;
                player.Modifications.CanSpectate = false;
                CurePlayer(player);
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        
        return Task.CompletedTask;
    }

    private async Task StartHumanPrepTime()
    {
        Program.Logger.Info("Starting human prep time");
        
        await Task.Delay(2000);
        State = InfectedGameStates.HumanPrepTime;
        var players = Server.AllPlayers.Where(p => p.Team == Team.TeamA);
        foreach (var player in players)
        {
            try
            {
                player.Modifications.CanDeploy = true;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        
        var countdown = HumanPrepTimeDuration;
        while (countdown > 0)
        {
            try
            {
                Server.AnnounceShort(
                    $"The infected will be released in {RichTextHelper.Bold(true)}{FormattingHelper.GetFormattedTimeFromSeconds(countdown)}{RichTextHelper.Bold(false)}!{RichTextHelper.NewLine()}Humans must fortify their defenses and attempt to survive!");
                await Task.Delay(1000);
                countdown--;
                    
                if (State != InfectedGameStates.HumanPrepTime) return;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        
        Server.AnnounceShort("The infected have been released! Good luck!");
    }

    private Task InfectRandomPlayers(int amount = 1)
    {
        Program.Logger.Info($"Infecting {amount} random players");
        
        Server.AnnounceShort($"Randomly infected {RichTextHelper.Bold(true)}{amount}{RichTextHelper.Bold(false)} humans!");
        
        // Get X amount of random players from Team A and infect them
        var players = Server.AllPlayers.Where(p => p.Team == Team.TeamA).OrderBy(p => _random.Next()).Take(amount);
        foreach (var player in players)
        {
            try
            {
                InfectPlayer(player);
                player.Modifications.CanDeploy = false;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        return Task.CompletedTask;
    }

    private async Task StartInfectedSelectionCountdown()
    {
        Program.Logger.Info("Starting infected selection countdown");
        
        State = InfectedGameStates.SelectingInfected;
        var countdown = SelectingInfectedDuration;
        while (countdown > 0 && State == InfectedGameStates.SelectingInfected)
        {
            try
            {
                Server.AnnounceShort(
                    $"Randomly selecting {RichTextHelper.Bold(true)}{AmountOfPlayersToInfectAtStart()}{RichTextHelper.Bold(false)} players to become infected in {RichTextHelper.Bold(true)}{FormattingHelper.GetFormattedTimeFromSeconds(countdown)}{RichTextHelper.Bold(false)}!");
                await Task.Delay(1000);
                countdown--;

                if (State != InfectedGameStates.SelectingInfected || State == InfectedGameStates.Ending) return;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
    }

    public override Task OnConnected()
    {
        Program.Logger.Info("Connected to server");
        
        SetServerSettings();
        if (Server.RoundSettings.State == GameState.Playing)
        {
            StartGame();
        }
        return Task.CompletedTask;
    }

    public override Task OnPlayerSpawned(BattleBitPlayer player)
    {
        SendPlayerSpawnMessage(player);
        return Task.CompletedTask;
    }

    public override Task OnPlayerChangeTeam(BattleBitPlayer player, Team team)
    {
        Program.Logger.Info($"Player {player.Name} has changed teams from {player.Team} to {team}");

        if (IsPlayerInfected(player))
        {
            player.ChangeTeam(Team.TeamB);
            return Task.CompletedTask;
        }

        if ((State == InfectedGameStates.Running || State == InfectedGameStates.HumanPrepTime) && team == Team.TeamA)
        {
            InfectPlayer(player);
        }
        else
        {
            player.ChangeTeam(Team.TeamA);
        }
        
        return Task.CompletedTask;
    }

    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        Program.Logger.Info($"Player {player.Name} has connected");
        
        // Set initial player settings
        player.Modifications.CanDeploy = false;
        player.Modifications.CanSpectate = false;
        
        switch (State)
        {
            case InfectedGameStates.WaitingForPlayers:
                CurePlayer(player);
                break;
            case InfectedGameStates.SelectingInfected:
                CurePlayer(player);
                break;
            case InfectedGameStates.HumanPrepTime:
                InfectPlayer(player);
                break;
            case InfectedGameStates.Running:
                InfectPlayer(player);
                player.Modifications.CanDeploy = true;
                break;
            case InfectedGameStates.Ending:
                CurePlayer(player);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return Task.CompletedTask;
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        Program.Logger.Info($"Game state changed from {oldState} to {newState}");
        
        switch (newState)
        {
            case GameState.WaitingForPlayers:
                State = InfectedGameStates.WaitingForPlayers;
                Server.RoundSettings.PlayersToStart = RequiredPlayerCountToStart;
                break;
            case GameState.CountingDown:
                StartGame();
                break;
            case GameState.Playing:
                Server.RoundSettings.SecondsLeft = 1800;
                Server.RoundSettings.TeamATickets = 10000;
                Server.RoundSettings.TeamBTickets = 10000;
                break;
        }
        
        return Task.CompletedTask;
    }

    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        player.Modifications.IsExposedOnMap = false;
        
        var isInfected = player.GetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.IsInfected) == "true";
        if (isInfected)
        {
            // Randomly become either a normal zombie (90% chance), fast zombie (5% chance), or tank zombie (5% chance)
            var random = _random.Next(100);
            if (random < 85) // Normal zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "normal");
                player.Modifications.RunningSpeedMultiplier = 1.1f;
                player.Modifications.JumpHeightMultiplier = 1.1f;
            }
            else if (random < 90) // Fast zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "fast");
                player.Modifications.RunningSpeedMultiplier = 1.5f;
                player.Modifications.JumpHeightMultiplier = 1.1f;
                player.Modifications.IsExposedOnMap = true;
                player.Modifications.IsExposedOnMap = true;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.BlueTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.BlueTeam.Belt;
                request.Wearings.Chest = PlayerOutfits.BlueTeam.Chest;
                request.Wearings.Head = PlayerOutfits.BlueTeam.Head;
            }
            else if (random < 95) // Leaper zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "leaper");
                player.Modifications.RunningSpeedMultiplier = 1.5f;
                player.Modifications.JumpHeightMultiplier = 2.5f;
                player.Modifications.IsExposedOnMap = true;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.GreenTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.GreenTeam.Belt;
                request.Wearings.Chest = PlayerOutfits.GreenTeam.Chest;
                request.Wearings.Head = PlayerOutfits.GreenTeam.Head;
            }
            else // Tank zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "tank");
                player.Modifications.ReceiveDamageMultiplier = 0.5f;
                player.Modifications.IsExposedOnMap = true;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.RedTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.RedTeam.Belt;
                request.Wearings.Chest = PlayerOutfits.RedTeam.Chest;
                request.Wearings.Head = PlayerOutfits.RedTeam.Head;
            }
        }
        else
        {
            // Player settings
            player.Modifications.GiveDamageMultiplier = 0.5f;
            player.Modifications.ReceiveDamageMultiplier = 0.5f;
        }

        return request;
    }
    
}

