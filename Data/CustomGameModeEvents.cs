﻿using BattleBitMinigames.Api;
using BattleBitMinigames.Events;

namespace BattleBitMinigames.Data;

public static class CustomGameModeEvents
{
    public static readonly List<Event> ZombieEvents = new ()
    {
        new ZombiesGamemode(),
        // new RegionManager()
    };
    
    
    public static readonly List<Event> VipEvents = new()
    {
        new VipGamemode(),
        new RegionManager()
    };
    
    public static readonly List<Event> HideAndSeekEvents = new()
    {
        new HideAndSeekGamemode(),
        new RegionManager()
    };

    public static readonly List<Event> GunGameEvents = new()
    {
        new GunGameGamemode()
    };

    public static readonly List<Event> SwapEvents = new()
    {
        new SwapGamemode()
    };

    public static readonly List<Event> SwapRandomEvents = new()
    {
        new SwapRandomGamemode()
    };
}