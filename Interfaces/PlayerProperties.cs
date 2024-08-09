namespace BattleBitMinigames.Interfaces;

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

    /// <summary>
    /// Infected player properties
    /// </summary>
    public interface IInfectedPlayerProperties
    {
        public const string IsInfected = "infected_is_infected";
        public const string ZombieType = "infected_zombie_type";
        public const string KillsAsInfected = "infected_kills_as_infected";
        public const string KillsAsHuman = "infected_kills_as_human";
    }
    
    /// <summary>
    /// VIP player properties
    /// </summary>
    public interface IVipPlayerProperties
    {
        public const string IsVip = "player_is_vip";
        public const string SpawnedInSpawn = "spawned_in_spawn";
        public const string SpawnedInSpawnTime = "spawned_in_spawn_time";
        public const string EnteredSpawnTime = "entered_spawn_time";
    }
}