using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
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
		public BackDrop MusicPlayerPanel;
		public bool mpToggleVisibility = true;
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
		public HoverButton sortIDButton;
		public HoverButton sortNameButton;
		public HoverButton filterModButton;
		public HoverButton clearFilterModButton;
		public HoverButton clearAvailabilityButton;
		public HoverButton obtainedButton;
		public HoverButton unobtainedButton;
		public HoverButton viewModeButton;

		public bool smallPanel = true;
		public bool listening = false;
		public bool recording = false;

		public int DisplayBox = 0;
		public int ListenDisplay = -1;
		public int playingMusic = -1;

		public BackDrop selectionPanel;
		public BackDrop musicEntryPanel;
		
		public NewUITextBox searchBar;
		public MusicBoxSlot AddMusicBoxSlot;
		public ListenStorageSlot MusicStorageSlot;
		public HoverButton closeButton;
		public UIList SelectionList;
		public FixedUIScrollbar selectionScrollBar;
		public MusicBoxSlot[] SelectionSlots;
		internal List<int> canPlay;
		internal bool viewMode = false;

		public bool selectionVisible = false;
		
		public static Texture2D[] panelTextures;
		public static Texture2D buttonTextures;
		public static Texture2D closeTextures;

		public SortBy sortType = SortBy.ID;
		public ProgressBy availabililty = ProgressBy.None;
		public string FilterMod = "";
		internal List<string> ModList;

		internal List<MusicData> musicData;
		
		public override void OnInitialize()
		{
			panelTextures = new Texture2D[4] {
				CropTexture(ModContent.GetTexture($"tMusicPlayer/UI/backdrop"), new Rectangle(0, 0, 1, 1)),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop"),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop2"),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop3")
			};
			
			buttonTextures = ModContent.GetTexture("tMusicPlayer/UI/buttons");
			closeTextures = ModContent.GetTexture("tMusicPlayer/UI/close");

			MusicPlayerPanel = new BackDrop(panelTextures[0]) {
				Id = "MusicPlayerPanel"
			};
			MusicPlayerPanel.Width.Pixels = panelTextures[1].Width;
			MusicPlayerPanel.Height.Pixels = panelTextures[1].Height;
			MusicPlayerPanel.Left.Pixels = 500f;
			MusicPlayerPanel.Top.Pixels = 6f;

			prevButton = new HoverButton(buttonTextures, new Rectangle(0, 0, 22, 22)) {
				Id = "prev"
			};
			prevButton.Width.Pixels = 22f;
			prevButton.Height.Pixels = 22f;
			prevButton.Left.Pixels = 100f;
			prevButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - prevButton.Height.Pixels - 4f;
			prevButton.OnClick += (a, b) => ChangeDisplay(false);
			prevButton.OnRightClick += (a, b) => ChangeDisplay(false, true);

			playButton = new HoverButton(buttonTextures, new Rectangle(24, 0, 22, 22)) {
				Id = "play"
			};
			playButton.Width.Pixels = 22f;
			playButton.Height.Pixels = 22f;
			playButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - playButton.Width.Pixels - 6f;
			playButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - playButton.Height.Pixels * 2f - 4f;
			playButton.OnClick += (a, b) => ToggleButton(MusicMode.Play);
			MusicPlayerPanel.Append(playButton);

			nextButton = new HoverButton(buttonTextures, new Rectangle(72, 0, 22, 22)) {
				Id = "next"
			};
			nextButton.Width.Pixels = 22f;
			nextButton.Height.Pixels = 22f;
			nextButton.Left.Pixels = playButton.Left.Pixels + nextButton.Width.Pixels - 2f;
			nextButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - nextButton.Height.Pixels - 4f;
			nextButton.OnClick += (a, b) => ChangeDisplay(true);
			nextButton.OnRightClick += (a, b) => ChangeDisplay(true, true);

			viewButton = new HoverButton(buttonTextures, new Rectangle(144, 0, 22, 22)) {
				Id = "view"
			};
			viewButton.Width.Pixels = 22f;
			viewButton.Height.Pixels = 22f;
			viewButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - viewButton.Width.Pixels - 6f;
			viewButton.Top.Pixels = MusicPlayerPanel.Height.Pixels - viewButton.Height.Pixels - 4f;
			viewButton.OnClick += (a, b) => selectionVisible = true;
			MusicPlayerPanel.Append(viewButton);

			detectButton = new HoverButton(buttonTextures, new Rectangle(96, 0, 22, 22)) {
				Id = "listen"
			};
			detectButton.Width.Pixels = 22f;
			detectButton.Height.Pixels = 22f;
			detectButton.Left.Pixels = viewButton.Left.Pixels - detectButton.Width.Pixels - 2f;
			detectButton.Top.Pixels = viewButton.Top.Pixels;
			detectButton.OnClick += (a, b) => ToggleButton(MusicMode.Listen);

			recordButton = new HoverButton(buttonTextures, new Rectangle(168, 0, 22, 22)) {
				Id = "record"
			};
			recordButton.Width.Pixels = 22f;
			recordButton.Height.Pixels = 22f;
			recordButton.Left.Pixels = detectButton.Left.Pixels - recordButton.Width.Pixels - 4f;
			recordButton.Top.Pixels = viewButton.Top.Pixels;
			recordButton.OnClick += (a, b) => ToggleButton(MusicMode.Record);

			expandButton = new HoverButton(closeTextures, new Rectangle(20, 0, 18, 18)) {
				Id = "expand"
			};
			expandButton.Width.Pixels = 18f;
			expandButton.Height.Pixels = 18f;
			expandButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - expandButton.Width.Pixels - 8f;
			expandButton.Top.Pixels = 4f;
			expandButton.OnClick += (a, b) => SwapPanelSize();
			MusicPlayerPanel.Append(expandButton);
			
			DisplayMusicSlot = new MusicBoxSlot(0, 1f) {
				Id = "DisplaySlot"
			};
			DisplayMusicSlot.Left.Pixels = 8f;
			DisplayMusicSlot.Top.Pixels = MusicPlayerPanel.Height.Pixels / 2f - (Main.inventoryBackTexture.Height / 2);
			MusicPlayerPanel.Append(DisplayMusicSlot);

			selectionPanel = new BackDrop(panelTextures[0]) {
				Id = "SelectionPanel"
			};
			selectionPanel.Width.Pixels = panelTextures[3].Width;
			selectionPanel.Height.Pixels = panelTextures[3].Height;
			selectionPanel.Left.Pixels = (Main.screenWidth / 2) - selectionPanel.Width.Pixels / 2f;
			selectionPanel.Top.Pixels = (Main.screenHeight / 2) - selectionPanel.Height.Pixels / 2f;

			// Positioning base for filter buttons
			float center = (selectionPanel.Width.Pixels / 2) - 11;

			sortIDButton = new HoverButton(buttonTextures, new Rectangle(0 * 24, 48, 22, 22)) {
				Id = "sortid"
			};
			sortIDButton.Width.Pixels = 22f;
			sortIDButton.Height.Pixels = 22f;
			sortIDButton.Left.Pixels = center - (20 * 3) - 8;
			sortIDButton.Top.Pixels = 42;
			sortIDButton.OnClick += (a, b) => OrganizeSelection(SortBy.ID, availabililty, FilterMod);
			selectionPanel.Append(sortIDButton);

			sortNameButton = new HoverButton(buttonTextures, new Rectangle(1 * 24, 48, 22, 22)) {
				Id = "sortname"
			};
			sortNameButton.Width.Pixels = 22f;
			sortNameButton.Height.Pixels = 22f;
			sortNameButton.Left.Pixels = center - (20 * 2) - 8;
			sortNameButton.Top.Pixels = 42;
			sortNameButton.OnClick += (a, b) => OrganizeSelection(SortBy.Name, availabililty, FilterMod);
			selectionPanel.Append(sortNameButton);

			filterModButton = new HoverButton(buttonTextures, new Rectangle(2 * 24, 48, 22, 22)) {
				Id = "filtermod"
			};
			filterModButton.Width.Pixels = 22f;
			filterModButton.Height.Pixels = 22f;
			filterModButton.Left.Pixels = center - (20 * 1);
			filterModButton.Top.Pixels = 42;
			filterModButton.OnClick += (a, b) => OrganizeSelection(sortType, availabililty, UpdateModFilter(true));
			filterModButton.OnRightClick += (a, b) => OrganizeSelection(sortType, availabililty, UpdateModFilter(false));
			selectionPanel.Append(filterModButton);

			clearFilterModButton = new HoverButton(buttonTextures, new Rectangle(3 * 24, 48, 22, 22)) {
				Id = "clearfiltermod"
			};
			clearFilterModButton.Width.Pixels = 22f;
			clearFilterModButton.Height.Pixels = 22f;
			clearFilterModButton.Left.Pixels = center;
			clearFilterModButton.Top.Pixels = 42;
			clearFilterModButton.OnClick += (a, b) => OrganizeSelection(sortType, availabililty, ResetModFilter());
			selectionPanel.Append(clearFilterModButton);

			clearAvailabilityButton = new HoverButton(buttonTextures, new Rectangle(4 * 24, 48, 22, 22)) {
				Id = "clearavailability"
			};
			clearAvailabilityButton.Width.Pixels = 22f;
			clearAvailabilityButton.Height.Pixels = 22f;
			clearAvailabilityButton.Left.Pixels = center + (20 * 1) + 8;
			clearAvailabilityButton.Top.Pixels = 42;
			clearAvailabilityButton.OnClick += (a, b) => OrganizeSelection(sortType, ProgressBy.None, FilterMod);
			selectionPanel.Append(clearAvailabilityButton);

			obtainedButton = new HoverButton(buttonTextures, new Rectangle(5 * 24, 48, 22, 22)) {
				Id = "obtainedavailability"
			};
			obtainedButton.Width.Pixels = 22f;
			obtainedButton.Height.Pixels = 22f;
			obtainedButton.Left.Pixels = center + (20 * 2) + 8;
			obtainedButton.Top.Pixels = 42;
			obtainedButton.OnClick += (a, b) => OrganizeSelection(sortType, ProgressBy.Obtained, FilterMod);
			selectionPanel.Append(obtainedButton);

			unobtainedButton = new HoverButton(buttonTextures, new Rectangle(6 * 24, 48, 22, 22)) {
				Id = "unobtainedavailability"
			};
			unobtainedButton.Width.Pixels = 22f;
			unobtainedButton.Height.Pixels = 22f;
			unobtainedButton.Left.Pixels = center + (20 * 3) + 8;
			unobtainedButton.Top.Pixels = 42;
			unobtainedButton.OnClick += (a, b) => OrganizeSelection(sortType, ProgressBy.Unobtained, FilterMod);
			selectionPanel.Append(unobtainedButton);

			closeButton = new HoverButton(closeTextures, new Rectangle(0, 0, 18, 18)) {
				Id = "select_close"
			};
			closeButton.Width.Pixels = 18f;
			closeButton.Height.Pixels = 18f;
			closeButton.Left.Pixels = selectionPanel.Width.Pixels - closeButton.Width.Pixels - 11f;
			closeButton.Top.Pixels = 12f;
			closeButton.OnClick += (a, b) => selectionVisible = !selectionVisible;
			selectionPanel.Append(closeButton);

			viewModeButton = new HoverButton(buttonTextures, new Rectangle(7 * 24, 48, 22, 22)) {
				Id = "viewmode"
			};
			viewModeButton.Width.Pixels = 22f;
			viewModeButton.Height.Pixels = 22f;
			viewModeButton.Left.Pixels = selectionPanel.Width.Pixels - closeButton.Width.Pixels - 13f;
			viewModeButton.Top.Pixels = closeButton.Top.Pixels + closeButton.Height.Pixels + 4f;
			viewModeButton.OnClick += (a, b) => UpdateViewMode();
			selectionPanel.Append(viewModeButton);

			musicEntryPanel = new BackDrop(panelTextures[0]) {
				Id = "MusicEntry"
			};
			musicEntryPanel.Width.Pixels = panelTextures[1].Height;
			musicEntryPanel.Height.Pixels = panelTextures[1].Width;
			musicEntryPanel.Left.Pixels = selectionPanel.Left.Pixels - musicEntryPanel.Width.Pixels + 10f;
			musicEntryPanel.Top.Pixels = selectionPanel.Top.Pixels + 10f;

			searchBar = new NewUITextBox("Search...", "");
			searchBar.Width.Pixels = 216f;
			searchBar.Height.Pixels = 28f;
			searchBar.Top.Pixels = 9f;
			searchBar.Left.Pixels = 12f;
			selectionPanel.Append(searchBar);

			AddMusicBoxSlot = new MusicBoxSlot(ItemID.MusicBox, 0.85f) {
				Id = "EntrySlot"
			};
			AddMusicBoxSlot.Left.Pixels = (musicEntryPanel.Width.Pixels / 2) - (AddMusicBoxSlot.Width.Pixels / 2);
			AddMusicBoxSlot.Top.Pixels = 8f;
			musicEntryPanel.Append(AddMusicBoxSlot);

			MusicStorageSlot = new ListenStorageSlot(Main.itemTexture[ItemID.MusicBox], ItemID.MusicBox);
			MusicStorageSlot.Left.Pixels = (musicEntryPanel.Width.Pixels / 2) - (MusicStorageSlot.Width.Pixels / 2);
			MusicStorageSlot.Top.Pixels = AddMusicBoxSlot.Height.Pixels + 16f;
			musicEntryPanel.Append(MusicStorageSlot);

			selectionScrollBar = new FixedUIScrollbar();
			selectionScrollBar.SetView(100f, 1000f);
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

			canPlay = new List<int>();
		}

		public override void Update(GameTime gameTime)
		{
			// This code mimics the "Music Box Recording" process.
			// Check if we have music boxes at the ready, if the player is in record mode and music is currently playing.
			// If all of those apply, we also go a rand check which will trigger the "recording" code.
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();
			if (modplayer.musicBoxesStored > 0 && tMusicPlayer.MusicPlayerUI.recording && Main.curMusic > 0 && Main.rand.Next(2700) == 0) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.music == Main.curMusic); // Make sure curMusic is a music box.
				if (index != -1) {
					int musicBoxType = tMusicPlayer.AllMusic[index].musicbox;					Main.PlaySound(SoundLoader.customSoundType, player.position, ModLoader.GetMod("tMusicPlayer").GetSoundSlot(SoundType.Custom, "Sounds/Custom/recorded")); // TODO: [1.4] Proper PlaySound
					if (modplayer.MusicBoxList.All(x => x.Type != musicBoxType)) {
						// If we don't have it in our music player, automatically add it in.
						modplayer.MusicBoxList.Add(new ItemDefinition(musicBoxType));
					}
					else {
						// If we do have it already, spawn the item.
						player.QuickSpawnItem(musicBoxType);
					}
					tMusicPlayer.SendDebugText($"Music Box ({tMusicPlayer.AllMusic[index].name}) obtained!", Color.BlanchedAlmond);

					// Automatically turn recording off and reduce the amount of stored music boxes by 1.
					tMusicPlayer.MusicPlayerUI.recording = false;
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
				mpToggleVisibility = !mpToggleVisibility;
				if (mpToggleVisibility) {
					if (tMusicPlayer.tMPConfig.StartWithSmall != tMusicPlayer.MusicPlayerUI.smallPanel) {
						tMusicPlayer.MusicPlayerUI.SwapPanelSize();
					}
				}
			}
			if (tMusicPlayer.PlayStopHotkey.JustPressed) {
				ToggleButton(MusicMode.Play);
			}
			if (tMusicPlayer.PrevSongHotkey.JustPressed) {
				ChangeDisplay(false, false);
			}
			if (tMusicPlayer.NextSongHotkey.JustPressed) {
				ChangeDisplay(true, false);
			}

			this.AddOrRemoveChild(MusicPlayerPanel, mpToggleVisibility);
			this.AddOrRemoveChild(musicEntryPanel, selectionVisible);
			this.AddOrRemoveChild(selectionPanel, selectionVisible);

			// Update positions of UIElements that are not parented by the main SelectionPanel
			musicEntryPanel.Left.Pixels = selectionPanel.Left.Pixels - musicEntryPanel.Width.Pixels + 10f;
			musicEntryPanel.Top.Pixels = selectionPanel.Top.Pixels + 10f;
			musicEntryPanel.Recalculate();
			
			if (!listening && canPlay.Count == 0) {
				ToggleButton(MusicMode.Listen);
			}
			if (modplayer.musicBoxesStored <= 0) {
				recording = false;
			}
			if (!selectionVisible) {
				searchBar.currentString = "";
				OrganizeSelection(sortType, availabililty, FilterMod);
			}
		}

		public int FindNextIndex()
		{
			int index = musicData.FindIndex(x => x.music == tMusicPlayer.AllMusic[DisplayBox].music);
			for (int i = index; i < musicData.Count; i++) {
				if (i != index && canPlay.Contains(musicData[i].music)) {
					return i;
				}
			}
			return -1;
		}

		public int FindPrevIndex()
		{
			int index = musicData.FindIndex(x => x.music == tMusicPlayer.AllMusic[DisplayBox].music);
			for (int i = index; i >= 0; i--) {
				if (i != index && canPlay.Contains(musicData[i].music)) {
					return i;
				}
			}
			return -1;
		}

		private void ChangeDisplay(bool next, bool jumpToEnd = false)
		{
			if (!listening) {
				int newIndex = next ? FindNextIndex() : FindPrevIndex();
				if (newIndex != -1) { 
					DisplayBox = tMusicPlayer.AllMusic.FindIndex(x => x.music == musicData[newIndex].music);
					if (playingMusic > -1) {
						playingMusic = tMusicPlayer.AllMusic[DisplayBox].music;
					}
				}
			}
		}

		internal string ResetModFilter()
		{
			FilterMod = "";
			tMusicPlayer.SendDebugText($"FilterMod: " + FilterMod);
			return FilterMod;
		}

		internal string UpdateModFilter(bool next)
		{
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

		internal void UpdateViewMode()
		{
			viewMode = !viewMode;
			OrganizeSelection(sortType, availabililty, FilterMod);
		}

		internal void OrganizeSelection(SortBy sortBy, ProgressBy progressBy, string filterMod, bool initializing = false)
		{
			int displayMusicID = tMusicPlayer.AllMusic[DisplayBox].music;
			if (sortBy == SortBy.ID) {
				musicData = musicData.OrderBy(x => x.music).ToList();
			}
			if (sortBy == SortBy.Name) {
				musicData = musicData.OrderBy(x => x.name).ToList();
			}

			DisplayBox = tMusicPlayer.AllMusic.FindIndex(x => x.music == displayMusicID);
			
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
						bool CheckObtained = progressBy == ProgressBy.Obtained && modplayer.MusicBoxList.All(x => x.Type != musicData[i].musicbox);
						bool CheckUnobtained = progressBy == ProgressBy.Unobtained && modplayer.MusicBoxList.Any(x => x.Type == musicData[i].musicbox);

						if (CheckFilterMod || CheckObtained || CheckUnobtained) {
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
						bool CheckObtained = progressBy == ProgressBy.Obtained && modplayer.MusicBoxList.All(x => x.Type != musicData[i].musicbox);
						bool CheckUnobtained = progressBy == ProgressBy.Unobtained && modplayer.MusicBoxList.Any(x => x.Type == musicData[i].musicbox);

						if (CheckFilterMod || CheckObtained || CheckUnobtained) {
							continue;
						}
					}

					newRow = new ItemSlotRow(i, panelTextures[2].Bounds.Width, panelTextures[2].Bounds.Height);

					// Item Slot
					SelectionSlots[i] = new MusicBoxSlot(musicData[i].musicbox, 0.85f);
					SelectionSlots[i].Left.Pixels = 20f;
					SelectionSlots[i].Top.Pixels = (newRow.Height.Pixels / 2f) - (SelectionSlots[i].Height.Pixels / 2f);
					SelectionSlots[i].Id = $"SelectionSlotList_{i}";
					newRow.Append(SelectionSlots[i]);
					
					// Play button
					HoverButton playSong = new HoverButton(buttonTextures, new Rectangle(24, 0, 22, 22)) {
						Id = $"altplay_{musicData[i].music}",
					};
					playSong.Width.Pixels = 22f;
					playSong.Height.Pixels = 22f;
					playSong.Left.Pixels = SelectionSlots[i].Left.Pixels + SelectionSlots[i].Width.Pixels + 8f;
					playSong.Top.Pixels = (newRow.Height.Pixels / 2f) - (playSong.Height.Pixels / 2f);
					playSong.OnClick += (a, b) => ListViewPlaySong(playSong.Id);
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

		private void ListViewPlaySong(string Id)
		{
			int musicID = Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1));
			int index = tMusicPlayer.AllMusic.FindIndex(x => x.music == musicID);
			if (canPlay.Contains(musicID)) {
				return;
			}
			if (tMusicPlayer.MusicPlayerUI.playingMusic != musicID) {
				tMusicPlayer.MusicPlayerUI.ListenDisplay = -1;
				tMusicPlayer.MusicPlayerUI.listening = false;
				tMusicPlayer.MusicPlayerUI.DisplayBox = index;
				tMusicPlayer.MusicPlayerUI.playingMusic = musicID;
			}
			else {
				tMusicPlayer.MusicPlayerUI.playingMusic = -1;
			}
		}

		private void ToggleButton(MusicMode type)
		{
			if (Main.musicVolume > 0f) {
				switch (type) {
					case MusicMode.Play:
						if (!listening) {
							playingMusic = (playingMusic == -1) ? tMusicPlayer.AllMusic[DisplayBox].music : -1;
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
							recording = (Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>().musicBoxesStored > 0);
						}
						if (recording) {
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

		public void SwapPanelSize()
		{
			smallPanel = !smallPanel;

			Texture2D size = smallPanel ? panelTextures[1] : panelTextures[2];
			MusicPlayerPanel.Width.Pixels = size.Width;
			MusicPlayerPanel.Height.Pixels = size.Height;
			//MusicPlayerPanel.Top.Pixels = 6f;
			//MusicPlayerPanel.Left.Pixels = 500f;

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

		//TODO: Currently broken, but only used for an invisible image so its fine for now
		public static Texture2D CropTexture(Texture2D texture, Rectangle snippet)
		{
			Texture2D croppedTexture = new Texture2D(Main.graphics.GraphicsDevice, snippet.Width, snippet.Height);
			Color[] data = new Color[snippet.Width * snippet.Height];
			texture.GetData<Color>(0, (Rectangle?)snippet, data, 0, data.Length);
			croppedTexture.SetData<Color>(data);
			return croppedTexture;
		}
	}

	public enum MusicMode
	{
		Play,
		Listen,
		Record
	}

	public enum SortBy
	{
		ID,
		Name
	}

	public enum ProgressBy
	{
		None,
		Obtained,
		Unobtained
	}
}
