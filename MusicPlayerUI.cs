using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.UI;
using Terraria.UI;
using static tModPorter.ProgressUpdate;

namespace tMusicPlayer
{
	internal class MusicPlayerUI : UIState
	{
		private bool playerVisible;
		public bool MusicPlayerVisible {
			get => playerVisible;
			set {
				playerVisible = value;
				this.AddOrRemoveChild(MusicPlayerPanel, value);
			}
		}

		private bool selectionVisible;
		public bool SelectionPanelVisible {
			get => selectionVisible;
			set {
				selectionVisible = value;
				this.AddOrRemoveChild(SelectionPanel, value);

				if (value) {
					OrganizeSelection(); // refresh on open
				}
				else {
					if (searchBar.currentString != "")
						searchBar.ClearSearchText(); // If not visible, clear search bar text and filter
				}
			}
		}

		// UI Panels
		public BackDrop MusicPlayerPanel;
		public BackDrop SelectionPanel;
		public static Asset<Texture2D> panelPlayer;
		public static Asset<Texture2D> panelMini;
		public static Asset<Texture2D> panelSelect;

		// Musicplayer buttons
		public HoverButton prevButton;
		public HoverButton playButton;
		public HoverButton nextButton;
		public HoverButton viewButton;
		public HoverButton detectButton;
		public HoverButton recordButton;
		public HoverButton expandButton;

		// Selection Panel Buttons
		public HoverButton closeButton;
		public HoverButton ejectButton;
		public HoverButton favoritesButton;
		public HoverButton sortIDButton;
		public HoverButton sortNameButton;
		public HoverButton filterModButton;
		public HoverButton clearFilterModButton;
		public HoverButton availabilityButton;
		public HoverButton viewModeButton;

		public static Asset<Texture2D> buttonTextures;
		public static Asset<Texture2D> closeTextures;

		// The Music Player's music box display
		public MusicBoxSlot DisplayMusicSlot;
		public int DisplayBox = 0;
		public MusicData DisplayBoxData() => MusicUISystem.Instance.AllMusic[DisplayBox];

		public MusicData VisualBoxDisplayed() {
			if (listening)
				return MusicUISystem.Instance.AllMusic[ListenDisplay];

			return DisplayBoxData();
		}

		public int ListenDisplay = -1;
		public int playingMusic = -1;
		public bool listening = false;
		public bool recording = false;
		public bool smallPanel = true;

		internal bool viewMode = false;
		internal bool viewFavs = false;

		// Selection Panel content and functionality
		public MusicBoxSlot BoxEntrySlot;
		public MusicBoxSlot[] SelectionSlots;
		public UIList SelectionList;
		public FixedUIScrollbar selectionScrollBar;
		public SearchBar searchBar;

		// Sort/Filter functionality
		internal List<MusicData> musicData;
		public SortBy sortType = SortBy.ID;
		public ProgressBy availabililty = ProgressBy.None;
		public string FilterMod = "";
		internal List<string> ModList;
		
