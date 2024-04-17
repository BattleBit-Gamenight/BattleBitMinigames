﻿using System.Collections.Concurrent;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;
using BattleBitMinigames.Interfaces;

namespace BattleBitMinigames.Events;

public class HideAndSeekGamemode : Event
{
    // GAMEMODE SETTINGS
    private int RequiredPlayerCountToStart { get; set; } = 4;
    private int HideTimeDuration { get; set; } = 180;
    
    // HIDER SETTINGS
    private float HiderRunSpeedMultiplierDuringCountdown { get; set; } = 2.0f;
    private float HiderRunSpeedMultiplierDuringGame { get; set; } = 0.6f;
    private float HiderJumpHeightMultiplierDuringCountdown { get; set; } = 2.0f;
    private float HiderJumpHeightMultiplierDuringGame { get; set; } = 1.0f;
    private float HiderFallDamageMultiplier { get; set; } = 0.0f;
    private float HiderGiveDamageMultiplier { get; set; } = 0.0f;
    private float HiderReceiveDamageMultiplier { get; set; } = 100.0f;
    
    // SEEKER SETTINGS
    private float SeekerRunSpeedMultiplier { get; set; } = 1.5f;
    private float SeekerJumpHeightMultiplier { get; set; } = 1.5f;
    private float SeekerFallDamageMultiplier { get; set; } = 0.0f;
    private float SeekerGiveDamageMultiplier { get; set; } = 100.0f;
    private float SeekerReceiveDamageMultiplier { get; set; } = 0.0f;
    
    // DO NOT CHANGE THESE VALUES
    private MinigameStates State { get; set; } = MinigameStates.WaitingForPlayers;
    
    // Game Logic
    readonly Random _random = new();
    
