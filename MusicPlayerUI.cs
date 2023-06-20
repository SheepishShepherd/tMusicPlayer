using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
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
				if (!value)
					SelectionPanelVisible = false; // Selection panel is hidden if the player is hidden
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

		/// <summary> The MusicData of the music box being displayed on the music player. </summary>
		public MusicData DisplayBox = null;

		/// <summary> The MusicData from the natural music being played with <see cref="Main.curMusic"/>. </summary>
		public MusicData ListenModeData => MusicUISystem.Instance.AllMusic.Find(data => data.MusicID == Main.curMusic) is MusicData data ? data : MusicUISystem.Instance.UnknownMusic;

		public MusicData VisualBoxDisplayed => IsListening ? ListenModeData : DisplayBox;

		private bool playingMusic = false;
		private bool listening = true;
		private bool recording = false;
		private bool smallPanel = true;
		private bool viewMode = false;

		public bool MiniModePlayer {
			get => smallPanel;
			set {
				MusicPlayerPanel.UpdatePanelDimensions(value);
				BoxEntrySlot.Top.Pixels = searchBar.Top.Pixels;
				ejectButton.Top.Pixels = 42;

				MusicPlayerPanel.AddOrRemoveChild(detectButton, !value); // mini-mode does not contain these buttons
				MusicPlayerPanel.AddOrRemoveChild(recordButton, !value);
				MusicPlayerPanel.AddOrRemoveChild(prevButton, !value);
				MusicPlayerPanel.AddOrRemoveChild(nextButton, !value);

				expandButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - expandButton.Width.Pixels - 8f;
				viewButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - 20 - 8;
				playButton.Left.Pixels = value ? MusicPlayerPanel.Width.Pixels - playButton.Width.Pixels - 6f : prevButton.Left.Pixels + playButton.Width.Pixels - 2f;
				playButton.Top.Pixels = value ? viewButton.Top.Pixels - playButton.Height.Pixels + 2f : MusicPlayerPanel.Height.Pixels - playButton.Height.Pixels - 4f;
				
				smallPanel = value;				
			}
		}

		/// <summary> What music ID is currently being played by the music player. Return -1 if not playing music. </summary>
		public int CurrentlyPlaying => IsPlayingMusic ? DisplayBox.MusicID : -1;

		/// <summary> Whether or not the music player is outputing music. </summary>
		public bool IsPlayingMusic {
			get => playingMusic;
			set {
				if (Main.musicVolume == 0f) {
					playingMusic = false;
					return; // Cannot change if music is turned off
				}

				if (MusicUISystem.Instance.AllMusic.All(data => !data.CanPlay(Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>())) || IsListening)
					value = false; // force false if the player has no music boxes to play OR while listening mode is active (play button is disabled)

				playingMusic = value;
			}
		}

		/// <summary> Whether or not the music player is actively listening to non-player related music. </summary>
		public bool IsListening {
			get => listening;
			set {
				if (Main.musicVolume == 0f) {
					listening = false;
					return; // Cannot change if music is turned off
				}

				if (value) {
					playingMusic = false; // if listening, playing music is turned off
				}
				else {
					recording = false; // if not listening, recording is disabled
				}
				listening = value;
			}
		}

		/// <summary> Whether or not the music player is currently recording music. This cannot be true if the music player has no stored music boxes. </summary>
		public bool IsRecording {
			get => recording;
			set {
				if (Main.musicVolume == 0f) {
					recording = false;
					return; // Cannot change if music is turned off
				}

				if (Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>().musicBoxesStored == 0)
					value = false; // force false if the player has no boxes to record with

				if (value)
					IsListening = true; // listening needs to occur when recording

				recording = value;
			}
		}

		internal bool IsGridMode => viewMode == false;
		internal bool IsListMode => viewMode == true;
		internal bool ViewMode {
			get => viewMode;
			set {
				viewMode = value;
				OrganizeSelection();
			}
		}

		internal bool viewFavs = false;

		// Selection Panel content and functionality
		public MusicBoxSlot BoxEntrySlot;
		public MusicBoxSlot[] SelectionSlots;
		public UIList SelectionList;
		public FixedUIScrollbar selectionScrollBar;
		public SearchBar searchBar;

		// Sort/Filter functionality
		internal List<MusicData> SortedMusicData;
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

			SelectionPanel = new BackDrop(panelSelect) {
				Id = "SelectionPanel"
			};

			ResetPanelPositionsToDefault();

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
			playButton.OnLeftClick += (a, b) => IsPlayingMusic = !IsPlayingMusic;
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
			detectButton.OnLeftClick += (a, b) => IsListening = !IsListening;

			recordButton = new HoverButton(buttonTextures.Value, new Point(7, 0)) {
				Id = "record"
			};
			recordButton.Left.Pixels = detectButton.Left.Pixels - recordButton.Width.Pixels - 4f;
			recordButton.Top.Pixels = viewButton.Top.Pixels;
			recordButton.OnLeftClick += (a, b) => IsRecording = !IsRecording;

			expandButton = new HoverButton(closeTextures.Value, new Point(1, 0)) {
				Id = "expand"
			};
			expandButton.Left.Pixels = MusicPlayerPanel.Width.Pixels - expandButton.Width.Pixels - 8f;
			expandButton.Top.Pixels = 4f;
			expandButton.OnLeftClick += (a, b) => MiniModePlayer = !MiniModePlayer;
			MusicPlayerPanel.Append(expandButton);
			
			DisplayMusicSlot = new MusicBoxSlot(1f) {
				IsDisplaySlot = true
			};
			DisplayMusicSlot.Left.Pixels = 8f;
			DisplayMusicSlot.Top.Pixels = MusicPlayerPanel.Height.Pixels / 2f - TextureAssets.InventoryBack.Value.Height / 2;
			MusicPlayerPanel.Append(DisplayMusicSlot);

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
			clearFilterModButton.OnLeftClick += (a, b) => OrganizeSelection(filterMod: "");
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
			viewModeButton.OnLeftClick += (a, b) => ViewMode = !ViewMode;
			SelectionPanel.Append(viewModeButton);

			searchBar = new SearchBar("Search...", "");
			searchBar.Width.Pixels = 216f;
			searchBar.Height.Pixels = 28f;
			searchBar.Top.Pixels = 9f;
			searchBar.Left.Pixels = 12f;
			SelectionPanel.Append(searchBar);

			BoxEntrySlot = new MusicBoxSlot(0.85f) {
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
			
			SelectionList = new UIList();
			SelectionList.Width.Pixels = SelectionPanel.Width.Pixels - (-selectionScrollBar.Left.Pixels * 2) - selectionScrollBar.Width.Pixels;
			SelectionList.Height.Pixels = SelectionPanel.Height.Pixels - 85f;
			SelectionList.Left.Pixels = 0f;
			SelectionList.Top.Pixels = 72f;
			SelectionPanel.Append(SelectionList);
			SelectionPanel.Append(selectionScrollBar); // append scrollbar after, so it is on top
		}

		public override void Update(GameTime gameTime) {
			if (Main.curMusic == 0)
				IsRecording = IsListening = IsPlayingMusic = false; // If the game's music is muted, turn off all music player functions

			// This code mimics the "Music Box Recording" process.
			// Check if we have music boxes at the ready, if the player is in record mode and music is currently playing.
			// If all of those apply, we also go a rand check which will trigger the "recording" code.
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();

			if (modplayer.musicBoxesStored > 0 && recording && ListenModeData is MusicData listenData && Main.rand.NextBool(540)) {
                SoundEngine.PlaySound(SoundID.Item166);
				if (!modplayer.BoxIsCollected(listenData.MusicBox)) {
					// If we don't have it in our music player, automatically add it in.
					// as soon as it is recorded, the player should be able to play the music
					modplayer.MusicBoxList.Add(new ItemDefinition(listenData.MusicBox));
					tMusicPlayer.SendDebugText(listenData.MusicBox, "Added", "Via.Recording", Colors.RarityGreen);
				}
				else {
					// If we do have it already, spawn the item.
					player.QuickSpawnItem(player.GetSource_OpenItem(listenData.MusicBox), listenData.MusicBox);
					tMusicPlayer.SendDebugText(listenData.MusicBox, "Recorded", "Via.NotAccepted", Color.BlanchedAlmond);
				}

				// Automatically turn recording off and reduce the amount of stored music boxes by 1.
				IsRecording = false;
				modplayer.musicBoxesStored--;
			}

			base.Update(gameTime);

			if (Main.gameMenu) {
				IsListening = true;
			}
			else if (MusicPlayerVisible) {
				if (tMusicPlayer.ListenModeHotkey.JustPressed) {
					IsListening = !IsListening;
				}
				else if (tMusicPlayer.PlayStopHotkey.JustPressed) {
					if (IsListening) {
						IsListening = false;
						IsPlayingMusic = false;
					}
					else {
						IsPlayingMusic = !IsPlayingMusic;
					}
				}
				else if (tMusicPlayer.PrevSongHotkey.JustPressed) {
					ChangeDisplay(false);
				}
				else if (tMusicPlayer.NextSongHotkey.JustPressed) {
					ChangeDisplay(true);
				}
			}
		}

		public void ResetPanelPositionsToDefault() {
			MusicPlayerPanel.Left.Pixels = 1115f;
			MusicPlayerPanel.Top.Pixels = 16f;
			SelectionPanel.Left.Pixels = Main.screenWidth / 2 - SelectionPanel.Width.Pixels / 2f;
			SelectionPanel.Top.Pixels = Main.screenHeight / 2 - SelectionPanel.Height.Pixels / 2f;
		}

		public MusicData FindNext() {
			if (IsListening)
				return null; // If player is listening, the button is disabled.

			int index = SortedMusicData.FindIndex(data => data.MusicID == DisplayBox.MusicID);
			if (index == -1 || index == SortedMusicData.Count - 1)
				return null; // either the music data is the last entry or invalid

			int nextIndex = index + 1;
			while (nextIndex < SortedMusicData.Count) {
				if (SortedMusicData[nextIndex].CanPlay(Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>()))
					return SortedMusicData[nextIndex]; // playable music has been found
				nextIndex++;
			}

			return null; // no playable music can be found next
		}

		public MusicData FindPrev() {
			if (IsListening)
				return null; // If player is listening, the button is disabled.

			int index = SortedMusicData.FindIndex(data => data.MusicID == DisplayBox.MusicID);
			if (index == -1 || index == 0)
				return null; // either the music data is the first entry or invalid

			int prevIndex = index - 1;
			while (prevIndex >= 0) {
				if (SortedMusicData[prevIndex].CanPlay(Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>()))
					return SortedMusicData[prevIndex]; // playable music has been found
				prevIndex--;
			}

			return null; // no playable music can be found previously
		}

		private void ChangeDisplay(bool next, bool jumpToEnd = false) {
			if (IsListening)
				return;

			MusicData newMusic = next ? FindNext() : FindPrev();
			if (newMusic is not null)
				DisplayBox = newMusic;
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
				FilterMod = ModList[^1];
			}
			else {
				int nextOrPrev = next ? 1 : -1;
				FilterMod = ModList[indexOfCurrent + nextOrPrev];
			}
			return FilterMod;
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
			if (sortBy.HasValue)
				sortType = sortBy.Value;

			if (progressBy.HasValue)
				availabililty = progressBy.Value;

			if (filterMod is not null)
				FilterMod = filterMod;

			if (clickedFavorites)
				viewFavs = !viewFavs;

			if (sortType == SortBy.Name) {
				SortedMusicData = SortedMusicData.OrderBy(x => x.Name).ToList();
			}
			else {
				SortedMusicData = SortedMusicData.OrderBy(x => x.MusicID).ToList(); // default sorting by ID
			}

			SelectionList.Clear();

			ItemSlotRow newRow = new ItemSlotRow(0);
			int slotCount = 0;
			int col = 0;
			int row = 0;
			foreach (MusicData data in SortedMusicData) {
				if (!initializing) {
					MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
					bool CheckFilterMod = FilterMod != "" && data.Mod != FilterMod;
					bool CheckObtained = availabililty == ProgressBy.Obtained && !modplayer.BoxIsCollected(data.MusicBox);
					bool CheckUnobtained = availabililty == ProgressBy.Unobtained && modplayer.BoxIsCollected(data.MusicBox);
					bool CheckFavorited = viewFavs && !modplayer.BoxIsFavorited(data.MusicBox);

					if (CheckFilterMod || CheckObtained || CheckUnobtained || CheckFavorited)
						continue;
				}

				MusicBoxSlot boxSlot = SelectionSlots[slotCount];

				if (IsListMode) {
					newRow = new ItemSlotRow(slotCount);

					// Item Slot
					boxSlot = new MusicBoxSlot(data);
					boxSlot.Left.Pixels = 20f;
					boxSlot.Top.Pixels = (newRow.Height.Pixels / 2f) - (boxSlot.Height.Pixels / 2f);
					newRow.Append(boxSlot);

					// Play button
					HoverButton playSong = new HoverButton(buttonTextures.Value, new Point(1, 0)) {
						Id = "altplay",
						refNum = data.GetIndex
					};
					playSong.Left.Pixels = boxSlot.Left.Pixels + boxSlot.Width.Pixels + 8f;
					playSong.Top.Pixels = (newRow.Height.Pixels / 2f) - (playSong.Height.Pixels / 2f);
					playSong.OnLeftClick += (a, b) => UpdateMusicPlayedViaSelectionMenu(data);
					newRow.Append(playSong);

					// Song name and mod
					UIText songName = new UIText(data.Name, 0.85f) {
						TextColor = ItemRarity.GetColor(ContentSamples.ItemsByType[boxSlot.SlotItemID].rare)
					};
					songName.Left.Pixels = playSong.Left.Pixels + playSong.Width.Pixels + 8f;
					songName.Top.Pixels = (newRow.Height.Pixels / 2f) - 15f;
					newRow.Append(songName);

					UIText songMod = new UIText(data.Mod_DisplayName_NoChatTags(), 0.85f);
					songMod.Left.Pixels = playSong.Left.Pixels + playSong.Width.Pixels + 8f;
					songMod.Top.Pixels = (newRow.Height.Pixels / 2f) + 4f;
					newRow.Append(songMod);

					SelectionList.Add(newRow);
				}
				else {
					boxSlot = new MusicBoxSlot(data);
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

		public void UpdateMusicPlayedViaSelectionMenu(MusicData data) {
			if (Main.musicVolume <= 0f || !data.CanPlay(Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>()))
				return;

			if (CurrentlyPlaying == data.MusicID) {
				IsPlayingMusic = false; // if the music box is being played, stop it instead
			}
			else {
				IsListening = false;
				DisplayBox = data;
				IsPlayingMusic = true;
			}
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
