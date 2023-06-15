using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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
				this.AddOrRemoveChild(musicEntryPanel, value);
				this.AddOrRemoveChild(selectionPanel, value);

				if (value) {
					musicEntryPanel.Left.Pixels = selectionPanel.Left.Pixels - musicEntryPanel.Width.Pixels + 4f;
					musicEntryPanel.Top.Pixels = selectionPanel.Top.Pixels + 10f;

					OrganizeSelection(sortType, availabililty, FilterMod); // refresh on open
				}
				else {
					// If not visible, clear search bar text
					if (searchBar.currentString != "")
						searchBar.currentString = "";
				}
			}
		}


		public BackDrop MusicPlayerPanel;
		public MusicBoxSlot DisplayMusicSlot;

		// Musicplayer buttons
		public HoverButton prevButton;
		public HoverButton playButton;
		public HoverButton nextButton;
		public HoverButton viewButton;
		public HoverButton detectButton;
		public HoverButton recordButton;
		public HoverButton expandButton;

		// Selection Panel Buttons
		public HoverButton ejectButton;
		public HoverButton favoritesButton;
		public HoverButton sortIDButton;
		public HoverButton sortNameButton;
		public HoverButton filterModButton;
		public HoverButton clearFilterModButton;
		public HoverButton availabilityButton;
		public HoverButton viewModeButton;

		public bool smallPanel = true;
		public bool listening = false;
		public bool recording = false;

		public int DisplayBox = 0;
		public int ListenDisplay = -1;
		public int playingMusic = -1;

		public BackDrop selectionPanel;
		public BackDrop musicEntryPanel;
		
		public SearchBar searchBar;
		public MusicBoxSlot AddMusicBoxSlot;
		public HoverButton closeButton;
		public UIList SelectionList;
		public FixedUIScrollbar selectionScrollBar;
		public MusicBoxSlot[] SelectionSlots;
		internal bool[] canPlay;
		internal bool viewMode = false;
		internal bool viewFavs = false;
		
		public static Asset<Texture2D>[] panelTextures;
		public static Asset<Texture2D> buttonTextures;
		public static Asset<Texture2D> closeTextures;

		public SortBy sortType = SortBy.ID;
		public ProgressBy availabililty = ProgressBy.None;
		public string FilterMod = "";
		internal List<string> ModList;

		internal List<MusicData> musicData;
		
		public override void OnInitialize() {
			panelTextures = new Asset<Texture2D>[4] {
				TextureAssets.MagicPixel,
				ModContent.Request<Texture2D>("tMusicPlayer/UI/backdrop", AssetRequestMode.ImmediateLoad),
				ModContent.Request<Texture2D>("tMusicPlayer/UI/backdrop2", AssetRequestMode.ImmediateLoad),
				ModContent.Request<Texture2D>("tMusicPlayer/UI/backdrop3", AssetRequestMode.ImmediateLoad)
			};
			
			buttonTextures = ModContent.Request<Texture2D>("tMusicPlayer/UI/buttons", AssetRequestMode.ImmediateLoad);
			closeTextures = ModContent.Request<Texture2D>("tMusicPlayer/UI/close", AssetRequestMode.ImmediateLoad);

			MusicPlayerPanel = new BackDrop() {
				Id = "MusicPlayerPanel",
			};
			MusicPlayerPanel.Width.Pixels = panelTextures[1].Value.Width;
			MusicPlayerPanel.Height.Pixels = panelTextures[1].Value.Height;
			MusicPlayerPanel.Left.Pixels = 1115f;
			MusicPlayerPanel.Top.Pixels = 16f;

			prevButton = new HoverButton(buttonTextures.Value, new Rectangle(0, 0, 22, 22)) {
				Id = "prev"
			};
			prevButton.Width.Pixels = 22f;
			prevButton.Height.Pixels = 22f;
			prevButton.Left.Pixels = 100f;
			prevButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - prevButton.Height.Pixels - 4f;
			prevButton.OnLeftClick += (a, b) => ChangeDisplay(false);
			prevButton.OnRightClick += (a, b) => ChangeDisplay(false, true);

			playButton = new HoverButton(buttonTextures.Value, new Rectangle(24, 0, 22, 22)) {
				Id = "play"
			};
			playButton.Width.Pixels = 22f;
			playButton.Height.Pixels = 22f;
			playButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - playButton.Width.Pixels - 6f;
			playButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - playButton.Height.Pixels * 2f - 4f;
			playButton.OnLeftClick += (a, b) => ToggleButton(MusicMode.Play);
			MusicPlayerPanel.Append(playButton);

			nextButton = new HoverButton(buttonTextures.Value, new Rectangle(72, 0, 22, 22)) {
				Id = "next"
			};
			nextButton.Width.Pixels = 22f;
			nextButton.Height.Pixels = 22f;
			nextButton.Left.Pixels = playButton.Left.Pixels + nextButton.Width.Pixels - 2f;
			nextButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - nextButton.Height.Pixels - 4f;
			nextButton.OnLeftClick += (a, b) => ChangeDisplay(true);
			nextButton.OnRightClick += (a, b) => ChangeDisplay(true, true);

			viewButton = new HoverButton(buttonTextures.Value, new Rectangle(144, 0, 22, 22)) {
				Id = "view"
			};
			viewButton.Width.Pixels = 22f;
			viewButton.Height.Pixels = 22f;
			viewButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - viewButton.Width.Pixels - 6f;
			viewButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - viewButton.Height.Pixels - 4f;
			viewButton.OnLeftClick += (a, b) => SelectionPanelVisible = !SelectionPanelVisible;
			MusicPlayerPanel.Append(viewButton);

			detectButton = new HoverButton(buttonTextures.Value, new Rectangle(96, 0, 22, 22)) {
				Id = "listen"
			};
			detectButton.Width.Pixels = 22f;
			detectButton.Height.Pixels = 22f;
			detectButton.Left.Pixels = viewButton.Left.Pixels - detectButton.Width.Pixels - 2f;
			detectButton.Top.Pixels = viewButton.Top.Pixels;
			detectButton.OnLeftClick += (a, b) => ToggleButton(MusicMode.Listen);

			recordButton = new HoverButton(buttonTextures.Value, new Rectangle(168, 0, 22, 22)) {
				Id = "record"
			};
			recordButton.Width.Pixels = 22f;
			recordButton.Height.Pixels = 22f;
			recordButton.Left.Pixels = detectButton.Left.Pixels - recordButton.Width.Pixels - 4f;
			recordButton.Top.Pixels = viewButton.Top.Pixels;
			recordButton.OnLeftClick += (a, b) => ToggleButton(MusicMode.Record);

			expandButton = new HoverButton(closeTextures.Value, new Rectangle(20, 0, 18, 18)) {
				Id = "expand"
			};
			expandButton.Width.Pixels = 18f;
			expandButton.Height.Pixels = 18f;
			expandButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - expandButton.Width.Pixels - 8f;
			expandButton.Top.Pixels = 4f;
			expandButton.OnLeftClick += (a, b) => SwapPanelSize();
			MusicPlayerPanel.Append(expandButton);
			
			DisplayMusicSlot = new MusicBoxSlot(0, 1f) {
				Id = "DisplaySlot"
			};
			DisplayMusicSlot.Left.Pixels = 8f;
			DisplayMusicSlot.Top.Pixels = MusicPlayerPanel.Height.Pixels / 2f - TextureAssets.InventoryBack.Value.Height / 2;
			MusicPlayerPanel.Append(DisplayMusicSlot);

			selectionPanel = new BackDrop() {
				Id = "SelectionPanel"
			};
			selectionPanel.Width.Pixels = panelTextures[3].Value.Width;
			selectionPanel.Height.Pixels = panelTextures[3].Value.Height;
			selectionPanel.Left.Pixels = (Main.screenWidth / 2) - selectionPanel.Width.Pixels / 2f;
			selectionPanel.Top.Pixels = (Main.screenHeight / 2) - selectionPanel.Height.Pixels / 2f;

			// Positioning base for filter buttons
			float center = (selectionPanel.Width.Pixels / 2) - 11;

			favoritesButton = new HoverButton(buttonTextures.Value, new Rectangle(7 * 24, 48, 22, 22)) {
				Id = "showFavorites"
			};
			favoritesButton.Width.Pixels = 22f;
			favoritesButton.Height.Pixels = 22f;
			favoritesButton.Left.Pixels = center - (20 * 3) - (8 * 2) + 4;
			favoritesButton.Top.Pixels = 42;
			favoritesButton.OnLeftClick += (a, b) => OrganizeSelection(sortType, availabililty, FilterMod, false, true);
			selectionPanel.Append(favoritesButton);

			sortIDButton = new HoverButton(buttonTextures.Value, new Rectangle(0 * 24, 48, 22, 22)) {
				Id = "sortbyid"
			};
			sortIDButton.Width.Pixels = 22f;
			sortIDButton.Height.Pixels = 22f;
			sortIDButton.Left.Pixels = center - (20 * 2) - 8 + 4;
			sortIDButton.Top.Pixels = 42;
			sortIDButton.OnLeftClick += (a, b) => OrganizeSelection(SortBy.ID, availabililty, FilterMod);
			selectionPanel.Append(sortIDButton);

			sortNameButton = new HoverButton(buttonTextures.Value, new Rectangle(1 * 24, 48, 22, 22)) {
				Id = "sortbyname"
			};
			sortNameButton.Width.Pixels = 22f;
			sortNameButton.Height.Pixels = 22f;
			sortNameButton.Left.Pixels = center - 20 - 8 + 4;
			sortNameButton.Top.Pixels = 42;
			sortNameButton.OnLeftClick += (a, b) => OrganizeSelection(SortBy.Name, availabililty, FilterMod);
			selectionPanel.Append(sortNameButton);

			filterModButton = new HoverButton(buttonTextures.Value, new Rectangle(2 * 24, 48, 22, 22)) {
				Id = "filtermod"
			};
			filterModButton.Width.Pixels = 22f;
			filterModButton.Height.Pixels = 22f;
			filterModButton.Left.Pixels = center + 8 - 4;
			filterModButton.Top.Pixels = 42;
			filterModButton.OnLeftClick += (a, b) => OrganizeSelection(sortType, availabililty, UpdateModFilter(true));
			filterModButton.OnRightClick += (a, b) => OrganizeSelection(sortType, availabililty, UpdateModFilter(false));
			selectionPanel.Append(filterModButton);

			clearFilterModButton = new HoverButton(buttonTextures.Value, new Rectangle(3 * 24, 48, 22, 22)) {
				Id = "clearfiltermod"
			};
			clearFilterModButton.Width.Pixels = 22f;
			clearFilterModButton.Height.Pixels = 22f;
			clearFilterModButton.Left.Pixels = center + 20 + 8 - 4;
			clearFilterModButton.Top.Pixels = 42;
			clearFilterModButton.OnLeftClick += (a, b) => OrganizeSelection(sortType, availabililty, ResetModFilter());
			selectionPanel.Append(clearFilterModButton);

			availabilityButton = new HoverButton(buttonTextures.Value, new Rectangle(4 * 24, 48, 22, 22)) {
				Id = "availability"
			};
			availabilityButton.Width.Pixels = 22f;
			availabilityButton.Height.Pixels = 22f;
			availabilityButton.Left.Pixels = center + (20 * 2) + (8 * 2) - 4;
			availabilityButton.Top.Pixels = 42;
			availabilityButton.OnLeftClick += (a, b) => OrganizeSelection(sortType, UpdateAvailabilityFilter(true), FilterMod);
			availabilityButton.OnRightClick += (a, b) => OrganizeSelection(sortType, UpdateAvailabilityFilter(false), FilterMod);
			selectionPanel.Append(availabilityButton);

			closeButton = new HoverButton(closeTextures.Value, new Rectangle(0, 0, 18, 18)) {
				Id = "select_close"
			};
			closeButton.Width.Pixels = 18f;
			closeButton.Height.Pixels = 18f;
			closeButton.Left.Pixels = selectionPanel.Width.Pixels - closeButton.Width.Pixels - 11f;
			closeButton.Top.Pixels = 12f;
			closeButton.OnLeftClick += (a, b) => SelectionPanelVisible = !SelectionPanelVisible;
			selectionPanel.Append(closeButton);

			viewModeButton = new HoverButton(buttonTextures.Value, new Rectangle(0 * 24, 96, 22, 22)) {
				Id = "viewmode"
			};
			viewModeButton.Width.Pixels = 22f;
			viewModeButton.Height.Pixels = 22f;
			viewModeButton.Left.Pixels = selectionPanel.Width.Pixels - closeButton.Width.Pixels - 13f;
			viewModeButton.Top.Pixels = closeButton.Top.Pixels + closeButton.Height.Pixels + 4f;
			viewModeButton.OnLeftClick += (a, b) => UpdateViewMode();
			selectionPanel.Append(viewModeButton);

			searchBar = new SearchBar("Search...", "");
			searchBar.Width.Pixels = 216f;
			searchBar.Height.Pixels = 28f;
			searchBar.Top.Pixels = 9f;
			searchBar.Left.Pixels = 12f;
			selectionPanel.Append(searchBar);

			musicEntryPanel = new BackDrop() {
				Id = "MusicEntry"
			};
			musicEntryPanel.Width.Pixels = panelTextures[1].Value.Height;
			musicEntryPanel.Height.Pixels = panelTextures[1].Value.Width;
			musicEntryPanel.Left.Pixels = selectionPanel.Left.Pixels - musicEntryPanel.Width.Pixels + 4f;
			musicEntryPanel.Top.Pixels = selectionPanel.Top.Pixels + 10f;

			ejectButton = new HoverButton(buttonTextures.Value, new Rectangle(8 * 24, 48, 22, 22)) {
				Id = "ejectMusicBoxes"
			};
			ejectButton.Width.Pixels = 22f;
			ejectButton.Height.Pixels = 22f;
			ejectButton.Left.Pixels = 10;
			ejectButton.Top.Pixels = 63;
			ejectButton.OnLeftClick += (a, b) => EjectBox(false);
			ejectButton.OnRightClick += (a, b) => EjectBox(true);
			musicEntryPanel.Append(ejectButton);

			AddMusicBoxSlot = new MusicBoxSlot(ItemID.MusicBox, 0.85f) {
				Id = "EntrySlot"
			};
			AddMusicBoxSlot.Left.Pixels = (musicEntryPanel.Width.Pixels / 2) - (AddMusicBoxSlot.Width.Pixels / 2) + 1;
			AddMusicBoxSlot.Top.Pixels = 8f;
			musicEntryPanel.Append(AddMusicBoxSlot);

			selectionScrollBar = new FixedUIScrollbar();
			selectionScrollBar.SetView(10f, 1000f);
			selectionScrollBar.Top.Pixels = 76f;
			selectionScrollBar.Left.Pixels = -10f;
			selectionScrollBar.Height.Set(0f, 0.75f);
			selectionScrollBar.HAlign = 1f;
			selectionPanel.Append(selectionScrollBar);

			SelectionList = new UIList();
			SelectionList.Width.Pixels = selectionPanel.Width.Pixels;
			SelectionList.Height.Pixels = selectionPanel.Height.Pixels - 85f;
			SelectionList.Left.Pixels = 0f;
			SelectionList.Top.Pixels = 72f;
			selectionPanel.Append(SelectionList);
		}

		public override void Update(GameTime gameTime) {
			// This code mimics the "Music Box Recording" process.
			// Check if we have music boxes at the ready, if the player is in record mode and music is currently playing.
			// If all of those apply, we also go a rand check which will trigger the "recording" code.
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();

			if (modplayer.musicBoxesStored > 0 && recording && Main.curMusic > 0 && Main.rand.NextBool(540)) {
				int index = MusicUISystem.Instance.AllMusic.FindIndex(x => x.music == Main.curMusic); // Make sure curMusic is a music box.
				if (index != -1) {
					MusicData musicData = MusicUISystem.Instance.AllMusic[index];
                    SoundEngine.PlaySound(SoundID.Item166);
					if (!modplayer.BoxIsCollected(musicData.musicbox)) {
						// If we don't have it in our music player, automatically add it in.
						modplayer.MusicBoxList.Add(new ItemDefinition(musicData.musicbox));
						canPlay[index] = true; // as soon as it is recorded, the player should be able to play the music
					}
					else {
						// If we do have it already, spawn the item.
						player.QuickSpawnItem(player.GetSource_OpenItem(musicData.musicbox), musicData.musicbox);
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
			
			if (!listening && !canPlay.Contains(true)) {
				ToggleButton(MusicMode.Listen); // If nothing can be played, turn on 'listening mode'
			}
		}

		public int FindNextIndex() {
			int index = musicData.FindIndex(x => x.music == MusicUISystem.Instance.AllMusic[DisplayBox].music);
			for (int i = index; i < musicData.Count; i++) {
				if (i != index && canPlay[DisplayBox])
					return i;
			}
			return -1;
		}

		public int FindPrevIndex() {
			int index = musicData.FindIndex(x => x.music == MusicUISystem.Instance.AllMusic[DisplayBox].music);
			for (int i = index; i >= 0; i--) {
				if (i != index && canPlay[DisplayBox])
					return i;
			}
			return -1;
		}

		private void ChangeDisplay(bool next, bool jumpToEnd = false) {
			if (!listening) {
				int newIndex = next ? FindNextIndex() : FindPrevIndex();
				if (newIndex != -1) { 
					DisplayBox = MusicUISystem.Instance.AllMusic.FindIndex(x => x.music == musicData[newIndex].music);
					if (playingMusic > -1) {
						playingMusic = MusicUISystem.Instance.AllMusic[DisplayBox].music;
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
			OrganizeSelection(sortType, availabililty, FilterMod);
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

		internal void OrganizeSelection(SortBy sortBy, ProgressBy progressBy, string filterMod, bool initializing = false, bool clickedFavorites = false) {
			sortType = sortBy;
			availabililty = progressBy;
			FilterMod = filterMod;

			if (clickedFavorites) {
				viewFavs = !viewFavs;
			}

			int displayMusicID = MusicUISystem.Instance.AllMusic[DisplayBox].music;
			if (sortBy == SortBy.ID) {
				musicData = musicData.OrderBy(x => x.music).ToList();
			}
			if (sortBy == SortBy.Name) {
				musicData = musicData.OrderBy(x => x.name).ToList();
			}

			DisplayBox = MusicUISystem.Instance.AllMusic.FindIndex(x => x.music == displayMusicID);
			
			SelectionList.Clear();
			
			if (!viewMode) {
				// Current view mode is GRID
				ItemSlotRow newRow = new ItemSlotRow(0, 400, 50);
				int col = 0;
				int row = 0;
				for (int i = 0; i < musicData.Count; i++) {
					// Filter checks do not happen when initializing
					// Include all music boxes if FilterMod is left empty
					// Otherwise find music boxes with the same mod name as the selected filter mod
					// If Availability isn't 'None' check if the box is obtained or not
					if (!initializing) {
						MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
						bool CheckFilterMod = filterMod != "" && (musicData[i].mod != filterMod);
						bool CheckObtained = progressBy == ProgressBy.Obtained && !modplayer.BoxIsCollected(musicData[i].musicbox);
						bool CheckUnobtained = progressBy == ProgressBy.Unobtained && modplayer.BoxIsCollected(musicData[i].musicbox);
						bool CheckFavorited = viewFavs && !modplayer.BoxIsFavorited(musicData[i].musicbox);

						if (CheckFilterMod || CheckObtained || CheckUnobtained || CheckFavorited) {
							continue;
						}
					}

					SelectionSlots[i] = new MusicBoxSlot(musicData[i].musicbox, 0.85f);
					SelectionSlots[i].Left.Pixels = 20f + (SelectionSlots[i].Width.Pixels + 10f) * col;
					SelectionSlots[i].Top.Pixels = (newRow.Height.Pixels / 2f) - (SelectionSlots[i].Height.Pixels / 2f);
					SelectionSlots[i].Id = $"SelectionSlotGrid_{i}";
					newRow.Append(SelectionSlots[i]);
					col++;
					if (col == 5) {
						row++;
						col = 0;
						SelectionList.Add(newRow);
						newRow = new ItemSlotRow(row, 400, 50);
					}
				}
				if (col != 0) {
					// Add the last row if we did not complete it
					SelectionList.Add(newRow);
				}
			}
			else {
				// Current view mode is LIST
				ItemSlotRow newRow;
				for (int i = 0; i < musicData.Count; i++) {
					// Include all music boxes if FilterMod is left empty
					// Otherwise find music boxes with the same mod name as the selected filter mod
					// If Availability isn't 'None' check if the box is obtained or not
					if (!initializing) {
						MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
						bool CheckFilterMod = filterMod != "" && (musicData[i].mod != filterMod);
						bool CheckObtained = progressBy == ProgressBy.Obtained && !modplayer.BoxIsCollected(musicData[i].musicbox);
						bool CheckUnobtained = progressBy == ProgressBy.Unobtained && modplayer.BoxIsCollected(musicData[i].musicbox);
						bool CheckFavorited = viewFavs && !modplayer.BoxIsCollected(musicData[i].musicbox);

						if (CheckFilterMod || CheckObtained || CheckUnobtained || CheckFavorited) {
							continue;
						}
					}

					newRow = new ItemSlotRow(i, panelTextures[2].Value.Bounds.Width, panelTextures[2].Value.Bounds.Height);

					// Item Slot
					SelectionSlots[i] = new MusicBoxSlot(musicData[i].musicbox, 0.85f);
					SelectionSlots[i].Left.Pixels = 20f;
					SelectionSlots[i].Top.Pixels = (newRow.Height.Pixels / 2f) - (SelectionSlots[i].Height.Pixels / 2f);
					SelectionSlots[i].Id = $"SelectionSlotList_{i}";
					newRow.Append(SelectionSlots[i]);
					
					// Play button
					HoverButton playSong = new HoverButton(buttonTextures.Value, new Rectangle(24, 0, 22, 22)) {
						Id = "altplay",
						refNum = musicData[i].music
					};
					playSong.Width.Pixels = 22f;
					playSong.Height.Pixels = 22f;
					playSong.Left.Pixels = SelectionSlots[i].Left.Pixels + SelectionSlots[i].Width.Pixels + 8f;
					playSong.Top.Pixels = (newRow.Height.Pixels / 2f) - (playSong.Height.Pixels / 2f);
					playSong.OnLeftClick += (a, b) => ListViewPlaySong(playSong.Id);
					newRow.Append(playSong);

					// Song name and mod
					UIText songName = new UIText(musicData[i].name, 0.85f);
					songName.Left.Pixels = playSong.Left.Pixels + playSong.Width.Pixels + 8f;
					songName.Top.Pixels = (newRow.Height.Pixels / 2f) - 15f;
					newRow.Append(songName);

					UIText songMod = new UIText(musicData[i].mod, 0.85f);
					songMod.Left.Pixels = playSong.Left.Pixels + playSong.Width.Pixels + 8f;
					songMod.Top.Pixels = (newRow.Height.Pixels / 2f) + 4f;
					newRow.Append(songMod);

					SelectionList.Add(newRow);
				}
			}

			SelectionList.SetScrollbar(selectionScrollBar);
		}

		private void ListViewPlaySong(string Id) {
			int musicID = Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1));
			int index = MusicUISystem.Instance.AllMusic.FindIndex(x => x.music == musicID);
			if (!canPlay[index] || Main.musicVolume <= 0f) {
				return;
			}

			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			if (UI.playingMusic != musicID) {
				UI.ListenDisplay = -1;
				UI.listening = false;
				UI.DisplayBox = index;
				UI.playingMusic = musicID;
			}
			else {
				UI.playingMusic = -1;
			}
		}

		private void ToggleButton(MusicMode type) {
			if (Main.musicVolume > 0f) {
				switch (type) {
					case MusicMode.Play:
						if (!listening) {
							playingMusic = (playingMusic == -1) ? MusicUISystem.Instance.AllMusic[DisplayBox].music : -1;
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

			Texture2D size = smallPanel ? panelTextures[1].Value : panelTextures[2].Value;
			MusicPlayerPanel.Width.Pixels = size.Width;
			MusicPlayerPanel.Height.Pixels = size.Height;

			expandButton.Left.Pixels = size.Width - expandButton.Width.Pixels - 8f;
			viewButton.Left.Pixels = (size.Width - 20 - 8);
			detectButton.Left.Pixels = viewButton.Left.Pixels - detectButton.Width.Pixels - 2f;
			MusicPlayerPanel.AddOrRemoveChild(detectButton, !smallPanel);

			recordButton.Left.Pixels = detectButton.Left.Pixels - recordButton.Width.Pixels - 2f;
			MusicPlayerPanel.AddOrRemoveChild(recordButton, !smallPanel);
			MusicPlayerPanel.AddOrRemoveChild(prevButton, !smallPanel);

			playButton.Left.Pixels = (!smallPanel) ? (prevButton.Left.Pixels + playButton.Width.Pixels - 2f) : (size.Width - playButton.Width.Pixels - 6f);
			playButton.Top.Pixels = (!smallPanel) ? (size.Height - playButton.Height.Pixels - 4f) : (viewButton.Top.Pixels - playButton.Height.Pixels + 2f);

			nextButton.Left.Pixels = playButton.Left.Pixels + nextButton.Width.Pixels - 2f;
			MusicPlayerPanel.AddOrRemoveChild(nextButton, !smallPanel);
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
