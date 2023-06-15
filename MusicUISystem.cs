using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	[Autoload(Side = ModSide.Client)]
	internal class MusicUISystem : ModSystem {
		public static MusicUISystem Instance { get; private set; }

		internal UserInterface MP_UserInterface;
		internal MusicPlayerUI MusicUI;

		public Dictionary<int, int> itemToMusicReference;
		internal Dictionary<string, List<int>> RegisteredMusic;
		internal const int MaxUnrecordedBoxes = 20;

		internal string UIHoverText = "";
		internal Color UIHoverTextColor = default;

		public override void Load() {
			Instance = this;

			// Setup the Music Player UI.
			MP_UserInterface = new UserInterface();
			MusicUI = new MusicPlayerUI();
			MusicUI.Activate();
			MP_UserInterface.SetState(MusicUI);

			// This grabs the entire dictionary of MODDED music-to-musicbox correlations. Code provided by Jopojelly. Thank you, Jopo!
			FieldInfo itemToMusicField = typeof(MusicLoader).GetField("itemToMusic", BindingFlags.Static | BindingFlags.NonPublic);
			itemToMusicReference = (Dictionary<int, int>)itemToMusicField.GetValue(null);
		}

		public override void Unload() {
			MP_UserInterface = null;
			MusicUI = null;
		}

		public override void UpdateUI(GameTime gameTime) {
			if (MP_UserInterface?.CurrentState != null)
				MP_UserInterface?.Update(gameTime);
		}

		public override void PostAddRecipes() {
			if (itemToMusicReference != null) {
				// Go through each key in the Modded MusicBox dictionary and attempt to add them to MusicData.
				foreach (KeyValuePair<int, int> music in itemToMusicReference) {
					if (!ContentSamples.ItemsByType.TryGetValue(music.Key, out Item item))
						continue; // If the item does not exist, move onto the next pair

					string name = item.Name.Contains("(") ? item.Name.Substring(item.Name.IndexOf("(") + 1).Replace(")", "") : item.Name;
					string modSource = item.ModItem == null ? "Terraria" : item.ModItem.Mod.DisplayName;

					if (!tMusicPlayer.AllMusic.Exists(x => x.musicbox == music.Key)) {
						tMusicPlayer.AllMusic.Add(new MusicData(music.Value, music.Key, modSource, name));
					}
					else {
						tMusicPlayer.instance.Logger.Info($"Prevented a vanilla overwrite for {name} [#{music.Key}]. Selection may play undesired music.");
					}
				}
			}
			else {
				tMusicPlayer.instance.Logger.Error($"itemToMusicReference failed and has a null value. Modded music will not be added to the music player.");
			}

			// Setup UI's item slot count.
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
			RegisteredMusic = new Dictionary<string, List<int>> {
				{ "Terraria", new List<int>() },
				{ "Terraria Otherworld", new List<int>() }
			};
			foreach (MusicData box in UI.musicData) {
				RegisteredMusic.TryAdd(box.mod, new List<int>());
				RegisteredMusic[box.mod].Add(box.music);
			}

			UI.ModList = RegisteredMusic.Keys.ToList();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			// Draws the Music Player UI.
			int index = layers.FindIndex((GameInterfaceLayer layer) => layer.Name.Equals("Vanilla: Inventory"));
			if (index != -1) {
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Music Player",
					delegate {
						if (MusicUI.MusicPlayerVisible) {
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