		public override void OnInitialize() {
			panelPlayer = ModContent.Request<Texture2D>("tMusicPlayer/UI/panel_player", AssetRequestMode.ImmediateLoad);
			panelMini = ModContent.Request<Texture2D>("tMusicPlayer/UI/panel_player_mini", AssetRequestMode.ImmediateLoad);
			panelSelect = ModContent.Request<Texture2D>("tMusicPlayer/UI/panel_selection", AssetRequestMode.ImmediateLoad);

			buttonTextures = ModContent.Request<Texture2D>("tMusicPlayer/UI/buttons", AssetRequestMode.ImmediateLoad);
			closeTextures = ModContent.Request<Texture2D>("tMusicPlayer/UI/close", AssetRequestMode.ImmediateLoad);

			MusicPlayerPanel = new BackDrop(panelPlayer) {
				Id = "MusicPlayerPanel",
			};
			MusicPlayerPanel.Left.Pixels = 1115f;
			MusicPlayerPanel.Top.Pixels = 16f;

			prevButton = new HoverButton(buttonTextures.Value, new Point(0, 0)) {
				Id = "prev"
			};
			prevButton.Left.Pixels = 100f;
			prevButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - prevButton.Height.Pixels - 4f;
			prevButton.OnLeftClick += (a, b) => ChangeDisplay(false);
			prevButton.OnRightClick += (a, b) => ChangeDisplay(false, true);

			playButton = new HoverButton(buttonTextures.Value, new Point(1, 0)) {
				Id = "play"
			};
			playButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - playButton.Width.Pixels - 6f;
			playButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - playButton.Height.Pixels * 2f - 4f;
			playButton.OnLeftClick += (a, b) => ToggleButton(MusicMode.Play);
			MusicPlayerPanel.Append(playButton);

			nextButton = new HoverButton(buttonTextures.Value, new Point(3, 0)) {
				Id = "next"
			};
			nextButton.Left.Pixels = prevButton.Left.Pixels + playButton.Width.Pixels - 2f + nextButton.Width.Pixels - 2f;
			nextButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - nextButton.Height.Pixels - 4f;
			nextButton.OnLeftClick += (a, b) => ChangeDisplay(true);
			nextButton.OnRightClick += (a, b) => ChangeDisplay(true, true);

			viewButton = new HoverButton(buttonTextures.Value, new Point(6, 0)) {
				Id = "view"
			};
			viewButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - viewButton.Width.Pixels - 6f;
			viewButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - viewButton.Height.Pixels - 4f;
			viewButton.OnLeftClick += (a, b) => SelectionPanelVisible = !SelectionPanelVisible;
			MusicPlayerPanel.Append(viewButton);

			detectButton = new HoverButton(buttonTextures.Value, new Point(4, 0)) {
				Id = "listen"
			};
			detectButton.Left.Pixels = viewButton.Left.Pixels - detectButton.Width.Pixels - 2f;
			detectButton.Top.Pixels = viewButton.Top.Pixels;
			detectButton.OnLeftClick += (a, b) => ToggleButton(MusicMode.Listen);

			recordButton = new HoverButton(buttonTextures.Value, new Point(7, 0)) {
				Id = "record"
			};
			recordButton.Left.Pixels = detectButton.Left.Pixels - recordButton.Width.Pixels - 4f;
			recordButton.Top.Pixels = viewButton.Top.Pixels;
			recordButton.OnLeftClick += (a, b) => ToggleButton(MusicMode.Record);

			expandButton = new HoverButton(closeTextures.Value, new Point(1, 0)) {
				Id = "expand"
			};
			expandButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - expandButton.Width.Pixels - 8f;
			expandButton.Top.Pixels = 4f;
			expandButton.OnLeftClick += (a, b) => SwapPanelSize();
			MusicPlayerPanel.Append(expandButton);
			
			DisplayMusicSlot = new MusicBoxSlot(0, 1f) {
				IsDisplaySlot = true
			};
			DisplayMusicSlot.Left.Pixels = 8f;
			DisplayMusicSlot.Top.Pixels = MusicPlayerPanel.Height.Pixels / 2f - TextureAssets.InventoryBack.Value.Height / 2;
			MusicPlayerPanel.Append(DisplayMusicSlot);

			SelectionPanel = new BackDrop(panelSelect) {
				Id = "SelectionPanel"
			};
			SelectionPanel.Left.Pixels = (Main.screenWidth / 2) - SelectionPanel.Width.Pixels / 2f;
			SelectionPanel.Top.Pixels = (Main.screenHeight / 2) - SelectionPanel.Height.Pixels / 2f;

			// Positioning base for filter buttons
			float center = (SelectionPanel.Width.Pixels / 2) - 11;

			favoritesButton = new HoverButton(buttonTextures.Value, new Point(7, 2)) {
				Id = "showFavorites"
			};
			favoritesButton.Left.Pixels = center - (20 * 3) - (8 * 2) + 4;
			favoritesButton.Top.Pixels = 42;
			favoritesButton.OnLeftClick += (a, b) => OrganizeSelection(clickedFavorites: true);
			SelectionPanel.Append(favoritesButton);

			sortIDButton = new HoverButton(buttonTextures.Value, new Point(0, 2)) {
				Id = "sortbyid"
			};
			sortIDButton.Left.Pixels = center - (20 * 2) - 8 + 4;
			sortIDButton.Top.Pixels = 42;
			sortIDButton.OnLeftClick += (a, b) => OrganizeSelection(sortBy: SortBy.ID);
			SelectionPanel.Append(sortIDButton);

			sortNameButton = new HoverButton(buttonTextures.Value, new Point(1, 2)) {
				Id = "sortbyname"
			};
			sortNameButton.Left.Pixels = center - 20 - 8 + 4;
			sortNameButton.Top.Pixels = 42;
			sortNameButton.OnLeftClick += (a, b) => OrganizeSelection(sortBy: SortBy.Name);
			SelectionPanel.Append(sortNameButton);

			filterModButton = new HoverButton(buttonTextures.Value, new Point(2, 2)) {
				Id = "filtermod"
			};
			filterModButton.Left.Pixels = center + 8 - 4;
			filterModButton.Top.Pixels = 42;
			filterModButton.OnLeftClick += (a, b) => OrganizeSelection(filterMod: UpdateModFilter(true));
			filterModButton.OnRightClick += (a, b) => OrganizeSelection(filterMod: UpdateModFilter(false));
			SelectionPanel.Append(filterModButton);

			clearFilterModButton = new HoverButton(buttonTextures.Value, new Point(3, 2)) {
				Id = "clearfiltermod"
			};
			clearFilterModButton.Left.Pixels = center + 20 + 8 - 4;
			clearFilterModButton.Top.Pixels = 42;
			clearFilterModButton.OnLeftClick += (a, b) => OrganizeSelection(filterMod: ResetModFilter());
			SelectionPanel.Append(clearFilterModButton);

			availabilityButton = new HoverButton(buttonTextures.Value, new Point(4, 2)) {
				Id = "availability"
			};
			availabilityButton.Left.Pixels = center + (20 * 2) + (8 * 2) - 4;
			availabilityButton.Top.Pixels = 42;
			availabilityButton.OnLeftClick += (a, b) => OrganizeSelection(progressBy: UpdateAvailabilityFilter(true));
			availabilityButton.OnRightClick += (a, b) => OrganizeSelection(progressBy: UpdateAvailabilityFilter(false));
			SelectionPanel.Append(availabilityButton);

			closeButton = new HoverButton(closeTextures.Value, new Point(0, 0)) {
				Id = "select_close"
			};
			closeButton.Left.Pixels = SelectionPanel.Width.Pixels - closeButton.Width.Pixels - 11f;
			closeButton.Top.Pixels = 12f;
			closeButton.OnLeftClick += (a, b) => SelectionPanelVisible = !SelectionPanelVisible;
			SelectionPanel.Append(closeButton);

			viewModeButton = new HoverButton(buttonTextures.Value, new Point(0, 4)) {
				Id = "viewmode"
			};
			viewModeButton.Left.Pixels = SelectionPanel.Width.Pixels - closeButton.Width.Pixels - 13f;
			viewModeButton.Top.Pixels = closeButton.Top.Pixels + closeButton.Height.Pixels + 4f;
			viewModeButton.OnLeftClick += (a, b) => UpdateViewMode();
			SelectionPanel.Append(viewModeButton);

			searchBar = new SearchBar("Search...", "");
			searchBar.Width.Pixels = 216f;
			searchBar.Height.Pixels = 28f;
			searchBar.Top.Pixels = 9f;
			searchBar.Left.Pixels = 12f;
			SelectionPanel.Append(searchBar);

			BoxEntrySlot = new MusicBoxSlot(ItemID.MusicBox, 0.85f) {
				IsEntrySlot = true
			};
			BoxEntrySlot.Left.Pixels = closeButton.Left.Pixels - BoxEntrySlot.Width.Pixels - 9f;
			BoxEntrySlot.Top.Pixels = closeButton.Top.Pixels;
			SelectionPanel.Append(BoxEntrySlot);

			ejectButton = new HoverButton(buttonTextures.Value, new Point(8, 2)) {
				Id = "ejectMusicBoxes"
			};
			ejectButton.Left.Pixels = BoxEntrySlot.Left.Pixels + (BoxEntrySlot.Width.Pixels / 2) - (ejectButton.Width.Pixels / 2);
			ejectButton.Top.Pixels = 42;
			ejectButton.OnLeftClick += (a, b) => EjectBox(false);
			ejectButton.OnRightClick += (a, b) => EjectBox(true);
			SelectionPanel.Append(ejectButton);

			selectionScrollBar = new FixedUIScrollbar();
			selectionScrollBar.SetView(10f, 1000f);
			selectionScrollBar.Top.Pixels = 76f;
			selectionScrollBar.Left.Pixels = -10f;
			selectionScrollBar.Height.Set(0f, 0.75f);
			selectionScrollBar.HAlign = 1f;
			SelectionPanel.Append(selectionScrollBar);

			SelectionList = new UIList();
			SelectionList.Width.Pixels = SelectionPanel.Width.Pixels;
			SelectionList.Height.Pixels = SelectionPanel.Height.Pixels - 85f;
			SelectionList.Left.Pixels = 0f;
			SelectionList.Top.Pixels = 72f;
			SelectionPanel.Append(SelectionList);
		}