    private static bool IsPlayerSeeking(BattleBitApiPlayer player)
    {
        var value = player.GetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.IsSeeking);
        return value != string.Empty && value != "false";
    }
    
    private static int GetPlayerHidersFound(BattleBitApiPlayer player)
    {
        var value = player.GetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.HidersFound);
        return value != string.Empty ? int.Parse(value) : 0;
    }
    
    private static void AddPlayerHidersFound(BattleBitApiPlayer player)
    {
        var hidersFound = GetPlayerHidersFound(player);
        player.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.HidersFound, (hidersFound + 1).ToString());
        Program.Logger.Info($"{player.Name} has found {hidersFound + 1} hiders!");
    }
    
    private static void MakePlayerSeeker(BattleBitApiPlayer player)
    {
        player.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.IsSeeking, "true");
        player.ChangeTeam(Team.TeamA);
        player.Modifications.CanSpectate = false;
        Program.Logger.Info($"{player.Name} is now a seeker!");
    }
    
    private static void MakePlayerHider(BattleBitApiPlayer player)
    {
        player.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.IsSeeking, "false");
        player.ChangeTeam(Team.TeamB);
        player.Modifications.CanSpectate = false;
        Program.Logger.Info($"{player.Name} is now a hider!");
    }
    
    // Set default values for the hide and seek player properties
    private static void SetDefaultPlayerProperties(BattleBitApiPlayer player)
    {
        player.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.IsSeeking, "false");
        player.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.SeekingMeter, "NO LIFE DETECTED (300m+)");
        player.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.HidersFound, "0");
        Program.Logger.Info($"Set default properties for {player.Name}");
    }
    
    private static void ClearPlayerProperties(BattleBitApiPlayer player)
    {
        player.RemovePlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.IsSeeking);
        player.RemovePlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.SeekingMeter);
        player.RemovePlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.HidersFound);
        Program.Logger.Info($"Cleared properties for {player.Name}");
    }

    private async void StartPlayerSeekerMeter()
    {
        Program.Logger.Info("Starting player seeker meter!");
        
        // Every 1 second check if a seeker is near a hider
        while (State == MinigameStates.Running)
        {
            try
            {
                var seekers = Server.AllPlayers.Where(IsPlayerSeeking).ToList();
                var hiders = Server.AllPlayers.Where(player => player.IsAlive && IsPlayerSeeking(player)).ToList();

                seekers.ForEach(async seeker =>
                {
                    // Initialize the closest distance to a very high value
                    double closestDistance = double.MaxValue;

                    hiders.ForEach(hider =>
                    {
                        // Calculate the distance between the seeker and each hider
                        var distance = FormattingHelper.DistanceTo(seeker.Position, hider.Position);

                        // Update the closest distance if this hider is closer than any previously checked
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                        }
                    });

                    // Update the SeekingMeter based on the closest hider found
                    seeker.SetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.SeekingMeter, closestDistance switch
                    {
                        <= 25 => $"{RichTextHelper.FromColorName("IndianRed")}SPICY{RichTextHelper.FromColorName("Snow")} (0-25m)",
                        <= 50 => $"{RichTextHelper.FromColorName("Red")}HOT{RichTextHelper.FromColorName("Snow")} (25-50m)",
                        <= 75 => $"{RichTextHelper.FromColorName("Orange")}WARM{RichTextHelper.FromColorName("Snow")} (50-75m)",
                        <= 150 => $"{RichTextHelper.FromColorName("Blue")}COLD{RichTextHelper.FromColorName("Snow")} (75-150m)",
                        <= 300 => $"{RichTextHelper.FromColorName("Blue")}FREEZING{RichTextHelper.FromColorName("Snow")} (150-300m)",
                        _ => $"{RichTextHelper.FromColorName("Violet")}NO LIFE DETECTED{RichTextHelper.FromColorName("Snow")} (300m+)"
                    });

                    await Task.Delay(5);
                });
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }

            await Task.Delay(5);
        }
    }

    private async void StartWaitingForPlayersMessage()
    {
        Program.Logger.Info("Waiting for players to join!");
        while (State == MinigameStates.WaitingForPlayers && Server.CurrentPlayerCount < RequiredPlayerCountToStart)
        {
            try
            {
                var message = new StringBuilder();
                var playersNeeded = RequiredPlayerCountToStart - Server.CurrentPlayerCount;
                message.AppendLine($"{RichTextHelper.Size(150)}BattleBit Hide and Seek!{RichTextHelper.Size(100)}");
                message.AppendLine("WaitingForPlayers for players to join! Need " + playersNeeded + " more players to start!");
                Server.AnnounceShort(message.ToString());
            }
            catch (Exception e)
            {                                               
                Program.Logger.Error(e.Message);
            }
            await Task.Delay(1000);
        }
        
        StartHideAndSeek();
    }

    private async void StartInfoMessage()
    {
        while (Server.IsConnected)
        {
            try
            {
                var totalSeekers = Server.AllPlayers.Count(IsPlayerSeeking);
                var totalHiders =
                    Server.AllPlayers.Count(player => player.IsAlive && !IsPlayerSeeking(player));
                foreach (var player in Server.AllPlayers)
                {
                    player.Modifications.CanSpectate = false;
                    var isSeeking = IsPlayerSeeking(player);
                    var seekerMeter = player.GetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.SeekingMeter);
                    var message = new StringBuilder();
                    message.AppendLine(
                        $"{RichTextHelper.Size(100)}{RichTextHelper.FromColorName("Snow")}BattleBit Hide and Seek!{RichTextHelper.Size(100)}");
                    switch (State)
                    {
                        case MinigameStates.SelectingSeekers:
                            message.AppendLine(
                                $"{RichTextHelper.Size(140)}{RichTextHelper.FromColorName("Orange")}Randomly selecting seekers!{RichTextHelper.Size(100)}");
                            break;
                        case MinigameStates.CountingDown:
                        {
                            // Tell hiders they need to hide
                            if (!isSeeking)
                            {
                                message.AppendLine(
                                    $"{RichTextHelper.Size(150)}{RichTextHelper.FromColorName("Orange")}You are currently " +
                                    (isSeeking ? "seeking" : "hiding") + RichTextHelper.Size(100) +
                                    RichTextHelper.FromColorName("Snow"));
                                message.AppendLine(
                                    $"{RichTextHelper.FromColorName("Orange")}Hide from the seekers!{RichTextHelper.NewLine()}You have 3 minutes and can't use vehicles!");
                            }
                            // Tell seekers they need to seek
                            else
                            {
                                message.AppendLine(
                                    $"{RichTextHelper.Size(150)}{RichTextHelper.FromColorName("Orange")}You are currently " +
                                    (isSeeking ? "seeking" : "hiding") + RichTextHelper.Size(100) +
                                    RichTextHelper.FromColorName("Snow"));
                                message.AppendLine(
                                    $"{RichTextHelper.FromColorName("Orange")}Once the countdown ends you have 3 minutes to find them!");
                            }

                            message.AppendLine(
                                $"{RichTextHelper.FromColorName("Snow")}Seekers: {totalSeekers} | Hiders: {totalHiders}");
                            break;
                        }
                        case MinigameStates.Running:
                        {
                            message.AppendLine(
                                $"{RichTextHelper.Size(150)}{RichTextHelper.FromColorName("Orange")}You are currently " +
                                (isSeeking ? "seeking" : "hiding") + RichTextHelper.Size(100) +
                                RichTextHelper.FromColorName("Snow"));
                            message.AppendLine("Coords: " + player.Position.X + ", " +
                                               player.Position.Y + ", " +
                                               player.Position.Z);
                            message.AppendLine("Seekers: " + totalSeekers + " | " + "Hiders: " + totalHiders);
                            if (isSeeking)
                            {
                                message.AppendLine("Seeking Meter: " + seekerMeter);
                            }

                            break;
                        }
                        case MinigameStates.Ending:
                            message.AppendLine(
                                $"{RichTextHelper.Size(140)}{RichTextHelper.FromColorName("Orange")}Game has ended!{RichTextHelper.Size(100)}");
                            break;
                        default:
                            message.AppendLine(
                                $"{RichTextHelper.Size(140)}{RichTextHelper.FromColorName("Orange")}WaitingForPlayers for players to join!{RichTextHelper.Size(100)}");
                            message.AppendLine(
                                $"{RichTextHelper.FromColorName("Snow")}Players needed to start: {RequiredPlayerCountToStart - Server.CurrentPlayerCount}");
                            break;
                    }

                    player.Message(message.ToString(), 1000);
                }
            }
            catch (Exception e)
            {
                Program.Logger.Error(e.Message);
            }

            await Task.Delay(100);
        }
    }
    
    private async void StartHideAndSeek()
    {
        try
        {
            Program.Logger.Info("Hide and Seek gamemode started!");
        
            State = MinigameStates.SelectingSeekers;

            Program.Logger.Info("Cleared all player properties!");
            foreach (var player in Server.AllPlayers)
            {
                player.ChangeTeam(Team.TeamB);
                player.Kill();
                ClearPlayerProperties(player);
            }
            
            Program.Logger.Info("Preparing to select seekers!");
            var countdown = 30;
            while (countdown > 0)
            {
                try
                {
                    Server.AnnounceShort(
                        $"Randomly selecting seekers in {FormattingHelper.GetFormattedTimeFromSeconds(countdown)}!");
                    await Task.Delay(1000);
                    countdown--;

                    if (State != MinigameStates.SelectingSeekers || State == MinigameStates.Ending) return;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            Program.Logger.Info("Setting default player properties!");
            // Add all players to the hide and seek list
            foreach (var player in Server.AllPlayers)
            {
                try
                {
                    SetDefaultPlayerProperties(player);
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            Program.Logger.Info("Selecting seekers and setting their properties!");
            // Get 1/16th of the players to be seekers and a maximum of 8 and minimum of 1
            // Randomize the players
            var seekers = Server.AllPlayers.OrderBy(player => _random.Next()).Take(Math.Clamp(Server.CurrentPlayerCount / 16, 1, 8)).ToList();

            foreach (var seeker in seekers)
            {
                try
                {
                    Program.Logger.Info(seeker.Name);
                    MakePlayerSeeker(seeker);
                    seeker.Modifications.CanDeploy = false;
                    seeker.Modifications.CanSpectate = false;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            State = MinigameStates.CountingDown;
            
            Program.Logger.Info("Setting hiders properties!");
            // Get all other players to be the hiders
            var hiders = Server.AllPlayers.Where(player => !IsPlayerSeeking(player)).ToList();
            foreach (var hider in hiders)
            {
                try
                {
                    MakePlayerHider(hider);
                    hider.Modifications.CanDeploy = true;
                    hider.Modifications.RunningSpeedMultiplier = HiderRunSpeedMultiplierDuringCountdown;
                    hider.Modifications.JumpHeightMultiplier = HiderJumpHeightMultiplierDuringCountdown;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            Program.Logger.Info("Starting countdown timer!");
            // Create countdown timer that counts down from 3 minutes and update the announcement every 1 second with the remaining time
            countdown = HideTimeDuration;
            int hidersCount;
            while (countdown > 0)
            {
                try
                {
                    Server.AnnounceShort(
                        $"Hide and Seek will start in {FormattingHelper.GetFormattedTimeFromSeconds(countdown)}!{RichTextHelper.NewLine()}Seekers will be able to deploy once the game starts!");
                    await Task.Delay(1000);
                    countdown--;
                    
                    foreach (var player in Server.AllPlayers)
                    {
                        try
                        {
                            if (IsPlayerSeeking(player))
                            {
                                PlayerHelpers.KillPlayerInVehicle(player,
                                    "Vehicles are not allowed until the game starts!");
                            }
                        }
                        catch (Exception e)
                        {
                            Program.Logger.Error(e.Message);
                        }
                    }
                    
                    if (State != MinigameStates.CountingDown && State != MinigameStates.Ending) return;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            // Get all other players to be the hiders
            hiders = Server.AllPlayers.Where(player => !IsPlayerSeeking(player)).ToList();
            foreach (var hider in hiders)
            {
                try
                {
                    hider.Modifications.RunningSpeedMultiplier = HiderRunSpeedMultiplierDuringGame;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            Server.AnnounceShort($"Hide and Seek Started and seekers can now spawn in!");
            
            State = MinigameStates.Running;
            await Task.Run(StartPlayerSeekerMeter);
            
            Program.Logger.Info("Allowing seekers to deploy!");
            // Get filter for all seekers
            var seekersFilter = Server.AllPlayers.Where(IsPlayerSeeking).ToList();
            foreach (var seeker in seekersFilter)
            {
                try
                {
                    seeker.Modifications.CanDeploy = true;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            Program.Logger.Info("Making all hiders that are not alive seekers!");
            // Get filter for all hiders not alive
            var hidersFilter = Server.AllPlayers.Where(player => !IsPlayerSeeking(player) && !player.IsAlive).ToList();
            foreach (var hider in hidersFilter)
            {
                try
                {
                    MakePlayerSeeker(hider);
                    hider.Modifications.CanDeploy = true;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            hidersCount = Server.AllPlayers.Count(player => !IsPlayerSeeking(player));
            if (hidersCount == 0)
            {
                Program.Logger.Info("All hiders have been found! Seekers win! Ending game!");
                Server.AnnounceLong("All hiders have been found! Seekers win!");
                Server.ForceEndGame(Team.TeamA);
            }
        }
        catch (Exception e)
        {
            Program.Logger.Error(e.Message);
        }
    }

    public override Task OnPlayerJoinedSquad(BattleBitApiPlayer player, Squad<BattleBitApiPlayer> squad)
    {
        player.KickFromSquad();
        return Task.CompletedTask;
    }

    public override Task OnPlayerChangeTeam(BattleBitApiPlayer player, Team team)
    {
        player.Modifications.CanSpectate = false;
        if (State is MinigameStates.Running or MinigameStates.CountingDown)
        {
            MakePlayerSeeker(player);
        
            var totalHiders = Server.AllPlayers.Count(hiderPlayer => !IsPlayerSeeking(hiderPlayer));
            if (totalHiders == 0 && State == MinigameStates.Running)
            {
                Server.AnnounceLong("All hiders have been found! Seekers win!");
                Server.ForceEndGame(Team.TeamA);
            }
        }
        
        return Task.CompletedTask;
    }

    /*public override Task<bool> OnPlayerTypedMessage(BattleBitApiPlayer player, ChatChannel channel, string msg)
    {
        if (msg == "start")
        {
            StartHideAndSeek();
            
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }*/

    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitApiPlayer player,
        OnPlayerSpawnArguments request)
    {
        player.Modifications.RespawnTime = 0.0f;
        player.Modifications.CaptureFlagSpeedMultiplier = 0.0f;
        
        if (IsPlayerSeeking(player))
        {
            request.Loadout = new PlayerLoadout();
            request.Loadout.HeavyGadget = Gadgets.SledgeHammer;
            request.Loadout.LightGadget = Gadgets.AirDrone;
            request.Loadout.Throwable = Gadgets.AntiPersonnelMine;
            request.Wearings.Head = "ANV2_Universal_UniC_Helmet_00_Red_N";
            request.Wearings.Chest = "ANV2_Universal_UniC_Armor_00_Red_N";
            request.Wearings.Belt = "ANV2_Universal_UniC_Belt_00_Red_S";
            request.Wearings.Backbag = "ANV2_Universal_UniC_Backpack_00_Red_N";
            request.Wearings.Uniform = "ANY_NU_Uniform_UniCRed_00";
            
            player.Modifications.FallDamageMultiplier = SeekerFallDamageMultiplier;
            player.Modifications.ReceiveDamageMultiplier = SeekerReceiveDamageMultiplier;
            player.Modifications.GiveDamageMultiplier = SeekerGiveDamageMultiplier;
            player.Modifications.RunningSpeedMultiplier = SeekerRunSpeedMultiplier;
            player.Modifications.JumpHeightMultiplier = SeekerJumpHeightMultiplier;
            player.Modifications.CanSuicide = true;
            player.Modifications.HideOnMap = false;
            player.Modifications.IsExposedOnMap = true;
            player.Modifications.AllowedVehicles = VehicleType.All;
            player.Modifications.FriendlyHUDEnabled = true;
        }
        else
        {
            request.Loadout = new PlayerLoadout();
            request.Loadout.LightGadget = Gadgets.BinoSoflam;
            request.Loadout.Throwable = Gadgets.SmokeGrenadeWhite;
            player.Modifications.FallDamageMultiplier = HiderFallDamageMultiplier;
            player.Modifications.ReceiveDamageMultiplier = HiderReceiveDamageMultiplier;
            player.Modifications.GiveDamageMultiplier = HiderGiveDamageMultiplier;
            player.Modifications.RunningSpeedMultiplier = HiderRunSpeedMultiplierDuringGame;
            player.Modifications.JumpHeightMultiplier = HiderJumpHeightMultiplierDuringGame;
            player.Modifications.CanSuicide = false;
            player.Modifications.HideOnMap = true;
            player.Modifications.IsExposedOnMap = false;
            player.Modifications.AllowedVehicles = VehicleType.None;
            player.Modifications.FriendlyHUDEnabled = false;
        }
        
        return request;
    }

    public override async Task OnPlayerSpawned(BattleBitApiPlayer player)
    {
        await Task.Delay(50);
        
        if (player.InVehicle)
        {
            if (IsPlayerSeeking(player) && State != MinigameStates.Running)
            {
                PlayerHelpers.KillPlayerInVehicle(player, "Vehicles are not allowed until the game starts!");
            }
            
            if (!IsPlayerSeeking(player))
            {
                PlayerHelpers.KillPlayerInVehicle(player, "Hiders are not allowed to use vehicles!");
            }
        }
    }

    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitApiPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        
        if (killer == null) return Task.CompletedTask;
        
        if (IsPlayerSeeking(killer) && !IsPlayerSeeking(victim))
        {
            AddPlayerHidersFound(killer);
            MakePlayerSeeker(victim);
            Server.SayToAllChat($"{RichTextHelper.Bold(true)}{killer.Name}{RichTextHelper.Bold(false)} found {RichTextHelper.Bold(true)}{victim.Name}{RichTextHelper.Bold(false)}! They've found {killer.GetPlayerProperty(PlayerProperties.IHideAndSeekPlayerProperties.HidersFound)} hiders!");
        }
        
        // If all hiders are found, end the game
        if (Server.AllPlayers.Count(player => player.IsAlive && !IsPlayerSeeking(player)) == 0)
        {
            Server.AnnounceLong("All hiders have been found! Seekers win!");
            Server.ForceEndGame(Team.TeamA);
        }
        
        return Task.CompletedTask;
    }

    public override Task OnConnected()
    {
        Server.MapRotation.SetRotation(
            "Azagor",
            "Basra",
            "Construction",
            "District",
            "Dustydew",
            "Eduardovo",
            "Frugis",
            "Isle",
            "Kodiak",
            "Lonovo",
            "Multuislands",
            "Namak",
            "Oildunes",
            "River",
            "Salhan",
            "SandySunset",
            "TensaTown",
            "Valley",
            "Wakistan",
            "WineParadise",
            "Zalfibay",
            "Old_Multuislands",
            "Old_Eduardovo",
            "Old_Namak",
            "Old_District",
            "Old_Oildunes"
        );
        Server.GamemodeRotation.SetRotation("INFCONQ");
        Server.ExecuteCommand("setsize ultra");
        Server.ExecuteCommand("setspeedhackdetection false");

        Task.Run(StartInfoMessage);
        Task.Run(StartWaitingForPlayersMessage);
        Server.RoundSettings.PlayersToStart = RequiredPlayerCountToStart;
        
        return Task.CompletedTask;
    }

    public override Task OnPlayerDisconnected(BattleBitApiPlayer player)
    {
        var totalHiders = Server.AllPlayers.Count(player => !IsPlayerSeeking(player));
        if (totalHiders == 0 && State == MinigameStates.Running)
        {
            Server.AnnounceLong("All hiders have been found! Seekers win!");
            Server.ForceEndGame(Team.TeamA);
        }
        
        return Task.CompletedTask;
    }

    public override async Task OnPlayerConnected(BattleBitApiPlayer player)
    {
        player.Modifications.CanSpectate = false;
        player.Modifications.CaptureFlagSpeedMultiplier = 0.0f;
        
        if (State is MinigameStates.Running or MinigameStates.CountingDown)
        {
            MakePlayerSeeker(player);
            player.Modifications.CanDeploy = State != MinigameStates.CountingDown;
        } else {
            MakePlayerHider(player);
            player.Modifications.CanDeploy = false;
        }
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                Server.RoundSettings.SecondsLeft = 10000;
                Server.RoundSettings.TeamATickets = 10000;
                Server.RoundSettings.TeamBTickets = 10000;
                break;
            case GameState.CountingDown:
                Task.Run(StartWaitingForPlayersMessage);
                Server.RoundSettings.SecondsLeft = 0;
                break;
            case GameState.WaitingForPlayers:
                State = MinigameStates.WaitingForPlayers;
                Server.RoundSettings.PlayersToStart = 1;
                break;
            case GameState.EndingGame:
                State = MinigameStates.Ending;
                Server.RoundSettings.SecondsLeft = 0;
                break;
        }

        return Task.CompletedTask;
    }
}