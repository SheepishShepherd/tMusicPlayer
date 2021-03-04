using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class BackDrop : UIElement
	{
		internal Texture2D texture;
		public bool dragging;
		private Vector2 offset;

		public BackDrop(Texture2D texture)
		{
			this.texture = texture;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Rectangle rect = GetInnerDimensions().ToRectangle();
			if (Id == "SelectionPanel") {
				Texture2D texture = MusicPlayerUI.panelTextures[3];
				spriteBatch.Draw(texture, rect, Color.White);
			}
			else if (Id == "MusicPlayerPanel") {
				Texture2D texture = tMusicPlayer.MusicPlayerUI.smallPanel ? MusicPlayerUI.panelTextures[1] : MusicPlayerUI.panelTextures[2];
				spriteBatch.Draw(texture, rect, Color.White);
			}
			else if (Id == "MusicEntry") {
				Texture2D obj = MusicPlayerUI.panelTextures[1];
				// TODO[?]: Simplify/make better later
				spriteBatch.Draw(obj, new Rectangle(rect.X + obj.Bounds.Height + 2, rect.Y, obj.Width, obj.Height), obj.Bounds, Color.White, MathHelper.ToRadians(90), Vector2.Zero, SpriteEffects.None, 0f);
			}
			base.Draw(spriteBatch);
			if (Id == "MusicPlayerPanel" && !tMusicPlayer.MusicPlayerUI.smallPanel) {
				int musicBoxDisplayed = (tMusicPlayer.MusicPlayerUI.listening && tMusicPlayer.MusicPlayerUI.ListenDisplay != -1) ? tMusicPlayer.MusicPlayerUI.ListenDisplay : tMusicPlayer.MusicPlayerUI.DisplayBox;
				MusicData musicRef = tMusicPlayer.AllMusic[musicBoxDisplayed];
				Vector2 pos = new Vector2((rect.X + 64), (rect.Y + 10));
				Utils.DrawBorderString(spriteBatch, musicRef.name, pos, Color.White, 0.75f, 0f, 0f, -1);
				pos = new Vector2((rect.X + 64), (rect.Y + 30));
				Utils.DrawBorderString(spriteBatch, musicRef.mod, pos, Color.White, 0.75f, 0f, 0f, -1);
			}
			
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				// Needed to remove mousetext from outside sources when using the Boss Log
				Main.player[Main.myPlayer].mouseInterface = true;
				Main.mouseText = true;
				// Item icons such as hovering over a bed will not appear
				Main.LocalPlayer.showItemIcon = false;
				Main.LocalPlayer.showItemIcon2 = -1;
				Main.ItemIconCacheUpdate(0);
			}
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			base.MouseDown(evt);
			if (((IEnumerable<UIElement>)Elements).All((UIElement x) => !x.IsMouseHovering)) {
				DragStart(evt);
			}
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			base.MouseUp(evt);
			if (dragging) {
				DragEnd(evt);
			}
		}

		private void DragStart(UIMouseEvent evt)
		{
			CalculatedStyle dimensions2 = GetDimensions();
			Rectangle dimensions = dimensions2.ToRectangle();
			offset = new Vector2(evt.MousePosition.X - dimensions.Left, evt.MousePosition.Y - dimensions.Top);
			dragging = true;
		}

		private void DragEnd(UIMouseEvent evt)
		{
			Vector2 end = evt.MousePosition;
			dragging = false;
			Left.Set(end.X - offset.X, 0f);
			Top.Set(end.Y - offset.Y, 0f);
			Recalculate();
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}
			if (dragging) {
				Left.Set(Main.mouseX - offset.X, 0f);
				Top.Set(Main.mouseY - offset.Y, 0f);
				Recalculate();
			}
			CalculatedStyle dimensions = Parent.GetDimensions();
			Rectangle parentSpace = dimensions.ToRectangle();
			Rectangle mouseRect = new Rectangle(Main.mouseX, Main.mouseY, 0, 0);
			dimensions = GetDimensions();
			Rectangle val = dimensions.ToRectangle();
			if (val.Intersects(parentSpace) || !mouseRect.Intersects(parentSpace)) {
				Left.Pixels = Utils.Clamp(Left.Pixels, 0f, parentSpace.Right - Width.Pixels);
				Top.Pixels = Utils.Clamp(Top.Pixels, 0f, parentSpace.Bottom - Height.Pixels);
				Recalculate();
			}
		}
	}

	internal class FixedUIScrollbar : UIScrollbar
	{
		public FixedUIScrollbar()
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = tMusicPlayer.MP_UserInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = tMusicPlayer.MP_UserInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
	

	internal class HoverButton : UIImage
	{
		internal Texture2D texture;
		internal Rectangle src;

		public HoverButton(Texture2D texture, Rectangle src) : base(texture)
		{
			this.texture = texture;
			this.src = src;
		}

		public bool UseAlternateTexture()
		{
			if (Id.Contains("altplay")) {
				int num = Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1));
				return tMusicPlayer.MusicPlayerUI.playingMusic == num;
			}
			switch (Id) {
				case "expand":
					return !tMusicPlayer.MusicPlayerUI.smallPanel;
				case "play":
					return tMusicPlayer.MusicPlayerUI.playingMusic > -1;
				case "listen":
					return tMusicPlayer.MusicPlayerUI.listening;
				case "record":
					return !tMusicPlayer.MusicPlayerUI.recording;
				case "viewmode":
					return !tMusicPlayer.MusicPlayerUI.viewMode;
				default:
					return false;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			bool useAlt = UseAlternateTexture();
			bool firstOrLast = (Id == "prev" && tMusicPlayer.MusicPlayerUI.DisplayBox == 0) || (Id == "next" && tMusicPlayer.MusicPlayerUI.DisplayBox == tMusicPlayer.AllMusic.Count - 1);
			int indexPrev = tMusicPlayer.MusicPlayerUI.FindPrevIndex();
			int indexNext = tMusicPlayer.MusicPlayerUI.FindNextIndex();
			bool firstOrLastUnavail = (Id == "prev" && (indexPrev == -1 || tMusicPlayer.MusicPlayerUI.listening)) || (Id == "next" && (indexNext == -1 || tMusicPlayer.MusicPlayerUI.listening));
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			bool recordUnavail = Id == "record" && modplayer.musicBoxesStored <= 0;
			bool activeListen = (Id == "next" || Id == "prev" || Id == "play") && tMusicPlayer.MusicPlayerUI.listening;
			bool musicAtZero = Id != "expand" && Id != "view" && Main.musicVolume <= 0f;
			bool clearModDisabled = Id == "clearfiltermod" && tMusicPlayer.MusicPlayerUI.FilterMod == "";
			bool cannotPlayListMusic = Id.Contains("altplay") && !tMusicPlayer.MusicPlayerUI.canPlay[tMusicPlayer.AllMusic.FindIndex(x => x.music == Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1)))];
			bool disabled = firstOrLast | firstOrLastUnavail | recordUnavail | activeListen | musicAtZero | clearModDisabled | cannotPlayListMusic;
			Rectangle push = new Rectangle(useAlt ? (src.X + src.Width + 2) : src.X, (IsMouseHovering && !disabled) ? (src.Y + src.Height + 2) : src.Y, src.Width, src.Height);
			Texture2D val = texture;
			CalculatedStyle innerDimensions = GetInnerDimensions();
			spriteBatch.Draw(val, innerDimensions.ToRectangle(), push, disabled ? new Color(60, 60, 60, 60) : Color.White);
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorEnabled && !disabled) {
					Main.hoverItemName = SetHoverItemName(Id);
				}
			}
		}

		public string SetHoverItemName(string ID)
		{
			switch (ID) {
				case "expand":
					return (tMusicPlayer.MusicPlayerUI.smallPanel ? "Maximize" : "Minimize") ?? "";
				case "play":
					return ((tMusicPlayer.MusicPlayerUI.playingMusic >= 0) ? "Stop" : "Play") ?? "";
				case "listen":
					return ((tMusicPlayer.MusicPlayerUI.playingMusic >= 0) ? "Stop" : "Play") ?? "";
				case "record":
					return (tMusicPlayer.MusicPlayerUI.listening ? "Disable" : "Enable") + " Listening";
				case "prev":
					return "Previous Song";
				case "next":
					return "Next Song";
				case "view":
					return (tMusicPlayer.MusicPlayerUI.selectionVisible ? "Close" : "Open") + " Selection List";
				case "sortby":
					return $"Sorted by {(tMusicPlayer.MusicPlayerUI.sortType == SortBy.ID ? "ID" : "Name")}";
				case "filtermod":
					return $"Showing {(tMusicPlayer.MusicPlayerUI.FilterMod == "" ? "all " : "")}music boxes{(tMusicPlayer.MusicPlayerUI.FilterMod != "" ? " from " : "")}{tMusicPlayer.MusicPlayerUI.FilterMod}";
				case "availability":
					return $"Showing all {(tMusicPlayer.MusicPlayerUI.availabililty == ProgressBy.Obtained ? "obtained " : "")}{(tMusicPlayer.MusicPlayerUI.availabililty == ProgressBy.Unobtained ? "unobtained " : "")}music boxes";
				case "viewmode":
					return tMusicPlayer.MusicPlayerUI.viewMode ? "Change to Grid mode" : "Change to List mode";
				default:
					return "";
			}
		}
	}

	internal class ItemSlotRow : UIElement
	{
		private int order;

		public ItemSlotRow(int order, float width, float height)
		{
			this.order = order;
			Width.Pixels = 400f;
			Height.Pixels = 50f;
		}

		public override int CompareTo(object obj)
		{
			ItemSlotRow other = obj as ItemSlotRow;
			return order.CompareTo(other.order);
		}
	}

	internal class MusicBoxSlot : UIElement
	{
		internal Item musicBox;
		internal int itemID;
		internal int refItem;
		private readonly int context;
		private readonly float scale;

		public MusicBoxSlot(int refItem, float scale)
		{
			context = 4;
			this.scale = scale;
			itemID = refItem;
			this.refItem = refItem;
			musicBox = new Item();
			musicBox.SetDefaults(0, false);
			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			bool isSelectionSlot = Id.Contains("SelectionSlot");
			bool isDisplaySlot = Id == "DisplaySlot";
			bool isEntrySlot = Id == "EntrySlot";
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = scale;
			CalculatedStyle dimensions = GetDimensions();
			Rectangle rectangle = dimensions.ToRectangle();
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			if (isDisplaySlot) {
				if (tMusicPlayer.MusicPlayerUI.listening) {
					int index = tMusicPlayer.AllMusic.FindIndex((MusicData musicRef) => musicRef.music == Main.curMusic);
					tMusicPlayer.MusicPlayerUI.ListenDisplay = index;
					if (index != -1) {
						itemID = tMusicPlayer.AllMusic[index].musicbox;
					}
				}
				if (!tMusicPlayer.MusicPlayerUI.listening || tMusicPlayer.MusicPlayerUI.ListenDisplay == -1) {
					tMusicPlayer.MusicPlayerUI.ListenDisplay = -1;
					itemID = tMusicPlayer.AllMusic[tMusicPlayer.MusicPlayerUI.DisplayBox].musicbox;
				}
			}
			if (!isEntrySlot) {
				bool HasMusicBox = modplayer.MusicBoxList.Any(item => item.Type == itemID);
				musicBox.SetDefaults(HasMusicBox ? itemID : 0);
			}
			else {
				if (!musicBox.IsAir) {
					if (musicBox.type != ItemID.MusicBox) {
						modplayer.MusicBoxList.Add(new ItemDefinition(musicBox.type));
						tMusicPlayer.MusicPlayerUI.canPlay[tMusicPlayer.AllMusic.FindIndex((MusicData x) => x.musicbox == musicBox.type)] = true;
						tMusicPlayer.SendDebugText($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{refItem}]", Colors.RarityGreen);
					}
					else if (modplayer.musicBoxesStored < 5) {
						modplayer.musicBoxesStored++;
					}
					musicBox.TurnToAir();
				}
			}
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (isSelectionSlot && (Main.mouseItem.type == refItem || Main.mouseItem.IsAir) && !Main.mouseRight) {
					ItemSlot.Handle(ref musicBox, context);
				}
				else if (isEntrySlot) {
					int mouseType = Main.mouseItem.type;
					if (mouseType != 0) {
						bool ValidEntryBox = modplayer.MusicBoxList.All(x => x.Type != mouseType) && tMusicPlayer.AllMusic.Any(y => y.musicbox == mouseType);
						bool isUnrecordedAndNotMax = mouseType == 576 && modplayer.musicBoxesStored < 5;
						if (ValidEntryBox | isUnrecordedAndNotMax) {
							ItemSlot.Handle(ref musicBox, context);
						}
					}
					else {
						ItemSlot.Handle(ref musicBox, context);
					}
					if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorEnabled) {
						Main.hoverItemName = "Insert a music box you do not already own!";
					}
				}
			}
			Texture2D backup = Main.inventoryBack2Texture;
			Main.inventoryBack2Texture = (isEntrySlot ? Main.inventoryBack7Texture : Main.inventoryBack3Texture);
			ItemSlot.Draw(spriteBatch, ref musicBox, context, Utils.TopLeft(rectangle));
			Main.inventoryBack2Texture = backup;
			Main.inventoryScale = oldScale;
			if (isSelectionSlot) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.musicbox == refItem);
				if (musicBox.type == refItem) {
					if (modplayer.MusicBoxList.All(x => x.Type != refItem)) {
						modplayer.MusicBoxList.Add(new ItemDefinition(refItem));
						tMusicPlayer.MusicPlayerUI.canPlay[index] = true;
						tMusicPlayer.SendDebugText($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{refItem}]", Colors.RarityGreen);
					}
					if (IsMouseHovering && Main.mouseRight && Id.Contains("Grid")) {
						tMusicPlayer.MusicPlayerUI.ListenDisplay = -1;
						tMusicPlayer.MusicPlayerUI.listening = false;
						tMusicPlayer.MusicPlayerUI.DisplayBox = index;
						tMusicPlayer.MusicPlayerUI.playingMusic = tMusicPlayer.AllMusic[index].music;
					}
				}
				else if (musicBox.IsAir && ContainsPoint(Main.MouseScreen) && modplayer.MusicBoxList.Any(x => x.Type == refItem)) {
					modplayer.MusicBoxList.RemoveAll(x => x.Type == refItem);
					//tMusicPlayer.MusicPlayerUI.canPlay[index] = tMusicPlayer.tMPConfig.EnableAllMusicBoxes;
					tMusicPlayer.SendDebugText($"Removed Music Box [ID#{refItem}]", Color.IndianRed);
					if (!tMusicPlayer.MusicPlayerUI.canPlay[index]) {
						int next = tMusicPlayer.MusicPlayerUI.FindNextIndex();
						int prev = tMusicPlayer.MusicPlayerUI.FindPrevIndex();
						if (next != -1) {
							tMusicPlayer.MusicPlayerUI.DisplayBox = next;
						}
						else if (prev != -1) {
							tMusicPlayer.MusicPlayerUI.DisplayBox = prev;
						}
						else {
							tMusicPlayer.MusicPlayerUI.playingMusic = -1;
							tMusicPlayer.MusicPlayerUI.listening = true;
						}
					}
				}
			}
			if (musicBox.IsAir) {
				int type;
				if (isDisplaySlot) {
					if (tMusicPlayer.MusicPlayerUI.listening && Main.musicVolume > 0f && tMusicPlayer.MusicPlayerUI.ListenDisplay != -1) {
						type = tMusicPlayer.AllMusic[tMusicPlayer.MusicPlayerUI.ListenDisplay].musicbox;
					}
					else {
						if (tMusicPlayer.MusicPlayerUI.DisplayBox == -1) {
							return;
						}
						type = tMusicPlayer.AllMusic[tMusicPlayer.MusicPlayerUI.DisplayBox].musicbox;
					}
				}
				else {
					type = refItem;
				}
				if (type > 0) {
					float x2 = (rectangle.X + rectangle.Width / 2) - Main.itemTexture[type].Width * scale / 2f;
					float y2 = (rectangle.Y + rectangle.Height / 2) - Main.itemTexture[type].Height * scale / 2f;
					spriteBatch.Draw(Main.itemTexture[type], new Vector2(x2, y2), Main.itemTexture[type].Bounds, new Color(75, 75, 75, 75), 0f, Vector2.Zero, scale, 0, 0f);
				}
			}
			return;
		}
	}

	internal class NewUITextBox : UITextBox
	{
		internal bool focused = false;

		private readonly int _maxLength = 40;
		private readonly string hintText;
		internal string currentString = "";

		private int textBlinkerCount;
		private int textBlinkerState;

		public NewUITextBox(string hintText, string text = "") : base(text)
		{
			this.hintText = hintText;
			currentString = text;
			SetPadding(0f);
			BackgroundColor = Color.White;
			BorderColor = Color.White;
		}

		public override void Click(UIMouseEvent evt)
		{
			Focus();
			base.Click(evt);
		}

		public override void RightClick(UIMouseEvent evt)
		{
			base.RightClick(evt);
			SetText("");
		}

		public void Unfocus()
		{
			if (focused) {
				focused = false;
				Main.blockInput = false;
			}
		}

		public void Focus()
		{
			if (!focused) {
				Main.clrInput();
				focused = true;
				Main.blockInput = true;
			}
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2(Main.mouseX, Main.mouseY);
			if (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight)) {
				Unfocus();
			}
			base.Update(gameTime);
		}

		public override void SetText(string text, float textScale, bool large)
		{
			if (text == null)
				return;
			if (text.ToString().Length > _maxLength) {
				text = text.ToString().Substring(0, _maxLength);
			}
			if (currentString != text) {
				currentString = text;
			}
		}

		private static bool JustPressed(Keys key)
		{
			return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Rectangle hitbox = innerDimensions.ToRectangle();
			//base.DrawSelf(spriteBatch);
			Texture2D searchbar = ModContent.GetTexture("tMusicPlayer/UI/search");
			spriteBatch.Draw(searchbar, GetDimensions().Position(), Color.White);
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
			if (focused) {
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (Main.fontMouseText.MeasureString(newString.ToLower()).X < Width.Pixels - 10f) {
					if (!newString.Equals(currentString)) {
						currentString = newString.ToLower();
					}
					else {
						currentString = newString.ToLower();
					}
					MusicPlayerUI ui = tMusicPlayer.MusicPlayerUI;
					if (currentString.Length > 0) {
						List<MusicData> musicData = new List<MusicData>();
						foreach (MusicData item in tMusicPlayer.AllMusic.ToArray()) {
							if (item.name.ToLower().Contains(currentString)) {
								musicData.Add(item);
							}
						}
						ui.musicData = musicData;
						ui.OrganizeSelection(ui.sortType, ui.availabililty, ui.FilterMod);
					}
					else {
						ui.OrganizeSelection(ui.sortType, ui.availabililty, ui.FilterMod);
					}
				}
				if (JustPressed(Keys.Tab) || JustPressed(Keys.Escape)) {
					Unfocus();
				}
				if (JustPressed(Keys.Enter)) {
					Main.drawingPlayerChat = false;
					Unfocus();
				}
				if (++textBlinkerCount >= 20) {
					textBlinkerState = (textBlinkerState + 1) % 2;
					textBlinkerCount = 0;
				}
				Main.instance.DrawWindowsIMEPanel(new Vector2(198f, Main.screenHeight - 36), 0f);
			}
			string displayString = currentString;
			if (textBlinkerState == 1 && focused) {
				displayString += "|";
			}
			Color color2 = Color.White;
			Vector2 drawPos = GetDimensions().Position() + new Vector2(32f, 3f);
			if (currentString.Length == 0 && !focused) {
				color2 *= 0.6f;
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, hintText, drawPos, color2);
			}
			else {
				color2 *= 0.8f;
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, displayString, drawPos, color2);
			}
		}
	}

	internal class ListenStorageSlot : UIImage
	{
		private readonly Texture2D texture;
		private readonly int itemID;

		public ListenStorageSlot(Texture2D texture, int itemID) : base(texture)
		{
			this.texture = texture;
			this.itemID = itemID;
			Width.Pixels = texture.Bounds.Width;
			Height.Pixels = texture.Bounds.Height;
		}

		public override void Click(UIMouseEvent evt)
		{
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();
			if (IsMouseHovering && modplayer.musicBoxesStored > 0) {
				if (Main.mouseItem.IsAir) {
					Main.mouseItem.SetDefaults(itemID);
					modplayer.musicBoxesStored--;
				}
				while (modplayer.musicBoxesStored > 0) {
					player.QuickSpawnItem(itemID, 1);
					modplayer.musicBoxesStored--;
				}
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();
			if (IsMouseHovering && modplayer.musicBoxesStored > 0) {
				if (Main.mouseItem.IsAir) {
					Main.mouseItem.SetDefaults(itemID);
					modplayer.musicBoxesStored--;
					Main.PlaySound(SoundID.Grab);
				}
				else {
					player.QuickSpawnItem(itemID, 1);
					modplayer.musicBoxesStored--;
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			Rectangle rectangle = dimensions.ToRectangle();
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorEnabled) {
					Main.hoverItemName = 
						"Stored music boxes record songs while recording is enabled\n" + 
						$"Up to {tMusicPlayer.tMPConfig.MaxStorage} music boxes can be held at once\n" + 
						"Right click to take out one music box\nLeft click to take all of them out";
				}
			}
			base.DrawSelf(spriteBatch);
			Vector2 pos = new Vector2(rectangle.X + Width.Pixels * 0.75f, rectangle.Y + Height.Pixels * 0.75f);
			Utils.DrawBorderString(spriteBatch, modplayer.musicBoxesStored.ToString(), pos, Color.White, 0.85f);
		}
	}

}
