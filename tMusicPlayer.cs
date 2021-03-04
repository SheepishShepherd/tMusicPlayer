using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace tMusicPlayer
{
	public class tMusicPlayer : Mod
	{
		internal static UserInterface MP_UserInterface;
		internal static MusicPlayerUI MusicPlayerUI;

		internal static List<MusicData> AllMusic;
		public static Dictionary<int, int> itemToMusicReference;

		public static TMPConfig tMPConfig;

		internal static ModHotKey HidePlayerHotkey;
		internal static ModHotKey PlayStopHotkey;
		internal static ModHotKey PrevSongHotkey;
		internal static ModHotKey NextSongHotkey;
		
		public override void Load()
		{
			// Setup hotkeys and the configs instance.
			tMPConfig = ModContent.GetInstance<TMPConfig>();
			HidePlayerHotkey = RegisterHotKey("Hide Music Player", "Up");
			PlayStopHotkey = RegisterHotKey("Play/Stop Music", "Down");
			PrevSongHotkey = RegisterHotKey("Previous Song", "Left");
			NextSongHotkey = RegisterHotKey("Next Song", "Right");

			// Register this mods music boxes.
			// TODO: [1.4] Remove music boxes. The 1.4 update reimplements the console music.
			if (!Main.dedServ) {
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleSpace"), ModContent.ItemType<Items.MusicBoxConsoleSpace>(), ModContent.TileType<Tiles.MusicBoxConsoleSpace>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleOcean"), ModContent.ItemType<Items.MusicBoxConsoleOcean>(), ModContent.TileType<Tiles.MusicBoxConsoleOcean>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleTitle"), ModContent.ItemType<Items.MusicBoxConsoleTitle>(), ModContent.TileType<Tiles.MusicBoxConsoleTitle>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleTutorial"), ModContent.ItemType<Items.MusicBoxConsoleTutorial>(), ModContent.TileType<Tiles.MusicBoxConsoleTutorial>());
			}

			// A list that contains all the MusicData we need, including mod and name.
			// TODO: [1.4] Reorder boxes based on progression? Maybe grab info from bosschecklist in the future?
			AllMusic = new List<MusicData>
			{
				new MusicData(MusicID.OverworldDay, ItemID.MusicBoxOverworldDay),
				new MusicData(MusicID.AltOverworldDay, ItemID.MusicBoxAltOverworldDay),
				new MusicData(MusicID.Night, ItemID.MusicBoxNight),
				new MusicData(MusicID.Rain, ItemID.MusicBoxRain),
				new MusicData(MusicID.Snow, ItemID.MusicBoxSnow),
				new MusicData(MusicID.Ice, ItemID.MusicBoxIce),
				new MusicData(MusicID.Desert, ItemID.MusicBoxDesert),
				new MusicData(MusicID.Ocean, ItemID.MusicBoxOcean),
				new MusicData(MusicID.Space, ItemID.MusicBoxSpace),
				new MusicData(MusicID.Underground, ItemID.MusicBoxUnderground),
				new MusicData(MusicID.AltUnderground, ItemID.MusicBoxAltUnderground),
				new MusicData(MusicID.Mushrooms, ItemID.MusicBoxMushrooms),
				new MusicData(MusicID.Jungle, ItemID.MusicBoxJungle),
				new MusicData(MusicID.Corruption, ItemID.MusicBoxCorruption),
				new MusicData(MusicID.UndergroundCorruption, ItemID.MusicBoxUndergroundCorruption),
				new MusicData(MusicID.Crimson, ItemID.MusicBoxCrimson),
				new MusicData(MusicID.UndergroundCrimson, ItemID.MusicBoxUndergroundCrimson),
				new MusicData(MusicID.TheHallow, ItemID.MusicBoxTheHallow),
				new MusicData(MusicID.UndergroundHallow, ItemID.MusicBoxUndergroundHallow),
				new MusicData(MusicID.Hell, ItemID.MusicBoxHell),
				new MusicData(MusicID.Dungeon, ItemID.MusicBoxDungeon),
				new MusicData(MusicID.Temple, ItemID.MusicBoxTemple),
				new MusicData(MusicID.Boss1, ItemID.MusicBoxBoss1),
				new MusicData(MusicID.Boss2, ItemID.MusicBoxBoss2),
				new MusicData(MusicID.Boss3, ItemID.MusicBoxBoss3),
				new MusicData(MusicID.Boss4, ItemID.MusicBoxBoss4),
				new MusicData(MusicID.Boss5, ItemID.MusicBoxBoss5),
				new MusicData(MusicID.Plantera, ItemID.MusicBoxPlantera),
				new MusicData(MusicID.Eerie, ItemID.MusicBoxEerie),
				new MusicData(MusicID.Eclipse, ItemID.MusicBoxEclipse),
				new MusicData(MusicID.GoblinInvasion, ItemID.MusicBoxGoblins),
				new MusicData(MusicID.PirateInvasion, ItemID.MusicBoxPirates),
				new MusicData(MusicID.MartianMadness, ItemID.MusicBoxMartians),
				new MusicData(MusicID.PumpkinMoon, ItemID.MusicBoxPumpkinMoon),
				new MusicData(MusicID.FrostMoon, ItemID.MusicBoxFrostMoon),
				new MusicData(MusicID.TheTowers, ItemID.MusicBoxTowers),
				new MusicData(MusicID.LunarBoss, ItemID.MusicBoxLunarBoss),
				new MusicData(MusicID.Sandstorm, ItemID.MusicBoxSandstorm),
				new MusicData(MusicID.OldOnesArmy, ItemID.MusicBoxDD2),
				new MusicData(MusicID.Title, ItemID.MusicBoxTitle)
			};

			// Code provided by Jopojelly. Thank you, Jopo!
			// This grabs the entire dictionary of MODDED music-to-musicbox correlations.
			FieldInfo field = typeof(SoundLoader).GetField("itemToMusic", BindingFlags.Static | BindingFlags.NonPublic);
			itemToMusicReference = (Dictionary<int, int>)field.GetValue(null);

			// Setup the Music Player UI.
			if (!Main.dedServ) {
				MusicPlayerUI = new MusicPlayerUI();
				MusicPlayerUI.Activate();
				MP_UserInterface = new UserInterface();
				MP_UserInterface.SetState(MusicPlayerUI);
			}
		}

		public override void Unload()
		{
			tMPConfig = null;
			HidePlayerHotkey = null;
			PlayStopHotkey = null;
			PrevSongHotkey = null;
			NextSongHotkey = null;
			MP_UserInterface = null;
			MusicPlayerUI = null;
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
				string displayName = item.modItem.mod.DisplayName;
				string name = item.Name.Contains("(") ? item.Name.Substring(item.Name.IndexOf("(") + 1).Replace(")", "") : item.Name;
				int musicID = itemToMusicReference.TryGetValue(itemID, out int num) ? num : (-1);
				if (musicID != -1) {
					AllMusic.Add(new MusicData(musicID, itemID, displayName, name));
				}
			}
			item.TurnToAir();

			// Setup the canPlay list to match thie size of AllMusic, as well as the UI's item slots.
			if (!Main.dedServ) {
				MusicPlayerUI.canPlay = new List<bool>();
				foreach (MusicData box in AllMusic) {
					MusicPlayerUI.canPlay.Add(false);
				}
				MusicPlayerUI.SelectionSlots = new MusicBoxSlot[AllMusic.Count];
				MusicPlayerUI.OrganizeSelection(null, SortBy.ID, ProgressBy.None, "", true);
				
				// Setup the mod list for the Mod Filter
				// Must occur after all other modded music is established
				MusicPlayerUI.ModList = new List<string>();
				List<MusicData> musicData = new List<MusicData>(tMusicPlayer.AllMusic);
				foreach (MusicData box in musicData) {
					if (!MusicPlayerUI.ModList.Contains(box.mod)) {
						MusicPlayerUI.ModList.Add(box.mod);
					}
				}
				MusicPlayerUI.ModList.Sort();
				MusicPlayerUI.ModList.Remove("Terraria");
				MusicPlayerUI.ModList.Insert(0, "Terraria"); // Put Terraria infront of all mods
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			// Update Music Player UI as long as it exists.
			UserInterface mP_UserInterface = MP_UserInterface;
			if (mP_UserInterface != null) {
				mP_UserInterface.Update(gameTime);
			}
		}

		public override void Close()
		{
			// A temporary fix for unloaded music still playing.
			// TODO: [1.4] Remove since we will no longer have music boxes for this mod.
			int[] array = new int[4]
			{
				GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleSpace"),
				GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleOcean"),
				GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleTitle"),
				GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleTutorial")
			};
			foreach (int index in array) {
				if (Main.music[index] != null && Main.music[index].IsPlaying) {
					Main.music[index].Stop(AudioStopOptions.Immediate);
				}
			}
			base.Close();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			// Draws the Music Player UI.
			int index = layers.FindIndex((GameInterfaceLayer layer) => layer.Name.Equals("Vanilla: Inventory"));
			if (index != -1) {
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Music Player",
					delegate {
						if (MusicPlayerUI.mpToggleVisibility) {
							MP_UserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
				/*
				// TODO: reimplement right click to play song text in selection menu
				layers.Insert(index + 1, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Right-click text",
					delegate {
						//if (ExampleUI.Visible) {
						//	  _exampleUserInterface.Draw(Main.spriteBatch, new GameTime());
						//}
						return true;
					},
					InterfaceScaleType.UI)
				);
				*/
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
