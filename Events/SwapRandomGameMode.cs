using System.Numerics;
using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class SwapRandomGamemode : Event
{
    Random rnd = new Random();
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        var checks = 0;

        if (killer == null || killer == victim || !killer.IsAlive)
            return;

        var newPosition = victim.Position + new Vector3(0, 1.5f, 0);

        killer.Teleport(newPosition);
        await Task.Delay(10);
        SetPlayerLoadout(killer, victim.CurrentLoadout);

        while (Vector3.Distance(killer.Position, newPosition) > 3 && checks < 15)
        {
            checks++;
            killer.Teleport(newPosition);
            await Task.Delay(10);
        }
    }

    private static void SetPlayerLoadout(BattleBitPlayer player, PlayerLoadout loadout)
    {
        player.SetFirstAidGadget(loadout.FirstAidName, loadout.FirstAidExtra + 1);
        player.SetLightGadget(loadout.LightGadgetName, loadout.LightGadgetExtra + 1);
        player.SetHeavyGadget(loadout.HeavyGadgetName, loadout.HeavyGadgetExtra + 1);
        player.SetThrowable(loadout.ThrowableName, loadout.ThrowableExtra + 1);
        player.SetSecondaryWeapon(loadout.SecondaryWeapon, loadout.SecondaryExtraMagazines + 1);
        Task.Delay(100).Wait();
        player.SetPrimaryWeapon(loadout.PrimaryWeapon, loadout.PrimaryExtraMagazines + 1);
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                Server.RoundSettings.SecondsLeft = 100000;
                Server.RoundSettings.TeamATickets = 450;
                Server.RoundSettings.TeamBTickets = 450;
                break;
            case GameState.CountingDown:
                Server.RoundSettings.SecondsLeft = 10;
                break;
            case GameState.WaitingForPlayers:
            case GameState.EndingGame:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        return Task.CompletedTask;
    }

    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        player.Modifications.CanSpectate = false;
        player.Modifications.DownTimeGiveUpTime = 0f;

        return base.OnPlayerConnected(player);
    }
    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        try
        {
            request.Loadout.PrimaryWeapon = new WeaponItem()
            {
                Tool = WeaponList.ElementAt(rnd.Next(0, WeaponList.Count())),
                Barrel = BarrelList.ElementAt(rnd.Next(0, BarrelList.Count())),
                MainSight = SightList.ElementAt(rnd.Next(0, SightList.Count())),
                UnderRail = UnderRailList.ElementAt(rnd.Next(0, UnderRailList.Count())),
                BoltAction = Attachments.BoltActionE,
                CamoIndex = (ushort)rnd.Next(1, 1000),
                AttachmentsCamoIndex = (ushort)rnd.Next(1, 1000),
                UVIndex = (byte)rnd.Next(2, 4),
                AttachmentsUVIndex = (byte)rnd.Next(2, 4)
            };

            request.Loadout.SecondaryWeapon = new WeaponItem()
            {
                Tool = WeaponList.FindAll(weapon => weapon != request.Loadout.PrimaryWeapon.Tool).ElementAt(rnd.Next(0, WeaponList.Count() - 1)),
                Barrel = BarrelList.ElementAt(rnd.Next(0, BarrelList.Count())),
                MainSight = SightList.ElementAt(rnd.Next(0, SightList.Count())),
                UnderRail = UnderRailList.ElementAt(rnd.Next(0, UnderRailList.Count())),
                BoltAction = Attachments.BoltActionE,
                CamoIndex = (ushort)rnd.Next(1, 1000),
                AttachmentsCamoIndex = (ushort)rnd.Next(1, 1000),
                UVIndex = (byte)rnd.Next(2, 4),
                AttachmentsUVIndex = (byte)rnd.Next(2, 4)
            };
            

            request.Loadout.LightGadget = GadgetList.ElementAt(rnd.Next(0, GadgetList.Count()));
            if (request.Loadout.LightGadget.Name.Contains("Rpg"))
                request.Loadout.HeavyGadget = GadgetList.FindAll(gadget => !gadget.Name.Contains("Rpg")).ElementAt(rnd.Next(0, GadgetList.Count() - 5));
            else if (request.Loadout.LightGadget.Name.Contains("Sledge") || request.Loadout.LightGadget.Name.Contains("Pickaxe"))
                request.Loadout.HeavyGadget = GadgetList.FindAll(gadget => !(gadget.Name.Contains("Sledge") || gadget.Name.Contains("Pickaxe"))).ElementAt(rnd.Next(0, GadgetList.Count() - 6));
            else
                request.Loadout.HeavyGadget = GadgetList.FindAll(gadget => gadget != request.Loadout.LightGadget).ElementAt(rnd.Next(0, GadgetList.Count() - 1));
            request.Loadout.Throwable = ThrowableList.ElementAt(rnd.Next(0, ThrowableList.Count()));

            if (player.PingMs > 150)//this didn't work, need to figure out why
            {
                player.Modifications.RunningSpeedMultiplier = (float)rnd.Next(100, 125) / 100;
                player.Modifications.JumpHeightMultiplier = (float)rnd.Next(100, 150) / 100;
                player.Modifications.ReloadSpeedMultiplier = (float)rnd.Next(100, 300) / 100;
                player.Modifications.FallDamageMultiplier = 0;
            }
            player.Modifications.RunningSpeedMultiplier = (float)rnd.Next(95, 300) / 100;
            player.Modifications.JumpHeightMultiplier = (float)rnd.Next(80, 300) / 100;
            player.Modifications.ReloadSpeedMultiplier = (float)rnd.Next(100, 300) / 100;
            player.Modifications.FallDamageMultiplier = (float)rnd.Next(0, 10) / 100;


            player.Modifications.KillFeed = true;
            player.Modifications.RespawnTime = 0;
            player.Modifications.CanSuicide = false;
            return request;
        }
        catch (Exception e)
        {
            Console.Out.WriteLine($"Error occured on {player.Name} spawning: {e}");
            return null;
        }
    }
    public List<Weapon> WeaponList = new List<Weapon>()
    {
        Weapons.ACR,
        Weapons.AK15,
        Weapons.AK74,
        Weapons.G36C,
        Weapons.HoneyBadger,
        Weapons.KrissVector,
        Weapons.L86A1,
        Weapons.L96,
        Weapons.M4A1,
        Weapons.M9,
        Weapons.M110,
        Weapons.M249,
        Weapons.MK14EBR,
        Weapons.MK20,
        Weapons.MP7,
        Weapons.PP2000,
        Weapons.SCARH,
        Weapons.SSG69,
        Weapons.SV98,
        Weapons.UMP45,
        Weapons.Unica,
        Weapons.USP,
        Weapons.AsVal,
        Weapons.AUGA3,
        Weapons.DesertEagle,
        Weapons.FAL,
        Weapons.Glock18,
        Weapons.M200,
        Weapons.MP443,
        Weapons.FAMAS,
        Weapons.MP5,
        Weapons.P90,
        Weapons.MSR,
        Weapons.PP19,
        Weapons.SVD,
        Weapons.Rem700,
        Weapons.SG550,
        Weapons.Groza,
        Weapons.HK419,
        Weapons.ScorpionEVO,
        Weapons.Rsh12,
        Weapons.MG36,
        Weapons.AK5C,
        Weapons.Ultimax100,
        new Weapon("G3", WeaponType.Rifle),
        new Weapon("F2000", WeaponType.Rifle)
    };
    public List<Attachment> SideRailList = new List<Attachment>()
    {
        Attachments.Flashlight,
        Attachments.Rangefinder,
        Attachments.Redlaser,
        Attachments.TacticalFlashlight,
        Attachments.Greenlaser,
        Attachments.Searchlight
    };
    public List<Attachment> BarrelList = new List<Attachment>()
    {
        Attachments.Basic,
        Attachments.Compensator,
        Attachments.Heavy,
        Attachments.LongBarrel,
        Attachments.MuzzleBreak,
        Attachments.Ranger,
        Attachments.SuppressorLong,
        Attachments.SuppressorShort,
        Attachments.Tactical,
        Attachments.FlashHider,
        Attachments.Osprey9,
        Attachments.DGN308,
        Attachments.VAMB762,
        Attachments.SDN6762,
        Attachments.NT4556
    };
    public List<Attachment> SightList = new List<Attachment>()
    {
        Attachments._6xScope,
        Attachments._8xScope,
        Attachments._15xScope,
        Attachments._20xScope,
        Attachments.PTR40Hunter,
        Attachments._1P78,
        Attachments.Acog,
        Attachments.M125,
        Attachments.Prisma,
        Attachments.Slip,
        Attachments.PistolDeltaSight,
        Attachments.PistolRedDot,
        Attachments.AimComp,
        Attachments.Holographic,
        Attachments.Kobra,
        Attachments.OKP7,
        Attachments.PKAS,
        Attachments.RedDot,
        Attachments.Reflex,
        Attachments.Strikefire,
        Attachments.Razor,
        Attachments.Flir,
        Attachments.Echo,
        Attachments.TRI4X32,
        Attachments.FYouSight,
        Attachments.HoloPK120,
        Attachments.Pistol8xScope,
        Attachments.BurrisAR332,
        Attachments.HS401G5,
        new Attachment("F2000_Sight", AttachmentType.MainSight)
    };
    public List<Attachment> UnderRailList = new List<Attachment>()
    {
        Attachments.AngledGrip,
        Attachments.Bipod,
        Attachments.VerticalGrip,
        Attachments.StubbyGrip,
        Attachments.StabilGrip,
        Attachments.VerticalSkeletonGrip,
        Attachments.FABDTFG,
        Attachments.MagpulAngled,
        Attachments.BCMGunFighter,
        Attachments.ShiftShortAngledGrip,
        Attachments.SE5Grip,
        Attachments.RK6Foregrip,
        Attachments.HeraCQRFront,
        Attachments.B25URK,
        Attachments.VTACUVGTacticalGrip
    };
    public List<Gadget> GadgetList = new List<Gadget>()
    {
        Gadgets.Bandage,
        Gadgets.Binoculars,
        Gadgets.RepairTool,
        Gadgets.C4,
        Gadgets.Claymore,
        Gadgets.M320SmokeGrenadeLauncher,
        Gadgets.SmallAmmoKit,
        Gadgets.AntiPersonnelMine,
        Gadgets.AntiVehicleMine,
        Gadgets.MedicKit,
        Gadgets.Rpg7HeatExplosive,
        Gadgets.RiotShield,
        Gadgets.SledgeHammer,
        Gadgets.AdvancedBinoculars,
        Gadgets.Mdx201,
        Gadgets.BinoSoflam,
        Gadgets.HeavyAmmoKit,
        Gadgets.Rpg7Pgo7Tandem,
        Gadgets.Rpg7Pgo7HeatExplosive,
        Gadgets.Rpg7Pgo7Fragmentation,
        Gadgets.Rpg7Fragmentation,
        Gadgets.GrapplingHook,
        Gadgets.AirDrone,
        Gadgets.Pickaxe,
        Gadgets.SuicideC4,
        Gadgets.SledgeHammerSkinA,
        Gadgets.SledgeHammerSkinB,
        Gadgets.SledgeHammerSkinC,
        Gadgets.PickaxeIronPickaxe
    };
    public List<Gadget> ThrowableList = new List<Gadget>()
    {
        Gadgets.FragGrenade,
        Gadgets.ImpactGrenade,
        Gadgets.AntiVehicleGrenade,
        Gadgets.SmokeGrenadeBlue,
        Gadgets.SmokeGrenadeGreen,
        Gadgets.SmokeGrenadeRed,
        Gadgets.SmokeGrenadeWhite,
        Gadgets.Flare,
        Gadgets.Flashbang
    };
}