		public override void Update(GameTime gameTime) {
			// This code mimics the "Music Box Recording" process.
			// Check if we have music boxes at the ready, if the player is in record mode and music is currently playing.
			// If all of those apply, we also go a rand check which will trigger the "recording" code.
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();

			if (modplayer.musicBoxesStored > 0 && recording && Main.curMusic > 0 && Main.rand.NextBool(540)) {
				int index = MusicUISystem.Instance.AllMusic.FindIndex(x => x.MusicID == Main.curMusic); // Make sure curMusic is a music box.
				if (index != -1) {
					MusicData musicData = MusicUISystem.Instance.AllMusic[index];
                    SoundEngine.PlaySound(SoundID.Item166);
					if (!modplayer.BoxIsCollected(musicData.MusicBox)) {
						// If we don't have it in our music player, automatically add it in.
						modplayer.MusicBoxList.Add(new ItemDefinition(musicData.MusicBox));
						musicData.canPlay = true; // as soon as it is recorded, the player should be able to play the music
					}
					else {
						// If we do have it already, spawn the item.
						player.QuickSpawnItem(player.GetSource_OpenItem(musicData.MusicBox), musicData.MusicBox);
					}
					tMusicPlayer.SendDebugText($"Music Box ({musicData.name}) obtained!", Color.BlanchedAlmond);

					// Automatically turn recording off and reduce the amount of stored music boxes by 1.
					recording = false;
					modplayer.musicBoxesStored--;
				}
			}

			base.Update(gameTime);

			if (Main.gameMenu) {
				playingMusic = -1;
				ListenDisplay = -1;
				listening = false;
			}

			if (tMusicPlayer.HidePlayerHotkey.JustPressed) {
				MusicPlayerVisible = !MusicPlayerVisible;
			}
			else if (tMusicPlayer.PlayStopHotkey.JustPressed) {
				ToggleButton(MusicMode.Play);
			}
			else if (tMusicPlayer.PrevSongHotkey.JustPressed) {
				ChangeDisplay(false, false);
			}
			else if (tMusicPlayer.NextSongHotkey.JustPressed) {
				ChangeDisplay(true, false);
			}
			
			if (!listening && MusicUISystem.Instance.AllMusic.All(data => data.canPlay == false)) {
				ToggleButton(MusicMode.Listen); // If nothing can be played, turn on 'listening mode'
			}
		}

