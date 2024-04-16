using System.Collections.Concurrent;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;
using BattleBitMinigames.Helpers;

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
    private string MiniGameState { get; set; } = "waiting";
    
    // Game Logic
    readonly Random _random = new();
    private static ConcurrentDictionary<ulong, HideAndSeekPlayerData> HideAndSeekers = new ConcurrentDictionary<ulong, HideAndSeekPlayerData>();

    private async void StartPlayerSeekerMeter()
    {
        // Every 1 second check if a seeker is near a hider
        while (MiniGameState == "running")
        {
            try
            {
                var seekers = HideAndSeekers.Values.Where(player => player.IsSeeking).ToList();
                var hiders = HideAndSeekers.Values.Where(player => player is { IsSeeking: false, Player.IsAlive: true }).ToList();

                seekers.ForEach(async seeker =>
                {
                    // Initialize the closest distance to a very high value
                    double closestDistance = double.MaxValue;

                    hiders.ForEach(hider =>
                    {
                        // Calculate the distance between the seeker and each hider
                        var distance = FormattingHelper.DistanceTo(seeker.Player.Position, hider.Player.Position);

                        // Update the closest distance if this hider is closer than any previously checked
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                        }
                    });

                    // Update the SeekingMeter based on the closest hider found
                    seeker.SeekingMeter = closestDistance switch
                    {
                        <= 25 => $"{RichTextHelper.FromColorName("IndianRed")}SPICY{RichTextHelper.FromColorName("Snow")} (0-25m)",
                        <= 50 => $"{RichTextHelper.FromColorName("Red")}HOT{RichTextHelper.FromColorName("Snow")} (25-50m)",
                        <= 75 => $"{RichTextHelper.FromColorName("Orange")}WARM{RichTextHelper.FromColorName("Snow")} (50-75m)",
                        <= 150 => $"{RichTextHelper.FromColorName("Blue")}COLD{RichTextHelper.FromColorName("Snow")} (75-150m)",
                        <= 300 => $"{RichTextHelper.FromColorName("Blue")}FREEZING{RichTextHelper.FromColorName("Snow")} (150-300m)",
                        _ => $"{RichTextHelper.FromColorName("Violet")}NO LIFE DETECTED{RichTextHelper.FromColorName("Snow")} (300m+)"
                    };

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
        while (MiniGameState == "waiting" && Server.CurrentPlayerCount < RequiredPlayerCountToStart)
        {
            try
            {
                var message = new StringBuilder();
                var playersNeeded = RequiredPlayerCountToStart - Server.CurrentPlayerCount;
                message.AppendLine($"{RichTextHelper.Size(150)}BattleBit Hide and Seek!{RichTextHelper.Size(100)}");
                message.AppendLine("Waiting for players to join! Need " + playersNeeded + " more players to start!");
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
                var totalSeekers = HideAndSeekers.Values.Count(player => player.IsSeeking);
                var totalHiders =
                    HideAndSeekers.Values.Count(player => player is { IsSeeking: false, Player.IsAlive: true });
                foreach (var player in Server.AllPlayers)
                {
                    player.Modifications.CanSpectate = false;
                    var message = new StringBuilder();
                    var hideAndSeeker = GetHideAndSeekPlayerData(player);
                    message.AppendLine(
                        $"{RichTextHelper.Size(100)}{RichTextHelper.FromColorName("Snow")}BattleBit Hide and Seek!{RichTextHelper.Size(100)}");
                    switch (MiniGameState)
                    {
                        case "selectingseekers":
                            message.AppendLine(
                                $"{RichTextHelper.Size(140)}{RichTextHelper.FromColorName("Orange")}Randomly selecting seekers!{RichTextHelper.Size(100)}");
                            break;
                        case "countingdown":
                        {
                            // Tell hiders they need to hide
                            if (!hideAndSeeker.IsSeeking)
                            {
                                message.AppendLine(
                                    $"{RichTextHelper.Size(150)}{RichTextHelper.FromColorName("Orange")}You are currently " +
                                    (hideAndSeeker.IsSeeking ? "seeking" : "hiding") + RichTextHelper.Size(100) +
                                    RichTextHelper.FromColorName("Snow"));
                                message.AppendLine(
                                    $"{RichTextHelper.FromColorName("Orange")}Hide from the seekers!{RichTextHelper.NewLine()}You have 3 minutes and can't use vehicles!");
                            }
                            // Tell seekers they need to seek
                            else
                            {
                                message.AppendLine(
                                    $"{RichTextHelper.Size(150)}{RichTextHelper.FromColorName("Orange")}You are currently " +
                                    (hideAndSeeker.IsSeeking ? "seeking" : "hiding") + RichTextHelper.Size(100) +
                                    RichTextHelper.FromColorName("Snow"));
                                message.AppendLine(
                                    $"{RichTextHelper.FromColorName("Orange")}Once the countdown ends you have 3 minutes to find them!");
                            }

                            message.AppendLine(
                                $"{RichTextHelper.FromColorName("Snow")}Seekers: {totalSeekers} | Hiders: {totalHiders}");
                            break;
                        }
                        case "running":
                        {
                            message.AppendLine(
                                $"{RichTextHelper.Size(150)}{RichTextHelper.FromColorName("Orange")}You are currently " +
                                (hideAndSeeker.IsSeeking ? "seeking" : "hiding") + RichTextHelper.Size(100) +
                                RichTextHelper.FromColorName("Snow"));
                            message.AppendLine("Coords: " + hideAndSeeker.Player.Position.X + ", " +
                                               hideAndSeeker.Player.Position.Y + ", " +
                                               hideAndSeeker.Player.Position.Z);
                            message.AppendLine("Seekers: " + totalSeekers + " | " + "Hiders: " + totalHiders);
                            if (hideAndSeeker.IsSeeking)
                            {
                                message.AppendLine("Seeking Meter: " + hideAndSeeker.SeekingMeter);
                            }

                            break;
                        }
                        case "ending":
                            message.AppendLine(
                                $"{RichTextHelper.Size(140)}{RichTextHelper.FromColorName("Orange")}Game has ended!{RichTextHelper.Size(100)}");
                            break;
                        default:
                            message.AppendLine(
                                $"{RichTextHelper.Size(140)}{RichTextHelper.FromColorName("Orange")}Waiting for players to join!{RichTextHelper.Size(100)}");
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
    
    private HideAndSeekPlayerData GetHideAndSeekPlayerData(BattleBitApiPlayer player)
    {
        if (!HideAndSeekers.ContainsKey(player.SteamID))
        {
            HideAndSeekers.TryAdd(player.SteamID, new HideAndSeekPlayerData(player));
        }
        
        return HideAndSeekers[player.SteamID];
    }
    
    private async void StartHideAndSeek()
    {
        try
        {
            Program.Logger.Info("Hide and Seek gamemode started!");
        
            MiniGameState = "selectingseekers";
            
            // Clear all players from the hide and seek list
            HideAndSeekers.Clear();

            foreach (var player in Server.AllPlayers)
            {
                player.ChangeTeam(Team.TeamB);
                player.Kill();
            }
            
            var countdown = 30;
            while (countdown > 0)
            {
                try
                {
                    Server.AnnounceShort(
                        $"Randomly selecting seekers in {FormattingHelper.GetFormattedTimeFromSeconds(countdown)}!");
                    await Task.Delay(1000);
                    countdown--;

                    foreach (var player in Server.AllPlayers)
                    {
                        PlayerHelpers.KillPlayerInVehicle(player, "Vehicles are not allowed until the game starts!");
                    }

                    if (MiniGameState != "selectingseekers" || MiniGameState == "ending") return;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            // Add all players to the hide and seek list
            foreach (var player in Server.AllPlayers)
            {
                try
                {
                    HideAndSeekers.TryAdd(player.SteamID, new HideAndSeekPlayerData(player));
                    player.Modifications.CanDeploy = false;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            // Get 1/16th of the players to be seekers and a maximum of 8 and minimum of 1
            // Randomize the players
            var seekers = Server.AllPlayers.OrderBy(player => _random.Next()).Take(Math.Clamp(Server.CurrentPlayerCount / 16, 1, 8)).ToList();

            foreach (var seeker in seekers)
            {
                try
                {
                    Program.Logger.Info(seeker.Name);
                    var playerData = GetHideAndSeekPlayerData(seeker);
                    playerData.IsSeeking = true;
                    playerData.Player.Modifications.CanDeploy = false;
                    playerData.Player.Modifications.CanSpectate = false;
                    playerData.Player.ChangeTeam(Team.TeamA);
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            MiniGameState = "countingdown";
            
            // Get all other players to be the hiders
            var hiders = HideAndSeekers.Values.Where(player => !player.IsSeeking).ToList();
            foreach (var hider in hiders)
            {
                try
                {
                    hider.IsSeeking = false;
                    hider.Player.Modifications.CanDeploy = true;
                    hider.Player.Modifications.RunningSpeedMultiplier = HiderRunSpeedMultiplierDuringCountdown;
                    hider.Player.Modifications.JumpHeightMultiplier = HiderJumpHeightMultiplierDuringCountdown;
                    hider.Player.ChangeTeam(Team.TeamB);
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
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
                            var playerData = GetHideAndSeekPlayerData(player);
                            if (playerData.IsSeeking)
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
                    
                    if (MiniGameState != "countingdown" && MiniGameState != "ending") return;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            // Get all other players to be the hiders
            hiders = HideAndSeekers.Values.Where(player => !player.IsSeeking).ToList();
            foreach (var hider in hiders)
            {
                try
                {
                    hider.Player.Modifications.RunningSpeedMultiplier = HiderRunSpeedMultiplierDuringGame;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            Server.AnnounceShort($"Hide and Seek Started and seekers can now spawn in!");
            
            MiniGameState = "running";
            await Task.Run(StartPlayerSeekerMeter);
            
            // Get filter for all seekers
            var seekersFilter = HideAndSeekers.Values.Where(player => player.IsSeeking).Select(player => player.Player).ToList();
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
            
            // Get filter for all hiders not alive
            var hidersFilter = HideAndSeekers.Values.Where(player => !player.IsSeeking && !player.Player.IsAlive).Select(player => player.Player).ToList();
            foreach (var hider in hidersFilter)
            {
                try
                {
                    var hiderData = GetHideAndSeekPlayerData(hider);

                    hiderData.IsSeeking = true;
                    hider.Modifications.CanDeploy = false;
                    hider.Modifications.CanSpectate = false;
                    hider.Kill();
                    hider.ChangeTeam(Team.TeamA);
                    hider.Modifications.CanDeploy = true;
                }
                catch (Exception e)
                {
                    Program.Logger.Error(e.Message);
                }
            }
            
            hidersCount = HideAndSeekers.Values.Count(player => player.IsSeeking == false);
            if (hidersCount == 0)
            {
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
        if (MiniGameState == "running" || MiniGameState == "countingdown")
        {
            var playerData = GetHideAndSeekPlayerData(player);
            player.ChangeTeam(Team.TeamA);
            playerData.IsSeeking = true;
        
            var totalHiders = HideAndSeekers.Values.Count(player => player is { IsSeeking: false });
            if (totalHiders == 0 && MiniGameState == "running")
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

    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitApiPlayer player, OnPlayerSpawnArguments request)
    {
        player.Modifications.RespawnTime = 0.0f;
        player.Modifications.CaptureFlagSpeedMultiplier = 0.0f;
        
        var playerData = GetHideAndSeekPlayerData(player);
        
        if (playerData.IsSeeking)
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
        await Task.Delay(1000);
        
        var hideAndSeeker = GetHideAndSeekPlayerData(player);
        
        if (player.InVehicle && !hideAndSeeker.IsSeeking)
        {
            player.Kill();
            player.SayToChat($"You are not allowed to use vehicles in this gamemode!");
        }
    }

    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitApiPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        
        if (killer == null) return Task.CompletedTask;
        
        var killerData = GetHideAndSeekPlayerData(killer);
        var victimData = GetHideAndSeekPlayerData(victim);
        
        if (killerData.IsSeeking && !victimData.IsSeeking)
        {
            killerData.HidersFound++;
            victim.ChangeTeam(Team.TeamA);
            victimData.IsSeeking = true;
            Server.SayToAllChat($"{RichTextHelper.Bold(true)}{killer.Name}{RichTextHelper.Bold(false)} found {RichTextHelper.Bold(true)}{victim.Name}{RichTextHelper.Bold(false)}! They've found {killerData.HidersFound} hiders!");
        }
        
        // If all hiders are found, end the game
        if (HideAndSeekers.Values.Count(player => player is { IsSeeking: false, Player.IsAlive: true }) == 0)
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
        // Remove player from the hide and seek list
        HideAndSeekers.TryRemove(player.SteamID, out _);
        
        var totalHiders = HideAndSeekers.Values.Count(player => player is { IsSeeking: false });
        if (totalHiders == 0 && MiniGameState == "running")
        {
            Server.AnnounceLong("All hiders have been found! Seekers win!");
            Server.ForceEndGame(Team.TeamA);
        }
        
        return Task.CompletedTask;
    }

    public override async Task OnPlayerConnected(BattleBitApiPlayer player)
    {
        var playerData = GetHideAndSeekPlayerData(player);
        
        await Task.Delay(1000);
        player.Modifications.CanSpectate = false;
        player.Modifications.CaptureFlagSpeedMultiplier = 0.0f;
        
        if (MiniGameState == "running" || MiniGameState == "countingdown")
        {
            playerData.IsSeeking = true;
            player.ChangeTeam(Team.TeamA);
            if (MiniGameState == "countingdown")
            {
                playerData.Player.Modifications.CanDeploy = false;
            }
            else
            {
                playerData.Player.Modifications.CanDeploy = true;
            }
        } else {
            playerData.IsSeeking = false;
            playerData.Player.Modifications.CanDeploy = false;
            player.ChangeTeam(Team.TeamB);
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
                MiniGameState = "waiting";
                Server.RoundSettings.PlayersToStart = 1;
                break;
            case GameState.EndingGame:
                MiniGameState = "ending";
                Server.RoundSettings.SecondsLeft = 0;
                break;
        }

        return Task.CompletedTask;
    }
}

public class HideAndSeekPlayerData
{
    public HideAndSeekPlayerData(BattleBitApiPlayer player)
    {
        Player = player;
    }
    
    public BattleBitApiPlayer Player { get; set; }
    public bool IsSeeking { get; set; }
    public string SeekingMeter { get; set; } = "FREEZING";
    public int HidersFound { get; set; }
}