using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
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

		public HoverButton prevButton;
		public HoverButton playButton;
		public HoverButton nextButton;
		public HoverButton viewButton;
		public HoverButton detectButton;
		public HoverButton recordButton;
		public HoverButton expandButton;

		public bool smallPanel = true;
		public bool listening = false;
		public bool recording = false;

		public int DisplayBox = 0;
		public int ListenDisplay = -1;
		public int playingMusic = -1;

		public BackDrop selectionPanel;
		public BackDrop searchBarPanel;

		public NewUITextBox searchBar;
		public MusicBoxSlot AddMusicBoxSlot;
		public ListenStorageSlot MusicStorageSlot;
		public HoverButton closeButton;
		public UIList SelectionList;
		public FixedUIScrollbar selectionScrollBar;
		public MusicBoxSlot[] SelectionSlots;
		internal List<bool> canPlay;

		public UIFilter MusicSorter;
		public UIFilter MusicFilter;
		public UIFilter MusicFilter_Cycle;

		public bool selectionVisible = false;

		public const string path = "tMusicPlayer/UI/";
		public static Texture2D[] panelTextures;
		public static Texture2D buttonTextures;
		public static Texture2D closeTextures;
		public static Texture2D[] filterTextures;

		public SortBy sortType = SortBy.Music;
		public FilterBy filterType = FilterBy.None;
		public bool obtained;
		public string FilterMod = "Terraria";

		public override void OnInitialize()
		{
			panelTextures = new Texture2D[5]
			{
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/backdrop"), new Rectangle(0, 0, 1, 1)),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop"),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop2"),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop3"),
				ModContent.GetTexture("tMusicPlayer/UI/backdrop4")
			};

			filterTextures = new Texture2D[9]
			{
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(0, 0, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(32, 0, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(64, 0, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(0, 32, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(32, 32, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(64, 32, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(0, 64, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(32, 64, 30, 30)),
				CropTexture(ModContent.GetTexture("tMusicPlayer/UI/filters"), new Rectangle(64, 64, 30, 30))
			};

			buttonTextures = ModContent.GetTexture("tMusicPlayer/UI/buttons");
			closeTextures = ModContent.GetTexture("tMusicPlayer/UI/close");

			MusicPlayerPanel = new BackDrop(panelTextures[0]) {
				Id = "MusicPlayerPanel"
			};
			MusicPlayerPanel.Width.Pixels = (float)panelTextures[1].Width;
			MusicPlayerPanel.Height.Pixels = (float)panelTextures[1].Height;
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
			DisplayMusicSlot.Top.Pixels = MusicPlayerPanel.Height.Pixels / 2f - (float)(Main.inventoryBackTexture.Height / 2);
			MusicPlayerPanel.Append(DisplayMusicSlot);

			selectionPanel = new BackDrop(panelTextures[0]) {
				Id = "SelectionPanel"
			};
			selectionPanel.Width.Pixels = (float)panelTextures[3].Width;
			selectionPanel.Height.Pixels = (float)panelTextures[3].Height;
			selectionPanel.Left.Pixels = (float)(Main.screenWidth / 2) - selectionPanel.Width.Pixels / 2f;
			selectionPanel.Top.Pixels = (float)(Main.screenHeight / 2) - selectionPanel.Height.Pixels / 2f;
			searchBarPanel = new BackDrop(panelTextures[4]) {
				Id = "SearchBar"
			};

			searchBarPanel.Width.Pixels = (float)panelTextures[4].Width;
			searchBarPanel.Height.Pixels = (float)panelTextures[4].Height;
			searchBarPanel.Left.Pixels = selectionPanel.Left.Pixels + selectionPanel.Width.Pixels - searchBarPanel.Width.Pixels - 10f;
			searchBarPanel.Top.Pixels = selectionPanel.Top.Pixels - searchBarPanel.Height.Pixels;

			searchBar = new NewUITextBox("search...", "");
			searchBar.Width.Pixels = 144f;
			searchBar.Height.Pixels = 25f;
			searchBar.Top.Pixels = 8f;
			searchBar.Left.Pixels = 8f;
			searchBarPanel.Append(searchBar);

			AddMusicBoxSlot = new MusicBoxSlot(2010, 0.85f) {
				Id = "EntrySlot"
			};
			AddMusicBoxSlot.Top.Pixels = 10f;
			AddMusicBoxSlot.Left.Pixels = selectionPanel.Width.Pixels - 84f;
			selectionPanel.Append(AddMusicBoxSlot);

			MusicStorageSlot = new ListenStorageSlot(Main.itemTexture[576], 576);
			MusicStorageSlot.Top.Pixels = 20f;
			MusicStorageSlot.Left.Pixels = selectionPanel.Width.Pixels - 84f - MusicStorageSlot.Width.Pixels - 15f;
			selectionPanel.Append(MusicStorageSlot);

			selectionScrollBar = new FixedUIScrollbar();
			selectionScrollBar.SetView(100f, 1000f);
			selectionScrollBar.Top.Pixels = 50f;
			selectionScrollBar.Left.Pixels = -10f;
			selectionScrollBar.Height.Set(0f, 0.85f);
			selectionScrollBar.HAlign = 1f;

			SelectionList = new UIList();
			SelectionList.Width.Pixels = selectionPanel.Width.Pixels;
			SelectionList.Height.Pixels = selectionPanel.Height.Pixels - 74f;
			SelectionList.Left.Pixels = 0f;
			SelectionList.Top.Pixels = 60f;
			selectionPanel.Append(SelectionList);

			closeButton = new HoverButton(closeTextures, new Rectangle(0, 0, 18, 18)) {
				Id = "select_close"
			};
			closeButton.Width.Pixels = 18f;
			closeButton.Height.Pixels = 18f;
			closeButton.Left.Pixels = selectionPanel.Width.Pixels - closeButton.Width.Pixels - 11f;
			closeButton.Top.Pixels = 12f;
			closeButton.OnClick += (a, b) => mpToggleVisibility = !mpToggleVisibility;
			selectionPanel.Append(closeButton);

			MusicSorter = new UIFilter(filterTextures[3], 0);
			MusicSorter.Width.Pixels = 30f;
			MusicSorter.Height.Pixels = 30f;
			MusicSorter.Left.Pixels = 20f;
			MusicSorter.Top.Pixels = 15f;
			MusicSorter.OnClick += (a, b) => ChangeSortOrFilter(ButtonType.Sort, true);
			MusicSorter.OnRightClick += (a, b) => ChangeSortOrFilter(ButtonType.Sort, true);
			selectionPanel.Append(MusicSorter);

			MusicFilter = new UIFilter(filterTextures[3], 1);
			MusicFilter.Width.Pixels = 30f;
			MusicFilter.Height.Pixels = 30f;
			MusicFilter.Left.Pixels = 55f;
			MusicFilter.Top.Pixels = 15f;
			MusicFilter.OnClick += (a, b) => ChangeSortOrFilter(ButtonType.Filter, true);
			MusicFilter.OnRightClick += (a, b) => ChangeSortOrFilter(ButtonType.Filter, false);
			selectionPanel.Append(MusicFilter);

			MusicFilter_Cycle = new UIFilter(filterTextures[3], 2);
			MusicFilter_Cycle.Width.Pixels = 30f;
			MusicFilter_Cycle.Height.Pixels = 30f;
			MusicFilter_Cycle.Left.Pixels = 90f;
			MusicFilter_Cycle.Top.Pixels = 15f;
			MusicFilter_Cycle.OnClick += (a, b) => ChangeSortOrFilter(ButtonType.CycleFilter, true);
			MusicFilter_Cycle.OnRightClick += (a, b) => ChangeSortOrFilter(ButtonType.CycleFilter, true);
			selectionPanel.Append(MusicFilter_Cycle);
		}

		public override void Update(GameTime gameTime)
		{
			Update(gameTime);
			if (Main.gameMenu) {
				playingMusic = (ListenDisplay = -1);
				listening = false;
			}
			if (tMusicPlayer.HidePlayerHotkey.JustPressed) {
				mpToggleVisibility = !mpToggleVisibility;
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
			if (!Main.playerInventory) {
				selectionVisible = false;
			}

			this.AddOrRemoveChild(MusicPlayerPanel, Main.playerInventory && mpToggleVisibility);
			this.AddOrRemoveChild(selectionPanel, selectionVisible);
			this.AddOrRemoveChild(searchBarPanel, selectionVisible);
			selectionPanel.AddOrRemoveChild(MusicFilter_Cycle, filterType != FilterBy.None);

			searchBarPanel.Left.Pixels = selectionPanel.Left.Pixels + selectionPanel.Width.Pixels - searchBarPanel.Width.Pixels - 10f;
			searchBarPanel.Top.Pixels = selectionPanel.Top.Pixels - searchBarPanel.Height.Pixels;

			if (selectionPanel.ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface && selectionVisible) {
				Main.LocalPlayer.mouseInterface = true;
			}
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			if (!listening && !canPlay.Contains(true)) {
				ToggleButton(MusicMode.Listen);
			}
			if (modplayer.musicBoxesStored <= 0) {
				recording = false;
			}
			if (!selectionVisible) {
				searchBar.currentString = "";
				OrganizeSelection(new List<MusicData>(tMusicPlayer.AllMusic), sortType, filterType, false);
			}
		}

		public int FindNextIndex()
		{
			for (int i = DisplayBox; i < canPlay.Count; i++) {
				if (i != DisplayBox && canPlay[i]) {
					return i;
				}
			}
			return -1;
		}

		public int FindPrevIndex()
		{
			for (int i = DisplayBox; i >= 0; i--) {
				if (i != DisplayBox && canPlay[i]) {
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
					DisplayBox = newIndex;
				}
			}
		}

		public void CycleModFilter(bool next)
		{
			List<MusicData> musicData = new List<MusicData>(tMusicPlayer.AllMusic);
			List<string> mods = new List<string>();
			foreach (MusicData item in musicData) {
				if (!mods.Contains(item.mod)) {
					mods.Add(item.mod);
				}
			}
			mods.Sort();
			mods.Remove("Terraria");
			mods.Insert(0, "Terraria");
			int indexOfCurrent = mods.IndexOf(FilterMod);
			if (next && indexOfCurrent == mods.Count - 1) {
				FilterMod = "Terraria";
			}
			else if (!next && indexOfCurrent == 0) {
				FilterMod = mods[mods.Count - 1];
			}
			else {
				FilterMod = mods[indexOfCurrent + 1];
			}
		}

		public void ChangeSortOrFilter(ButtonType type, bool next)
		{
			switch (type) {
				case ButtonType.Sort:
					if (next) {
						if (sortType == SortBy.Name) {
							sortType = SortBy.Music;
						}
						else {
							sortType++;
						}
					}
					else if (sortType == SortBy.Music) {
						sortType = SortBy.Name;
					}
					else {
						sortType--;
					}
					break;
				case ButtonType.Filter:
					if (next) {
						if (filterType == FilterBy.Availability) {
							filterType = FilterBy.None;
						}
						else {
							filterType++;
						}
					}
					else if (filterType == FilterBy.None) {
						filterType = FilterBy.Availability;
					}
					else {
						filterType--;
					}
					if (filterType == FilterBy.Mod) {
						FilterMod = "Terraria";
					}
					obtained = true;
					break;
				case ButtonType.CycleFilter:
					if (filterType == FilterBy.Availability) {
						obtained = !obtained;
					}
					else {
						CycleModFilter(next);
					}
					break;
			}
			OrganizeSelection(new List<MusicData>(tMusicPlayer.AllMusic), sortType, filterType, false);
		}

		internal void OrganizeSelection(List<MusicData> list, SortBy sort, FilterBy filter, bool initializing = false)
		{
			if (list == null) {
				list = new List<MusicData>(tMusicPlayer.AllMusic);
			}

			if (sort == SortBy.Music) {
				list = list.OrderBy(x => x.music).ToList();
			}
			if (sort == SortBy.ID) {
				list = list.OrderBy(x => x.musicbox).ToList();
			}
			if (sort == SortBy.Name) {
				list = list.OrderBy(x => x.name).ToList();
			}

			if (!initializing) {
				selectionPanel.RemoveAllChildren();
				selectionPanel.Append(AddMusicBoxSlot);
				selectionPanel.Append(closeButton);
				selectionPanel.Append(MusicSorter);
				selectionPanel.Append(MusicFilter);
				selectionPanel.Append(MusicFilter_Cycle);
				selectionPanel.Append(MusicStorageSlot);
			}

			SelectionList.Clear();
			selectionPanel.RemoveChild(SelectionList);

			ItemSlotRow newRow = new ItemSlotRow(0) {
				Id = "Loot0"
			};
			int col = 0;
			int row = 0;
			for (int i = 0; i < SelectionSlots.Length; i++) {
				if (!initializing) {
					MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
					if (filter == FilterBy.Mod && (list[i].mod.ToLower() == FilterMod.ToLower())) {
						continue;
					}
					if (filter == FilterBy.Availability && obtained && modplayer.MusicBoxList.All(x => x.Type != list[i].musicbox)) {
						continue;
					}
					if (filter == FilterBy.Availability && !obtained && modplayer.MusicBoxList.Any(x => x.Type == list[i].musicbox)) {
						continue;
					}
				}

				SelectionSlots[i] = new MusicBoxSlot(list[i].musicbox, 0.85f);
				SelectionSlots[i].Left.Pixels = 20f + (SelectionSlots[i].Width.Pixels + 10f) * (float)col;
				SelectionSlots[i].Top.Pixels = 0f;
				SelectionSlots[i].Id = $"SelectionSlot_{i}";
				newRow.Append(SelectionSlots[i]);
				col++;
				if (col == 5) {
					row++;
					col = 0;
					SelectionList.Add(newRow);
					newRow = new ItemSlotRow(row) {
						Id = $"Loot{row}"
					};
				}
			}
			if (col != 0) {
				SelectionList.Add(newRow);
			}
			selectionPanel.Append(selectionScrollBar);
			selectionPanel.Append(SelectionList);
			SelectionList.SetScrollbar(selectionScrollBar);
		}

		private void ToggleButton(MusicMode type)
		{
			if (!(Main.musicVolume <= 0f)) {
				switch (type) {
					case MusicMode.Play:
						if (!listening) {
							playingMusic = ((playingMusic == -1) ? DisplayBox : (-1));
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
					tMusicPlayer.SendDebugMessage($"[c/{(playingMusic > -1 ? green : red)}:Playing] - [c/{(listening ? green : red)}:Listening] - [c/{(recording ? green : red)}:Recording]");
				}
			}
		}

		private void SwapPanelSize()
		{
			smallPanel = !smallPanel;

			Texture2D size = smallPanel ? panelTextures[1] : panelTextures[2];
			MusicPlayerPanel.Width.Pixels = (float)size.Width;
			MusicPlayerPanel.Height.Pixels = (float)size.Height;
			MusicPlayerPanel.Top.Pixels = 6f;
			MusicPlayerPanel.Left.Pixels = 500f;

			expandButton.Left.Pixels = (float)size.Width - expandButton.Width.Pixels - 8f;
			viewButton.Left.Pixels = (float)(size.Width - 20 - 8);
			detectButton.Left.Pixels = viewButton.Left.Pixels - detectButton.Width.Pixels - 2f;
			MusicPlayerPanel.AddOrRemoveChild(detectButton, !smallPanel);

			recordButton.Left.Pixels = detectButton.Left.Pixels - recordButton.Width.Pixels - 2f;
			MusicPlayerPanel.AddOrRemoveChild(recordButton, !smallPanel);
			MusicPlayerPanel.AddOrRemoveChild(prevButton, !smallPanel);

			playButton.Left.Pixels = ((!smallPanel) ? (prevButton.Left.Pixels + playButton.Width.Pixels - 2f) : ((float)size.Width - playButton.Width.Pixels - 6f));
			playButton.Top.Pixels = ((!smallPanel) ? ((float)size.Height - playButton.Height.Pixels - 4f) : (viewButton.Top.Pixels - playButton.Height.Pixels + 2f));

			nextButton.Left.Pixels = playButton.Left.Pixels + nextButton.Width.Pixels - 2f;
			MusicPlayerPanel.AddOrRemoveChild(nextButton, !smallPanel);
		}

		public static Texture2D CropTexture(Texture2D texture, Rectangle snippet)
		{
			Texture2D croppedTexture = new Texture2D(Main.graphics.GraphicsDevice, snippet.Width, snippet.Height);
			Color[] data = new Color[snippet.Width * snippet.Height];
			texture.GetData<Color>(0, (Rectangle?)snippet, data, 0, data.Length);
			croppedTexture.SetData<Color>(data);
			return croppedTexture;
		}
	}

	public enum ButtonType
	{
		Sort,
		Filter,
		CycleFilter
	}

	public enum FilterBy
	{
		None,
		Mod,
		Availability
	}

	public enum MusicMode
	{
		Play,
		Listen,
		Record
	}

	public enum SortBy
	{
		Music,
		ID,
		Name
	}

}