		public int FindNextIndex() {
			for (int i = DisplayBoxData().GetIndex; i < musicData.Count; i++) {
				if (i != DisplayBoxData().GetIndex && DisplayBoxData().canPlay)
					return i;
			}
			return -1;
		}

		public int FindPrevIndex() {
			int index = musicData.FindIndex(x => x.MusicID == MusicUISystem.Instance.AllMusic[DisplayBox].MusicID);
			for (int i = DisplayBoxData().GetIndex; i >= 0; i--) {
				if (i != DisplayBoxData().GetIndex && DisplayBoxData().canPlay)
					return i;
			}
			return -1;
		}

		private void ChangeDisplay(bool next, bool jumpToEnd = false) {
			if (!listening) {
				int newIndex = next ? FindNextIndex() : FindPrevIndex();
				if (newIndex != -1) { 
					DisplayBox = MusicUISystem.Instance.AllMusic.FindIndex(x => x.MusicID == musicData[newIndex].MusicID);
					if (playingMusic > -1) {
						playingMusic = MusicUISystem.Instance.AllMusic[DisplayBox].MusicID;
					}
				}
			}
		}

		internal string ResetModFilter() {
			FilterMod = "";
			tMusicPlayer.SendDebugText($"FilterMod: " + FilterMod);
			return FilterMod;
		}

		internal ProgressBy UpdateAvailabilityFilter(bool next) {
			return availabililty switch {
				ProgressBy.None => next ? ProgressBy.Obtained : ProgressBy.Unobtained,
				ProgressBy.Obtained => next ? ProgressBy.Unobtained : ProgressBy.None,
				ProgressBy.Unobtained => next ? ProgressBy.None : ProgressBy.Obtained,
				_ => ProgressBy.None
			};
		}

