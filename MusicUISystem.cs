using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class MusicUISystem : ModSystem {
		public static MusicUISystem Instance { get; private set; }

		internal UserInterface MP_UserInterface;
		internal MusicPlayerUI MusicUI;
		internal const int MaxUnrecordedBoxes = 20;

		internal string UIHoverText = "";
		internal Color UIHoverTextColor = default;

		public override void Load() {
			Instance = this;

			// Setup the Music Player UI.
			if (!Main.dedServ) {
				MusicUI = new MusicPlayerUI();
				MusicUI.Activate();
				MP_UserInterface = new UserInterface();
				MP_UserInterface.SetState(MusicUI);
			}
		}

		public override void Unload() {
			MP_UserInterface = null;
			MusicUI = null;
		}

		public override void PostAddRecipes() {
			// After all PostSetupContent has occured, setup all the MusicData.
			// Go through each key in the Modded MusicBox dictionary and attempt to add them to MusicData.
			foreach (KeyValuePair<int, int> music in tMusicPlayer.itemToMusicReference) {
				int itemID = music.Key;
				int musicID = music.Value;

				if (!ContentSamples.ItemsByType.TryGetValue(itemID, out Item item))
					continue; // If the item does not exist, move onto the next pair

				string displayName = item.ModItem.Mod.DisplayName;
				string name = item.Name.Contains("(") ? item.Name.Substring(item.Name.IndexOf("(") + 1).Replace(")", "") : item.Name;
				tMusicPlayer.AllMusic.Add(new MusicData(musicID, itemID, displayName, name));
			}

			// Setup UI's item slot count.
			if (!Main.dedServ) {
				MusicPlayerUI UI = Instance.MusicUI;

				UI.canPlay = new bool[tMusicPlayer.AllMusic.Count];

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

		public override void UpdateUI(GameTime gameTime) {
			MP_UserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
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

				layers.Insert(++index, new LegacyGameInterfaceLayer("BossChecklist: Custom UI Hover Text",
					delegate {
						// Detect if the hover text is a single localization key and draw the hover text accordingly
						if (UIHoverText != "") {
							string text = UIHoverText.StartsWith("$Mods.") ? Language.GetTextValue(UIHoverText.Substring(1)) : UIHoverText;
							DrawTooltipBG(Main.spriteBatch, text, UIHoverTextColor);
						}
						// Reset text and color back to default state
						UIHoverText = "";
						UIHoverTextColor = Main.MouseTextColorReal;
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		private void DrawTooltipBG(SpriteBatch sb, string text, Color textColor = default) {
			if (text == "")
				return;

			int padd = 20;
			Vector2 stringVec = FontAssets.MouseText.Value.MeasureString(text);
			Rectangle bgPos = new Rectangle(Main.mouseX + 20, Main.mouseY + 20, (int)stringVec.X + padd, (int)stringVec.Y + padd - 5);
			bgPos.X = Utils.Clamp(bgPos.X, 0, Main.screenWidth - bgPos.Width);
			bgPos.Y = Utils.Clamp(bgPos.Y, 0, Main.screenHeight - bgPos.Height);

			Vector2 textPos = new Vector2(bgPos.X + padd / 2, bgPos.Y + padd / 2);
			if (textColor == default) {
				textColor = Main.MouseTextColorReal;
			}

			Utils.DrawInvBG(sb, bgPos, new Color(23, 25, 81, 255) * 0.925f);
			Utils.DrawBorderString(sb, text, textPos, textColor);
		}
	}
}
