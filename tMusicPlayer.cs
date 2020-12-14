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
			tMPConfig = ModContent.GetInstance<TMPConfig>();
			HidePlayerHotkey = RegisterHotKey("Hide Music Player", "Up");
			PlayStopHotkey = RegisterHotKey("Play/Stop Music", "Down");
			PrevSongHotkey = RegisterHotKey("Previous Song", "Left");
			NextSongHotkey = RegisterHotKey("Next Song", "Right");

			if (!Main.dedServ) {
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleSpace"), ModContent.ItemType<Items.MusicBoxConsoleSpace>(), ModContent.TileType<Tiles.MusicBoxConsoleSpace>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleOcean"), ModContent.ItemType<Items.MusicBoxConsoleOcean>(), ModContent.TileType<Tiles.MusicBoxConsoleOcean>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleTitle"), ModContent.ItemType<Items.MusicBoxConsoleTitle>(), ModContent.TileType<Tiles.MusicBoxConsoleTitle>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/ConsoleTutorial"), ModContent.ItemType<Items.MusicBoxConsoleTutorial>(), ModContent.TileType<Tiles.MusicBoxConsoleTutorial>());
			}

			// TODO: [1.4] Magic numbers from decompiler... Thats ok, well update it when we add the 1.4 music boxes
			AllMusic = new List<MusicData>
			{
				new MusicData(1, 562),
				new MusicData(18, 1600),
				new MusicData(3, 564),
				new MusicData(19, 1601),
				new MusicData(14, 1596),
				new MusicData(20, 1602),
				new MusicData(21, 1603),
				new MusicData(22, 1604),
				new MusicData(15, 1597),
				new MusicData(4, 566),
				new MusicData(31, 1964),
				new MusicData(29, 1610),
				new MusicData(7, 568),
				new MusicData(8, 569),
				new MusicData(10, 570),
				new MusicData(16, 1598),
				new MusicData(33, 2742),
				new MusicData(9, 571),
				new MusicData(11, 573),
				new MusicData(36, 3237),
				new MusicData(23, 1605),
				new MusicData(26, 1608),
				new MusicData(5, 567),
				new MusicData(12, 572),
				new MusicData(13, 574),
				new MusicData(17, 1599),
				new MusicData(25, 1607),
				new MusicData(24, 1606),
				new MusicData(2, 563),
				new MusicData(27, 1609),
				new MusicData(39, 3371),
				new MusicData(35, 3236),
				new MusicData(37, 3235),
				new MusicData(30, 1963),
				new MusicData(32, 1965),
				new MusicData(34, 3370),
				new MusicData(38, 3044),
				new MusicData(40, 3796),
				new MusicData(41, 3869),
				new MusicData(6, 565)
			};

			FieldInfo field = typeof(SoundLoader).GetField("itemToMusic", BindingFlags.Static | BindingFlags.NonPublic);
			itemToMusicReference = (Dictionary<int, int>)field.GetValue(null);
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
			foreach (int itemID in itemToMusicReference.Keys) {
				Item item = new Item();
				item.SetDefaults(itemID);
				string displayName = item.modItem.mod.DisplayName;
				string name = item.Name.Contains("(") ? item.Name.Substring(item.Name.IndexOf("(") + 1).Replace(")", "") : item.Name;
				int musicID = itemToMusicReference.TryGetValue(itemID, out int num) ? num : (-1);
				if (musicID != -1) {
					AllMusic.Add(new MusicData(musicID, itemID, displayName, name));
				}
			}
			if (!Main.dedServ) {
				MusicPlayerUI.canPlay = new List<bool>();
				foreach (MusicData item in AllMusic) {
					MusicPlayerUI.canPlay.Add(false);
				}
				MusicPlayerUI.SelectionSlots = new MusicBoxSlot[AllMusic.Count];
				MusicPlayerUI.OrganizeSelection(new List<MusicData>(AllMusic), SortBy.Music, FilterBy.None, true);
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			UserInterface mP_UserInterface = MP_UserInterface;
			if (mP_UserInterface != null) {
				mP_UserInterface.Update(gameTime);
			}
		}

		public override void Close()
		{
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
			}
		}

		public static void SendDebugMessage(string message, Color color = default)
		{
			if (tMPConfig.EnableDebugMode) {
				Main.NewText(message, color);
			}
		}
	}
}