		internal string UpdateModFilter(bool next) {
			int indexOfCurrent = ModList.IndexOf(FilterMod);
			if (next && indexOfCurrent == ModList.Count - 1) {
				FilterMod = ModList[0];
			}
			else if (!next && indexOfCurrent == 0) {
				FilterMod = ModList[ModList.Count - 1];
			}
			else {
				int nextOrPrev = next ? 1 : -1;
				FilterMod = ModList[indexOfCurrent + nextOrPrev];
			}
			tMusicPlayer.SendDebugText($"FilterMod: " + FilterMod);
			return FilterMod;
		}

		internal void UpdateViewMode() {
			viewMode = !viewMode;
			OrganizeSelection();
		}

		internal void EjectBox(bool ejectAll) {
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();
			if (modplayer.musicBoxesStored <= 0)
				return;

			if (ejectAll) {
				while (modplayer.musicBoxesStored > 0) {
					player.QuickSpawnItem(player.GetSource_OpenItem(ItemID.MusicBox), ItemID.MusicBox);
					modplayer.musicBoxesStored--;
				}
			}
			else {
				player.QuickSpawnItem(player.GetSource_OpenItem(ItemID.MusicBox), ItemID.MusicBox);
				modplayer.musicBoxesStored--;
			}

			if (modplayer.musicBoxesStored == 0)
				recording = false;
		}

		internal void OrganizeSelection(bool initializing = false, SortBy? sortBy = null, ProgressBy? progressBy = null, string filterMod = null, bool clickedFavorites = false) {
			if (initializing) {
				sortType = SortBy.ID;
				availabililty = ProgressBy.None;
				FilterMod = "";
			}
			else {
				if (sortBy.HasValue)
					sortType = sortBy.Value;

				if (progressBy.HasValue)
					availabililty = progressBy.Value;

				if (!string.IsNullOrEmpty(filterMod))
					FilterMod = filterMod;

				if (clickedFavorites)
					viewFavs = !viewFavs;
			}

			if (sortType == SortBy.ID) {
				musicData = musicData.OrderBy(x => x.MusicID).ToList();
			}
			else if (sortType == SortBy.Name) {
				musicData = musicData.OrderBy(x => x.name).ToList();
			}
			
			SelectionList.Clear();

			ItemSlotRow newRow = new ItemSlotRow(0);
			int slotCount = 0;
			int col = 0;
			int row = 0;
			foreach (MusicData data in musicData) {
				if (!initializing) {
					MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
					bool CheckFilterMod = FilterMod != "" && (data.Mod != FilterMod);
					bool CheckObtained = availabililty == ProgressBy.Obtained && !modplayer.BoxIsCollected(data.MusicBox);
					bool CheckUnobtained = availabililty == ProgressBy.Unobtained && modplayer.BoxIsCollected(data.MusicBox);
					bool CheckFavorited = viewFavs && !modplayer.BoxIsFavorited(data.MusicBox);

					if (CheckFilterMod || CheckObtained || CheckUnobtained || CheckFavorited)
						continue;
				}

				MusicBoxSlot boxSlot = SelectionSlots[slotCount];

				if (viewMode) {
					newRow = new ItemSlotRow(slotCount);

					// Item Slot
					boxSlot = new MusicBoxSlot(data.MusicBox, 0.85f) {
						IsSelectionSlot = true
					};
					boxSlot.Left.Pixels = 20f;
					boxSlot.Top.Pixels = (newRow.Height.Pixels / 2f) - (boxSlot.Height.Pixels / 2f);
					newRow.Append(boxSlot);

					// Play button
					HoverButton playSong = new HoverButton(buttonTextures.Value, new Point(1, 0)) {
						Id = "altplay",
						refNum = data.GetIndex
					};
					playSong.Width.Pixels = 22f;
					playSong.Height.Pixels = 22f;
					playSong.Left.Pixels = boxSlot.Left.Pixels + boxSlot.Width.Pixels + 8f;
					playSong.Top.Pixels = (newRow.Height.Pixels / 2f) - (playSong.Height.Pixels / 2f);
					playSong.OnLeftClick += (a, b) => ListViewPlaySong(data.GetIndex);
					newRow.Append(playSong);

					// Song name and mod
					UIText songName = new UIText(data.name, 0.85f);
					songName.Left.Pixels = playSong.Left.Pixels + playSong.Width.Pixels + 8f;
					songName.Top.Pixels = (newRow.Height.Pixels / 2f) - 15f;
					newRow.Append(songName);

					UIText songMod = new UIText(data.Mod, 0.85f);
					songMod.Left.Pixels = playSong.Left.Pixels + playSong.Width.Pixels + 8f;
					songMod.Top.Pixels = (newRow.Height.Pixels / 2f) + 4f;
					newRow.Append(songMod);

					SelectionList.Add(newRow);
				}
				else {
					boxSlot = new MusicBoxSlot(data.MusicBox, 0.85f) {
						IsSelectionSlot = true
					};
					boxSlot.Left.Pixels = 20f + (boxSlot.Width.Pixels + 10f) * col;
					boxSlot.Top.Pixels = (newRow.Height.Pixels / 2f) - (boxSlot.Height.Pixels / 2f);
					newRow.Append(boxSlot);
					col++;
					if (col == 5) {
						row++;
						col = 0;
						SelectionList.Add(newRow);
						newRow = new ItemSlotRow(row);
					}
				}

				slotCount++;
			}

			if (col != 0)
				SelectionList.Add(newRow); // Add the last row if it is still incomplete

			SelectionList.SetScrollbar(selectionScrollBar);
		}

