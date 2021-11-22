using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace tMusicPlayer
{
	public class tMusicPlayer : Mod
	{
		internal static List<MusicData> AllMusic;
		public static Dictionary<int, int> itemToMusicReference;

		public static TMPConfig tMPConfig;
		public static TMPServerConfig tMPServerConfig;

		internal static ModKeybind HidePlayerHotkey;
		internal static ModKeybind PlayStopHotkey;
		internal static ModKeybind PrevSongHotkey;
		internal static ModKeybind NextSongHotkey;
		
		public override void Load()
		{
			// Setup hotkeys and the configs instance.
			tMPConfig = ModContent.GetInstance<TMPConfig>();
			tMPServerConfig = ModContent.GetInstance<TMPServerConfig>();
			HidePlayerHotkey = KeybindLoader.RegisterKeybind(this, "Hide Music Player", "Up");
			PlayStopHotkey = KeybindLoader.RegisterKeybind(this, "Play/Stop Music", "Down");
			PrevSongHotkey = KeybindLoader.RegisterKeybind(this, "Previous Song", "Left");
			NextSongHotkey = KeybindLoader.RegisterKeybind(this, "Next Song", "Right");

			// A list that contains all the MusicData we need, including mod and name.
			// TODO: [1.4] New boxes need to be added

			AllMusic = new List<MusicData> {
				new MusicData(MusicID.OverworldDay, ItemID.MusicBoxOverworldDay, 0),
				new MusicData(MusicID.AltOverworldDay, ItemID.MusicBoxAltOverworldDay, 17),
				new MusicData(MusicID.Night, ItemID.MusicBoxNight, 2),
				new MusicData(MusicID.Rain, ItemID.MusicBoxRain, 18),
				new MusicData(MusicID.Snow, ItemID.MusicBoxSnow, 13),
				new MusicData(MusicID.Ice, ItemID.MusicBoxIce, 19),
				new MusicData(MusicID.Desert, ItemID.MusicBoxDesert, 20),
				new MusicData(MusicID.Ocean, ItemID.MusicBoxOcean, 21),
				new MusicData(MusicID.Space, ItemID.MusicBoxSpace, 14),
				new MusicData(MusicID.Underground, ItemID.MusicBoxUnderground, 4),
				new MusicData(MusicID.AltUnderground, ItemID.MusicBoxAltUnderground, 29),
				new MusicData(MusicID.Mushrooms, ItemID.MusicBoxMushrooms, 27),
				new MusicData(MusicID.Jungle, ItemID.MusicBoxJungle, 6),
				new MusicData(MusicID.Corruption, ItemID.MusicBoxCorruption, 7),
				new MusicData(MusicID.UndergroundCorruption, ItemID.MusicBoxUndergroundCorruption, 8),
				new MusicData(MusicID.Crimson, ItemID.MusicBoxCrimson, 15),
				new MusicData(MusicID.UndergroundCrimson, ItemID.MusicBoxUndergroundCrimson, 31),
				new MusicData(MusicID.TheHallow, ItemID.MusicBoxTheHallow, 9),
				new MusicData(MusicID.UndergroundHallow, ItemID.MusicBoxUndergroundHallow, 11),
				new MusicData(MusicID.Hell, ItemID.MusicBoxHell, 35),
				new MusicData(MusicID.Dungeon, ItemID.MusicBoxDungeon, 22),
				new MusicData(MusicID.Temple, ItemID.MusicBoxTemple, 25),
				new MusicData(MusicID.Boss1, ItemID.MusicBoxBoss1, 5),
				new MusicData(MusicID.Boss2, ItemID.MusicBoxBoss2, 10),
				new MusicData(MusicID.Boss3, ItemID.MusicBoxBoss3, 12),
				new MusicData(MusicID.Boss4, ItemID.MusicBoxBoss4, 16),
				new MusicData(MusicID.Boss5, ItemID.MusicBoxBoss5, 24),
				new MusicData(MusicID.Plantera, ItemID.MusicBoxPlantera, 23),
				new MusicData(MusicID.Eerie, ItemID.MusicBoxEerie, 1),
				new MusicData(MusicID.Eclipse, ItemID.MusicBoxEclipse, 26),
				new MusicData(MusicID.GoblinInvasion, ItemID.MusicBoxGoblins, 37),
				new MusicData(MusicID.PirateInvasion, ItemID.MusicBoxPirates, 34),
				new MusicData(MusicID.MartianMadness, ItemID.MusicBoxMartians, 33),
				new MusicData(MusicID.PumpkinMoon, ItemID.MusicBoxPumpkinMoon, 28),
				new MusicData(MusicID.FrostMoon, ItemID.MusicBoxFrostMoon, 30),
				new MusicData(MusicID.TheTowers, ItemID.MusicBoxTowers, 36),
				new MusicData(MusicID.LunarBoss, ItemID.MusicBoxLunarBoss, 32),
				new MusicData(MusicID.Sandstorm, ItemID.MusicBoxSandstorm, 38),
				new MusicData(MusicID.OldOnesArmy, ItemID.MusicBoxDD2, 39),
				new MusicData(MusicID.Title, ItemID.MusicBoxTitle, 3)
			};

			// Code provided by Jopojelly. Thank you, Jopo!
			// This grabs the entire dictionary of MODDED music-to-musicbox correlations.
			FieldInfo field = typeof(MusicLoader).GetField("itemToMusic", BindingFlags.Static | BindingFlags.NonPublic);
			itemToMusicReference = (Dictionary<int, int>)field.GetValue(null);
		}

		public override void Unload()
		{
			tMPConfig = null;
			HidePlayerHotkey = null;
			PlayStopHotkey = null;
			PrevSongHotkey = null;
			NextSongHotkey = null;
			AllMusic = null;
			itemToMusicReference = null;
		}

		public override void PostAddRecipes()
		{
			// After all PostSetupContent has occured, setup all the MusicData.
			// Go through each key in the Modded MusicBox dictionary and attempt to add them to MusicData.
			Item item = new Item();
			foreach (int itemID in itemToMusicReference.Keys) {
				item.SetDefaults(itemID);
				string displayName = item.ModItem.Mod.DisplayName;
				string name = item.Name.Contains("(") ? item.Name.Substring(item.Name.IndexOf("(") + 1).Replace(")", "") : item.Name;
				int musicID = itemToMusicReference.TryGetValue(itemID, out int num) ? num : (-1);
				if (musicID != -1) {
					AllMusic.Add(new MusicData(musicID, itemID, displayName, name));
				}
			}
			item.TurnToAir();

			MusicPlayerUI UI = MusicUISystem.MusicUI;

			if (UI.sortType == SortBy.ID) {
				AllMusic = AllMusic.OrderBy(x => x.music).ToList();
			}
			if (UI.sortType == SortBy.Name) {
				AllMusic = AllMusic.OrderBy(x => x.name).ToList();
			}

			// Setup UI's item slot count.
			if (!Main.dedServ) {
				UI.SelectionSlots = new MusicBoxSlot[AllMusic.Count];
				UI.musicData = new List<MusicData>(AllMusic);
				UI.OrganizeSelection(SortBy.ID, ProgressBy.None, "", true);

				// Setup the mod list for the Mod Filter
				// Must occur after all other modded music is established
				UI.ModList = new List<string>();
				foreach (MusicData box in UI.musicData) {
					if (!UI.ModList.Contains(box.mod)) {
						UI.ModList.Add(box.mod);
					}
				}
				UI.ModList.Sort();
				UI.ModList.Remove("Terraria");
				UI.ModList.Insert(0, "Terraria"); // Put Terraria infront of all mods
			}
		}

		public static void SendDebugText(string message, Color color = default)
		{
			if (tMPConfig.EnableDebugMode) {
				Main.NewText(message, color);
			}
		}
	}
}
