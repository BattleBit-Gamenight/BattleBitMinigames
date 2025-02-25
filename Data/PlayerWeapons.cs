﻿using BattleBitAPI.Common;

namespace BattleBitMinigames.Data;

public static class PlayerWeapons
{
    public static List<Weapon> WeaponList = new List<Weapon>()
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
    public static List<Attachment> SideRailList = new List<Attachment>()
    {
        Attachments.Flashlight,
        Attachments.Rangefinder,
        Attachments.Redlaser,
        Attachments.TacticalFlashlight,
        Attachments.Greenlaser,
        Attachments.Searchlight
    };
    public static List<Attachment> BarrelList = new List<Attachment>()
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
    public static List<Attachment> SightList = new List<Attachment>()
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
    public static List<Attachment> UnderRailList = new List<Attachment>()
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
    public static List<Gadget> GadgetList = new List<Gadget>()
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
    public static List<Gadget> ThrowableList = new List<Gadget>()
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