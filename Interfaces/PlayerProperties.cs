﻿namespace BattleBitMinigames.Interfaces;

public interface PlayerProperties
{
    /// <summary>
    /// Hide and Seek player properties
    /// </summary>
    public interface IHideAndSeekPlayerProperties
    {
        public const string IsSeeking = "hide_and_seek_is_seeking";
        public const string HidersFound = "hide_and_seek_hiders_found";
        public const string SeekingMeter = "hide_and_seek_seeking_meter";
    }
    
    /// <summary>
    /// Gun Game player properties
    /// </summary>
    public interface IGunGamePlayerProperties
    {
        public const string Kills = "gungame_kills";
        public const string Deaths = "gungame_deaths";
        public const string Tier = "gungame_tier";
        public const string KillsToNextTier = "gungame_kills_to_next_tier";
    }
}