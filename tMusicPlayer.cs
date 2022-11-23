using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tMusicPlayer
{
	public class tMusicPlayer : Mod {
		internal static List<MusicData> AllMusic;
		public static Dictionary<int, int> itemToMusicReference;

		public static TMPConfig tMPConfig;

		internal static ModKeybind HidePlayerHotkey;
		internal static ModKeybind PlayStopHotkey;
		internal static ModKeybind PrevSongHotkey;
		internal static ModKeybind NextSongHotkey;
		
		public override void Load() {
			// Setup hotkeys and the configs instance.
			tMPConfig = ModContent.GetInstance<TMPConfig>();
			HidePlayerHotkey = KeybindLoader.RegisterKeybind(this, "Hide Music Player", "Up");
			PlayStopHotkey = KeybindLoader.RegisterKeybind(this, "Play/Stop Music", "Down");
			PrevSongHotkey = KeybindLoader.RegisterKeybind(this, "Previous Song", "Left");
			NextSongHotkey = KeybindLoader.RegisterKeybind(this, "Next Song", "Right");

			// A list that contains all the MusicData we need, including mod and name.
			AllMusic = new List<MusicData> {
				// 1.3
				new MusicData(MusicID.OverworldDay, ItemID.MusicBoxOverworldDay, 0),
				new MusicData(MusicID.Eerie, ItemID.MusicBoxEerie, 1),
				new MusicData(MusicID.Night, ItemID.MusicBoxNight, 2),
				new MusicData(MusicID.Title, ItemID.MusicBoxTitle, 3),
				new MusicData(MusicID.Underground, ItemID.MusicBoxUnderground, 4),
				new MusicData(MusicID.Boss1, ItemID.MusicBoxBoss1, 5),
				new MusicData(MusicID.Jungle, ItemID.MusicBoxJungle, 6),
				new MusicData(MusicID.Corruption, ItemID.MusicBoxCorruption, 7),
				new MusicData(MusicID.UndergroundCorruption, ItemID.MusicBoxUndergroundCorruption, 8),
				new MusicData(MusicID.TheHallow, ItemID.MusicBoxTheHallow, 9),
				new MusicData(MusicID.Boss2, ItemID.MusicBoxBoss2, 10),
				new MusicData(MusicID.UndergroundHallow, ItemID.MusicBoxUndergroundHallow, 11),
				new MusicData(MusicID.Boss3, ItemID.MusicBoxBoss3, 12),
				new MusicData(MusicID.Snow, ItemID.MusicBoxSnow, 13),
				new MusicData(MusicID.Space, ItemID.MusicBoxSpace, 14),
				new MusicData(MusicID.Crimson, ItemID.MusicBoxCrimson, 15),
				new MusicData(MusicID.Boss4, ItemID.MusicBoxBoss4, 16),
				new MusicData(MusicID.AltOverworldDay, ItemID.MusicBoxAltOverworldDay, 17),
				new MusicData(MusicID.Rain, ItemID.MusicBoxRain, 18),
				new MusicData(MusicID.Ice, ItemID.MusicBoxIce, 19),
				new MusicData(MusicID.Desert, ItemID.MusicBoxDesert, 20),
				new MusicData(MusicID.Ocean, ItemID.MusicBoxOcean, 21),
				new MusicData(MusicID.Dungeon, ItemID.MusicBoxDungeon, 22),
				new MusicData(MusicID.Plantera, ItemID.MusicBoxPlantera, 23),
				new MusicData(MusicID.Boss5, ItemID.MusicBoxBoss5, 24),
				new MusicData(MusicID.Temple, ItemID.MusicBoxTemple, 25),
				new MusicData(MusicID.Eclipse, ItemID.MusicBoxEclipse, 26),
				new MusicData(MusicID.Mushrooms, ItemID.MusicBoxMushrooms, 27),
				new MusicData(MusicID.PumpkinMoon, ItemID.MusicBoxPumpkinMoon, 28),
				new MusicData(MusicID.AltUnderground, ItemID.MusicBoxAltUnderground, 29),
				new MusicData(MusicID.FrostMoon, ItemID.MusicBoxFrostMoon, 30),
				new MusicData(MusicID.UndergroundCrimson, ItemID.MusicBoxUndergroundCrimson, 31),
				new MusicData(MusicID.LunarBoss, ItemID.MusicBoxLunarBoss, 32),
				new MusicData(MusicID.MartianMadness, ItemID.MusicBoxMartians, 33),
				new MusicData(MusicID.PirateInvasion, ItemID.MusicBoxPirates, 34),
				new MusicData(MusicID.Hell, ItemID.MusicBoxHell, 35),
				new MusicData(MusicID.TheTowers, ItemID.MusicBoxTowers, 36),
				new MusicData(MusicID.GoblinInvasion, ItemID.MusicBoxGoblins, 37),
				new MusicData(MusicID.Sandstorm, ItemID.MusicBoxSandstorm, 38),
				new MusicData(MusicID.OldOnesArmy, ItemID.MusicBoxDD2, 39),

				// 1.4
				new MusicData(MusicID.WindyDay, ItemID.MusicBoxWindyDay, 40),
				new MusicData(MusicID.SlimeRain, ItemID.MusicBoxSlimeRain, 41),
				new MusicData(MusicID.SpaceDay, ItemID.MusicBoxSpaceAlt, 42),
				new MusicData(MusicID.OceanNight, ItemID.MusicBoxOceanAlt, 43),
				new MusicData(MusicID.TownDay, ItemID.MusicBoxTownDay, 44),
				new MusicData(MusicID.TownNight, ItemID.MusicBoxTownNight, 45),
				new MusicData(MusicID.DayRemix, ItemID.MusicBoxDayRemix, 46),
				new MusicData(MusicID.MenuMusic, ItemID.MusicBoxTitleAlt, 47),
				new MusicData(MusicID.Monsoon, ItemID.MusicBoxStorm, 48),
				new MusicData(MusicID.Graveyard, ItemID.MusicBoxGraveyard, 49),
				new MusicData(MusicID.JungleUnderground, ItemID.MusicBoxUndergroundJungle, 50),
				new MusicData(MusicID.JungleNight, ItemID.MusicBoxJungleNight, 51),
				new MusicData(MusicID.QueenSlime, ItemID.MusicBoxQueenSlime, 52),
				new MusicData(MusicID.EmpressOfLight, ItemID.MusicBoxEmpressOfLight, 53),
				new MusicData(MusicID.DukeFishron, ItemID.MusicBoxDukeFishron, 54),
				new MusicData(MusicID.MorningRain, ItemID.MusicBoxMorningRain, 55),
				new MusicData(MusicID.ConsoleMenu, ItemID.MusicBoxConsoleTitle, 56),
				new MusicData(MusicID.UndergroundDesert, ItemID.MusicBoxUndergroundDesert, 57),
				new MusicData(MusicID.Credits, ItemID.MusicBoxCredits, 85),
				new MusicData(MusicID.Deerclops, ItemID.MusicBoxDeerclops, 86), // Deerclops doesn't have an ID?

				// Otherworld
				new MusicData(MusicID.OtherworldlyRain, ItemID.MusicBoxOWRain, 58),
				new MusicData(MusicID.OtherworldlyDay, ItemID.MusicBoxOWDay, 59),
				new MusicData(MusicID.OtherworldlyNight, ItemID.MusicBoxOWNight, 60),
				new MusicData(MusicID.OtherworldlyUnderground, ItemID.MusicBoxOWUnderground, 61),
				new MusicData(MusicID.OtherworldlyDesert, ItemID.MusicBoxOWDesert, 62),
				new MusicData(MusicID.OtherworldlyOcean, ItemID.MusicBoxOWOcean, 63),
				new MusicData(MusicID.OtherworldlyMushrooms, ItemID.MusicBoxOWMushroom, 64),
				new MusicData(MusicID.OtherworldlyDungeon, ItemID.MusicBoxOWDungeon, 65),
				new MusicData(MusicID.OtherworldlySpace, ItemID.MusicBoxOWSpace, 66),
				new MusicData(MusicID.OtherworldlyUnderworld, ItemID.MusicBoxOWUnderworld, 67),
				new MusicData(MusicID.OtherworldlySnow, ItemID.MusicBoxOWSnow, 68),
				new MusicData(MusicID.OtherworldlyCorruption, ItemID.MusicBoxOWCorruption, 69),
				new MusicData(MusicID.OtherworldlyUGCorrption, ItemID.MusicBoxOWUndergroundCorruption, 70),
				new MusicData(MusicID.OtherworldlyCrimson, ItemID.MusicBoxOWCrimson, 71),
				new MusicData(MusicID.OtherworldlyUGCrimson, ItemID.MusicBoxOWUndergroundCrimson, 72),
				new MusicData(MusicID.OtherworldlyIce, ItemID.MusicBoxOWUndergroundSnow, 73),
				new MusicData(MusicID.OtherworldlyUGHallow, ItemID.MusicBoxOWUndergroundHallow, 74),
				new MusicData(MusicID.OtherworldlyEerie, ItemID.MusicBoxOWBloodMoon, 75),
				new MusicData(MusicID.OtherworldlyBoss2, ItemID.MusicBoxOWBoss2, 76),
				new MusicData(MusicID.OtherworldlyBoss1, ItemID.MusicBoxOWBoss1, 77),
				new MusicData(MusicID.OtherworldlyInvasion, ItemID.MusicBoxOWInvasion, 78),
				new MusicData(MusicID.OtherworldlyTowers, ItemID.MusicBoxOWTowers, 79),
				new MusicData(MusicID.OtherworldlyLunarBoss, ItemID.MusicBoxOWMoonLord, 80),
				new MusicData(MusicID.OtherworldlyPlantera, ItemID.MusicBoxOWPlantera, 81),
				new MusicData(MusicID.OtherworldlyJungle, ItemID.MusicBoxOWJungle, 82),
				new MusicData(MusicID.OtherworldlyWoF, ItemID.MusicBoxOWWallOfFlesh, 83),
				new MusicData(MusicID.OtherworldlyHallow, ItemID.MusicBoxOWHallow, 84)
			};

			// Code provided by Jopojelly. Thank you, Jopo!
			// This grabs the entire dictionary of MODDED music-to-musicbox correlations.
			FieldInfo field = typeof(MusicLoader).GetField("itemToMusic", BindingFlags.Static | BindingFlags.NonPublic);
			itemToMusicReference = (Dictionary<int, int>)field.GetValue(null);
		}

		public override void Unload() {
			AllMusic = null;
			itemToMusicReference = null;

			tMPConfig = null;

			HidePlayerHotkey = null;
			PlayStopHotkey = null;
			PrevSongHotkey = null;
			NextSongHotkey = null;
		}

		public static void SendDebugText(string message, Color color = default) {
			if (tMPConfig.EnableDebugMode) {
				Main.NewText(message, color);
			}
		}
	}
}
