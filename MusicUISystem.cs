using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class MusicUISystem : ModSystem
	{
		public static MusicUISystem Instance { get; private set; }

		internal UserInterface MP_UserInterface;
		internal MusicPlayerUI MusicUI;
		internal const int MaxUnrecordedBoxes = 20;

		public override void Load()
		{
			Instance = this;

			// Setup the Music Player UI.
			if (!Main.dedServ) {
				MusicUI = new MusicPlayerUI();
				MusicUI.Activate();
				MP_UserInterface = new UserInterface();
				MP_UserInterface.SetState(MusicUI);
			}
		}

		public override void Unload()
		{
			MP_UserInterface = null;
			MusicUI = null;
		}

		public override void PostAddRecipes()
		{
			// After all PostSetupContent has occured, setup all the MusicData.
			// Go through each key in the Modded MusicBox dictionary and attempt to add them to MusicData.
			foreach (int itemID in tMusicPlayer.itemToMusicReference.Keys) {
				Item item = ContentSamples.ItemsByType[itemID];
				string displayName = item.ModItem.Mod.DisplayName;
				string name = item.Name.Contains("(") ? item.Name.Substring(item.Name.IndexOf("(") + 1).Replace(")", "") : item.Name;
				int musicID = tMusicPlayer.itemToMusicReference.TryGetValue(itemID, out int num) ? num : (-1);
				if (musicID != -1) {
					tMusicPlayer.AllMusic.Add(new MusicData(musicID, itemID, displayName, name));
				}
			}

			// Setup UI's item slot count.
			if (!Main.dedServ) {
				MusicPlayerUI UI = Instance.MusicUI;

				if (UI.sortType == SortBy.ID) {
					tMusicPlayer.AllMusic = tMusicPlayer.AllMusic.OrderBy(x => x.music).ToList();
				}
				if (UI.sortType == SortBy.Name) {
					tMusicPlayer.AllMusic = tMusicPlayer.AllMusic.OrderBy(x => x.name).ToList();
				}
				
				UI.SelectionSlots = new MusicBoxSlot[tMusicPlayer.AllMusic.Count];
				UI.musicData = new List<MusicData>(tMusicPlayer.AllMusic);
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

		public override void UpdateUI(GameTime gameTime)
		{
			// Update Music Player UI as long as it exists.
			UserInterface mP_UserInterface = MP_UserInterface;
			if (mP_UserInterface != null) {
				mP_UserInterface.Update(gameTime);
			}
		}

		//int lastSeenScreenWidth;
		//int lastSeenScreenHeight;
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			// Draws the Music Player UI.
			int index = layers.FindIndex((GameInterfaceLayer layer) => layer.Name.Equals("Vanilla: Inventory"));
			if (index != -1) {
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Music Player",
					delegate {
						if (MusicUI.mpToggleVisibility) {
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
	}
}