		private void ListViewPlaySong(int index) {
			if (index == -1 || Main.musicVolume <= 0f || !MusicUISystem.Instance.AllMusic[index].canPlay)
				return;

			int musicID = MusicUISystem.Instance.AllMusic[index].MusicID;
			if (playingMusic != musicID) {
				ListenDisplay = -1;
				listening = false;
				DisplayBox = index;
				playingMusic = musicID;
			}
			else {
				playingMusic = -1;
			}
		}

		private void ToggleButton(MusicMode type) {
			if (Main.musicVolume > 0f) {
				switch (type) {
					case MusicMode.Play:
						if (!listening) {
							playingMusic = (playingMusic == -1) ? MusicUISystem.Instance.AllMusic[DisplayBox].MusicID : -1;
							if (playingMusic != -1) {
								listening = false;
							}
							break;
						}
						return;
					case MusicMode.Listen:
						listening = !listening;
						if (listening) {
							playingMusic = -1;
						}
						else {
							recording = false;
						}
						break;
					case MusicMode.Record:
						recording = !recording;
						if (recording) {
							recording = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>().musicBoxesStored > 0;
						}
						if (recording) {
							playingMusic = -1;
							listening = true;
						}
						break;
				}
				if (tMusicPlayer.tMPConfig.EnableDebugMode) {
					string green = Utils.Hex3(Color.ForestGreen);
					string red = Utils.Hex3(Color.IndianRed);
					tMusicPlayer.SendDebugText($"[c/{(playingMusic > -1 ? green : red)}:Playing] - [c/{(listening ? green : red)}:Listening] - [c/{(recording ? green : red)}:Recording]");
				}
			}
		}

		public void SwapPanelSize() {
			smallPanel = !smallPanel;
			MusicPlayerPanel.UpdatePanelDimensions(smallPanel);
			BoxEntrySlot.Top.Pixels = searchBar.Top.Pixels;
			ejectButton.Top.Pixels = 42;

			MusicPlayerPanel.AddOrRemoveChild(detectButton, !smallPanel); // mini-mode does not contain these buttons
			MusicPlayerPanel.AddOrRemoveChild(recordButton, !smallPanel);
			MusicPlayerPanel.AddOrRemoveChild(prevButton, !smallPanel);
			MusicPlayerPanel.AddOrRemoveChild(nextButton, !smallPanel);

			expandButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - expandButton.Width.Pixels - 8f;
			viewButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - 20 - 8;
			playButton.Left.Pixels = (!smallPanel) ? (prevButton.Left.Pixels + playButton.Width.Pixels - 2f) : (MusicPlayerPanel.Width.Pixels - playButton.Width.Pixels - 6f);
			playButton.Top.Pixels = (!smallPanel) ? (MusicPlayerPanel.Height.Pixels - playButton.Height.Pixels - 4f) : (viewButton.Top.Pixels - playButton.Height.Pixels + 2f);
		}
	}

	public enum MusicMode {
		Play,
		Listen,
		Record
	}

	public enum SortBy {
		ID,
		Name
	}

	public enum ProgressBy {
		None,
		Obtained,
		Unobtained
	}
}
