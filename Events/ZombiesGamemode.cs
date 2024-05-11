using System.Text;
using BattleBitAPI.Common;
using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;
using BattleBitMinigames.Interfaces;

namespace BattleBitMinigames.Events;

public class ZombiesGamemode : Event
{
    // GAMEMODE SETTINGS
    private int RequiredPlayerCountToStart { get; set; } = 1;
    private int SelectingInfectedDuration { get; set; } = 60;
    private int HumanPrepTimeDuration { get; set; } = 90;
    
    // DO NOT CHANGE THESE VALUES
    private InfectedGameStates State { get; set; } = InfectedGameStates.WaitingForPlayers;
    
    // Game Logic
    readonly Random _random = new();

    private void StartRandomHumanSpotter()
    {
        Task.Run(async () =>
        {
            while (State == InfectedGameStates.Running)
            {
                try
                {
                    // Get a random player from Team A
                    var player = Server.AllPlayers.Where(p => p.Team == Team.TeamA).MinBy(p => _random.Next());
                    if (player == null) return;
                    player.Modifications.IsExposedOnMap = true;
                    await Task.Delay(500);
                    player.Modifications.IsExposedOnMap = false;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
                await Task.Delay(1000);
            }
        });
    }

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
        
        var loadingScreen = new StringBuilder();
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(250)}BattleBit Infected!{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)}");
        // Short lore about the gamemode
        loadingScreen.AppendLine(
            "The infection has spread across the world, and you are one of the few survivors left.");
        loadingScreen.AppendLine(
            "You must work together to survive the infection and make it out alive!");
        loadingScreen.AppendLine(" ");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(150)}Zombie Types:{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)}");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(125)}Normal{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)} - A standard zombie that's slightly faster than a human.");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(125)}Fast{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)} - A zombie with increased speed.");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(125)}Leaper{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)} - A zombie with increased speed and jump height.");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(125)}Boomer{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)} - A zombie that's equipped with suicide c4.");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(125)}Tank{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)} - A zombie that's equipped with riot shield and very strong.");
        
        // Rules
        loadingScreen.AppendLine(" ");
        loadingScreen.AppendLine($"{RichTextHelper.Bold(true)}{RichTextHelper.Size(150)}Rules:{RichTextHelper.Size(100)}{RichTextHelper.Bold(false)}");
        loadingScreen.AppendLine($"- Do not enter water");
        loadingScreen.AppendLine($"- Do not stay in safe zone");
        loadingScreen.AppendLine($"- Do not defend anywhere infected can't access");
        Server.SetLoadingScreenText(loadingScreen.ToString());
        
        Server.MapRotation.SetRotation(
            "Construction",
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
        Server.ServerSettings.UnlockAllAttachments = true;
        Server.ServerSettings.PlayerCollision = true;
        Server.ServerSettings.HideMapVotes = false;
    }

    private async void StartGame()
    {
        Program.Logger.Info("Starting Infected gamemode");
        
        State = InfectedGameStates.Starting;
        Server.ClearAllPlayerProperties();
        await SetDefaultPlayerSettings();
        UpdateAllPlayerSideMessages();
        await StartInfectedSelectionCountdown();
        await InfectRandomPlayers(AmountOfPlayersToInfectAtStart());
        UpdateAllPlayerSideMessages();
        await StartHumanPrepTime();
        await InfectNonDeployedPlayers();
        await ReleaseTheInfected();
        UpdateAllPlayerSideMessages();
        State = InfectedGameStates.Running;
        StartRandomHumanSpotter();
    }
    
    private void SetSquadPoints()
    {
        Program.Logger.Info("Setting squad points");
        
        // Set squad points for each squad on Team A
        foreach (var squad in Server.TeamASquads)
        {
            try
            {
               squad.SquadPoints = 100000;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
        
        // Set squad points for each squad on Team B
        foreach (var squad in Server.TeamBSquads)
        {
            try
            {
                squad.SquadPoints = 1000;
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
    }
    
    private int GetPlayerKillsAsInfected(BattleBitPlayer player)
    {
        var kills = player.GetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.KillsAsInfected);
        
        if (kills != "") return int.Parse(kills);
        
        player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.KillsAsInfected, "0");
        return 0;

    }
    
    private void IncrementPlayerKillsAsInfected(BattleBitPlayer player)
    {
        var kills = GetPlayerKillsAsInfected(player);
        kills++;
        player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.KillsAsInfected, kills.ToString());
    }
    
    private int GetPlayerKillsAsHuman(BattleBitPlayer player)
    {
        var kills = player.GetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.KillsAsHuman);
        
        if (kills != "") return int.Parse(kills);
        
        player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.KillsAsHuman, "0");
        return 0;

    }
    
    private void IncrementPlayerKillsAsHuman(BattleBitPlayer player)
    {
        var kills = GetPlayerKillsAsHuman(player);
        kills++;
        player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.KillsAsHuman, kills.ToString());
    }
    
    private void UpdatePlayerSideMessage(BattleBitPlayer player)
    {
        var message = new StringBuilder();
        message.AppendLine($"{RichTextHelper.Bold(true)}BattleBit Infected{RichTextHelper.Bold(false)} {RichTextHelper.Size(75)}(Beta){RichTextHelper.Size(100)}");
        message.AppendLine("");
        
        if (IsPlayerInfected(player))
        {
            message.AppendLine($"You are a {RichTextHelper.Bold(true)}{player.GetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType)}{RichTextHelper.Bold(false)} zombie!");
            message.AppendLine($"You've infected {RichTextHelper.Bold(true)}{GetPlayerKillsAsInfected(player)}{RichTextHelper.Bold(false)} humans!");
        }
        else
        {
            message.AppendLine($"You are a {RichTextHelper.Bold(true)}human{RichTextHelper.Bold(false)}!");
            message.AppendLine($"You've killed {RichTextHelper.Bold(true)}{GetPlayerKillsAsHuman(player)}{RichTextHelper.Bold(false)} infected!");
        }

        message.AppendLine(" ");
        message.AppendLine($"Humans: {RichTextHelper.Bold(true)}{Server.AllPlayers.Count(p => p.Team == Team.TeamA)}{RichTextHelper.Bold(false)} | Infected: {RichTextHelper.Bold(true)}{Server.AllPlayers.Count(p => p.Team == Team.TeamB)}{RichTextHelper.Bold(false)}");
        player.Message(message.ToString(), 1800);
    }
    
    private async void UpdateAllPlayerSideMessages()
    {
        Program.Logger.Info("Updating all player side messages");

        await Task.Delay(100);
        foreach (var player in Server.AllPlayers)
        {
            try
            {
                UpdatePlayerSideMessage(player);
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
        }
    }
    
    private async void CheckForWinConditions()
    {
        Program.Logger.Info("Checking for win conditions");
        
        await Task.Delay(1000);
        
        // Check if count of alive players on team a is 0
        if (Server.AllPlayers.Count(p => p.Team == Team.TeamA && p.IsAlive) == 0)
        {
            Server.AnnounceShort("The infected have won!");
            State = InfectedGameStates.Ending;
            Server.ForceEndGame(Team.TeamB);
        }
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
        State = InfectedGameStates.HumanPrepTime;
        
        await Task.Delay(100);
        
        // Give the human squads lots of squad points
        SetSquadPoints();
        
        Server.AnnounceLong("The infected have been selected! Humans have a short amount of time to prepare before the infected are released!");
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
        
        await Task.Delay(10000);
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
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }
            if (State != InfectedGameStates.SelectingInfected || State == InfectedGameStates.Ending) return;
            countdown--;
            await Task.Delay(1000);
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

    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        
        if (victim == null) return Task.CompletedTask;
        if (State != InfectedGameStates.Running) return Task.CompletedTask;

        if (killer == victim)
        {
            if (IsPlayerInfected(killer)) return Task.CompletedTask;
            Server.SayToAllChat($"{killer.Name} has killed themselves and has been infected!");
            InfectPlayer(killer);
            UpdateAllPlayerSideMessages();
            return Task.CompletedTask;
        }
        
        if (IsPlayerInfected(killer))
        {
            killer.Squad.SquadPoints += 10000;
            Server.SayToAllChat($"{killer.Name} has infected {victim.Name}!");
            InfectPlayer(victim);
            IncrementPlayerKillsAsInfected(killer);
            CheckForWinConditions();
        }
        else
        {
            killer.Squad.SquadPoints += 250;
            IncrementPlayerKillsAsHuman(killer);
        }
        
        UpdateAllPlayerSideMessages();
        
        return Task.CompletedTask;
    }

    public override Task OnPlayerDisconnected(BattleBitPlayer player)
    {
        // Check if there are no humans left
        if (Server.AllPlayers.Count(p => p.Team == Team.TeamA && p.IsAlive) == 0 && State == InfectedGameStates.Running)
        {
            Server.ForceEndGame(Team.TeamB);
        }
        
        UpdateAllPlayerSideMessages();
        
        return Task.CompletedTask;
    }

    public override Task OnPlayerSpawned(BattleBitPlayer player)
    {
        UpdatePlayerSideMessage(player);
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
        player.ClearAllPlayerProperties();
        
        UpdateAllPlayerSideMessages();
        
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
            case GameState.EndingGame:
                CheckForWinConditions();
                break;
        }
        
        return Task.CompletedTask;
    }

    public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        // Make sure the player isn't exposed on the map when they spawn
        player.Modifications.IsExposedOnMap = false;
        
        if (IsPlayerInfected(player))
        {
            // Infected player settings
            player.Modifications.FallDamageMultiplier = 0.0f;
            player.Modifications.MinimumHpToStartBleeding = 0;
            player.Modifications.MinimumDamageToStartBleeding = 10000;
            
            // Clear the player's loadout
            request.Loadout = new PlayerLoadout();
            
            // Set default infected player wearings
            request.Wearings = PlayerOutfits.Zombie;
            
            // Set default infected loadout
            request.Loadout.HeavyGadget = Gadgets.SledgeHammer;
            // Random 50% chance to get grapple
            if (_random.Next(100) < 50)
            {
                request.Loadout.LightGadget = Gadgets.GrapplingHook;
            }
            else
            {
                request.Loadout.Throwable = Gadgets.SmokeGrenadeRed;
                request.Loadout.ThrowableExtra = 1;
            }
            
            // Randomly become either a normal zombie (80% chance), fast zombie (5% chance), leaper zombie (5% chance), boomer zombie (5% chance), or tank zombie (5% chance)
            var random = _random.Next(100);
            if (random < 80) // Normal zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "normal");
                player.Modifications.RunningSpeedMultiplier = 1.1f;
                player.Modifications.JumpHeightMultiplier = 1.1f;
                
                // Send server announce log message
                Server.UILogOnServer($"A normal zombie has spawned!", 3);
            }
            else if (random < 85) // Fast zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "fast");
                player.Modifications.RunningSpeedMultiplier = 1.5f;
                player.Modifications.JumpHeightMultiplier = 1.1f;
                player.Modifications.IsExposedOnMap = true;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.BlueTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.BlueTeam.Belt;
                request.Wearings.Chest = PlayerOutfits.BlueTeam.Chest;
                request.Wearings.Head = PlayerOutfits.BlueTeam.Head;
                
                // Send server announce log message
                Server.UILogOnServer($"A fast zombie has spawned!", 6);
            }
            else if (random < 90) // Leaper zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "leaper");
                player.Modifications.RunningSpeedMultiplier = 1.5f;
                player.Modifications.JumpHeightMultiplier = 5.0f;
                player.Modifications.IsExposedOnMap = true;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.GreenTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.GreenTeam.Belt;
                request.Wearings.Chest = PlayerOutfits.GreenTeam.Chest;
                request.Wearings.Head = PlayerOutfits.GreenTeam.Head;
                
                // Send server announce log message
                Server.UILogOnServer($"A leaper zombie has spawned!", 6);
            }
            else if (random < 95) // Boomer zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "boomer");
                player.Modifications.RunningSpeedMultiplier = 1.25f;
                player.Modifications.IsExposedOnMap = true;
                
                // Player loadout
                request.Loadout.LightGadget = Gadgets.SuicideC4;
                request.Loadout.HeavyGadget = Gadgets.SledgeHammer;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.YellowTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.YellowTeam.Belt;
                request.Wearings.Chest = PlayerOutfits.YellowTeam.Chest;
                request.Wearings.Head = PlayerOutfits.YellowTeam.Head;
                
                // Send server announce log message
                Server.UILogOnServer($"A boomer zombie has spawned!", 6);
            }
            else // Tank zombie loadout
            {
                // Player settings
                player.SetPlayerProperty(PlayerProperties.IInfectedPlayerProperties.ZombieType, "tank");
                player.Modifications.ReceiveDamageMultiplier = 0.5f;
                player.Modifications.RunningSpeedMultiplier = 0.75f;
                player.Modifications.IsExposedOnMap = true;
                
                // Player loadout
                request.Loadout.HeavyGadget = Gadgets.RiotShield;
                request.Loadout.LightGadget = Gadgets.SledgeHammer;
                
                // Player wearings
                request.Wearings.Backbag = PlayerOutfits.RedTeam.Backbag;
                request.Wearings.Belt = PlayerOutfits.RedTeam.Belt;
                request.Wearings.Head = PlayerOutfits.RedTeam.Head;
                
                // Send server announce log message
                Server.UILogOnServer($"A tank zombie has spawned!", 6);
            }
        }
        else
        {
            // Player settings
            player.Modifications.GiveDamageMultiplier = 0.5f;
            player.Modifications.ReceiveDamageMultiplier = 0.5f;
            
            // Player loadout
            request.Loadout.FirstAidExtra = 1;
            request.Loadout.Throwable = Gadgets.SmokeGrenadeWhite;
            request.Loadout.ThrowableExtra = 3;
        }

        return Task.FromResult<OnPlayerSpawnArguments?>(request);
    }
    
}

