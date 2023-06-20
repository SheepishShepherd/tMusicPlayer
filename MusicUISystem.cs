using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
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

		internal List<MusicData> AllMusic;
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

			// A list that contains all the MusicData we need, including mod and name.
			AllMusic = new List<MusicData> {
				#region 1.3 Music Boxes
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
				#endregion
				#region 1.4 Music Boxes
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
				new MusicData(MusicID.Deerclops, ItemID.MusicBoxDeerclops, 86),
				new MusicData(MusicID.Shimmer, ItemID.MusicBoxShimmer, 87),
				#endregion
				#region Otherworld Music
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
				#endregion
			};
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

					LocalizedText name = item.ModItem == null ? Lang.GetItemName(music.Key) : item.ModItem.GetLocalization("DisplayName");
					string modSource = item.ModItem == null ? "Terraria" : item.ModItem.Mod.Name;

					if (!AllMusic.Exists(x => x.MusicBox == music.Key)) {
						AllMusic.Add(new MusicData(music.Value, music.Key, modSource, name));
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
			if (MusicUI.sortType == SortBy.ID) {
				AllMusic = AllMusic.OrderBy(x => x.MusicID).ToList();
			}
			if (MusicUI.sortType == SortBy.Name) {
				AllMusic = AllMusic.OrderBy(x => x.Name).ToList();
			}

			MusicUI.SelectionSlots = new MusicBoxSlot[AllMusic.Count];
			MusicUI.SortedMusicData = new List<MusicData>(AllMusic);
			MusicUI.OrganizeSelection(initializing: true);
			MusicUI.DisplayBox = MusicUI.SortedMusicData[0]; // defaults to first music box

			// Setup the mod list for the Mod Filter
			// Must occur after all other modded music is established
			RegisteredMusic = new Dictionary<string, List<int>> {
				{ "Terraria", new List<int>() },
				{ "Terraria Otherworld", new List<int>() }
			};
			foreach (MusicData box in MusicUI.SortedMusicData) {
				RegisteredMusic.TryAdd(box.Mod, new List<int>());
				RegisteredMusic[box.Mod].Add(box.MusicID);
			}

			MusicUI.ModList = RegisteredMusic.Keys.ToList();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			// Draws the Music Player UI.
			int index = layers.FindIndex((GameInterfaceLayer layer) => layer.Name.Equals("Vanilla: Inventory"));
			if (index != -1) {
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Music Player",
					delegate {
						if (MusicUI.MusicPlayerVisible)
							MP_UserInterface.Draw(Main.spriteBatch, new GameTime());

						return true;
					},
					InterfaceScaleType.UI)
				);

				layers.Insert(++index, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Custom UI Hover Text",
					delegate {
						if (!string.IsNullOrEmpty(UIHoverText))
							DrawTooltipBackground(Language.GetTextValue(UIHoverText), UIHoverTextColor);

						UIHoverText = ""; // Reset text and color back to default state
						UIHoverTextColor = Main.MouseTextColorReal;
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		/// <summary>
		/// <para>Draws backgrounds for texts similar to the ones used for item tooltips.</para>
		/// <para>ModifyInterfaceLayers will use this method when hovering over an element that changes the <see cref="UIHoverText"/></para>
		/// </summary>
		private void DrawTooltipBackground(string text, Color textColor = default) {
			if (text == "")
				return;

			int padd = 20;
			Vector2 stringVec = FontAssets.MouseText.Value.MeasureString(text);
			Rectangle bgPos = new Rectangle(Main.mouseX + 20, Main.mouseY + 20, (int)stringVec.X + padd, (int)stringVec.Y + padd - 5);
			bgPos.X = Utils.Clamp(bgPos.X, 0, Main.screenWidth - bgPos.Width);
			bgPos.Y = Utils.Clamp(bgPos.Y, 0, Main.screenHeight - bgPos.Height);

			Vector2 textPos = new Vector2(bgPos.X + padd / 2, bgPos.Y + padd / 2);
			if (textColor == default)
				textColor = Main.MouseTextColorReal;

			Utils.DrawInvBG(Main.spriteBatch, bgPos, new Color(23, 25, 81, 255) * 0.925f);
			Utils.DrawBorderString(Main.spriteBatch, text, textPos, textColor);
		}
	}
}
