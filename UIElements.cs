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
			CalculatedStyle innerDimensions;
			if (Id == "SelectionPanel") {
				Texture2D obj = MusicPlayerUI.panelTextures[3];
				innerDimensions = GetInnerDimensions();
				spriteBatch.Draw(obj, innerDimensions.ToRectangle(), Color.White);
			}
			else if (Id == "MusicPlayerPanel") {
				Texture2D texture = tMusicPlayer.MusicPlayerUI.smallPanel ? MusicPlayerUI.panelTextures[1] : MusicPlayerUI.panelTextures[2];
				Texture2D val = texture;
				innerDimensions = GetInnerDimensions();
				spriteBatch.Draw(val, innerDimensions.ToRectangle(), Color.White);
			}
			Draw(spriteBatch);
			if (Id == "MusicPlayerPanel" && !tMusicPlayer.MusicPlayerUI.smallPanel) {
				int musicBoxDisplayed = (tMusicPlayer.MusicPlayerUI.listening && tMusicPlayer.MusicPlayerUI.ListenDisplay != -1) ? tMusicPlayer.MusicPlayerUI.ListenDisplay : tMusicPlayer.MusicPlayerUI.DisplayBox;
				MusicData musicRef = tMusicPlayer.AllMusic[musicBoxDisplayed];
				innerDimensions = GetInnerDimensions();
				Rectangle rect = innerDimensions.ToRectangle();
				Vector2 pos = default(Vector2);
				pos = new Vector2((float)(rect.X + 64), (float)(rect.Y + 10));
				Utils.DrawBorderString(spriteBatch, musicRef.name, pos, Color.White, 0.75f, 0f, 0f, -1);
				pos = new Vector2((float)(rect.X + 64), (float)(rect.Y + 30));
				Utils.DrawBorderString(spriteBatch, musicRef.mod, pos, Color.White, 0.75f, 0f, 0f, -1);
				if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
					Main.LocalPlayer.mouseInterface = true;
				}
			}
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			MouseDown(evt);
			if (((IEnumerable<UIElement>)Elements).All<UIElement>((Func<UIElement, bool>)((UIElement x) => !x.IsMouseHovering))) {
				DragStart(evt);
			}
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			MouseUp(evt);
			if (dragging) {
				DragEnd(evt);
			}
		}

		private void DragStart(UIMouseEvent evt)
		{
			CalculatedStyle dimensions2 = GetDimensions();
			Rectangle dimensions = dimensions2.ToRectangle();
			offset = new Vector2(evt.MousePosition.X - (float)dimensions.Left, evt.MousePosition.Y - (float)dimensions.Top);
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
			Update(gameTime);
			if (ContainsPoint(Main.MouseScreen)) {
				Main.LocalPlayer.mouseInterface = true;
			}
			if (dragging) {
				Left.Set((float)Main.mouseX - offset.X, 0f);
				Top.Set((float)Main.mouseY - offset.Y, 0f);
				Recalculate();
			}
			CalculatedStyle dimensions = Parent.GetDimensions();
			Rectangle parentSpace = dimensions.ToRectangle();
			Rectangle mouseRect = new Rectangle(Main.mouseX, Main.mouseY, 0, 0);
			dimensions = GetDimensions();
			Rectangle val = dimensions.ToRectangle();
			if (val.Intersects(parentSpace) || !mouseRect.Intersects(parentSpace)) {
				Left.Pixels = Utils.Clamp<float>(Left.Pixels, 0f, (float)parentSpace.Right - Width.Pixels);
				Top.Pixels = Utils.Clamp<float>(Top.Pixels, 0f, (float)parentSpace.Bottom - Height.Pixels);
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
			DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = tMusicPlayer.MP_UserInterface;
			MouseDown(evt);
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

		public bool UseAlternateTexture(string ID)
		{
			switch (ID) {
				case "expand":
					return !tMusicPlayer.MusicPlayerUI.smallPanel;
				case "play":
					return tMusicPlayer.MusicPlayerUI.playingMusic > -1;
				case "listen":
					return tMusicPlayer.MusicPlayerUI.listening;
				case "record":
					return !tMusicPlayer.MusicPlayerUI.recording;
				default:
					return false;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			bool useAlt = UseAlternateTexture(Id);
			bool firstOrLast = (Id == "prev" && tMusicPlayer.MusicPlayerUI.DisplayBox == 0) || (Id == "next" && tMusicPlayer.MusicPlayerUI.DisplayBox == tMusicPlayer.AllMusic.Count - 1);
			int indexPrev = tMusicPlayer.MusicPlayerUI.FindPrevIndex();
			int indexNext = tMusicPlayer.MusicPlayerUI.FindNextIndex();
			bool firstOrLastUnavail = (Id == "prev" && (indexPrev == -1 || tMusicPlayer.MusicPlayerUI.listening)) || (Id == "next" && (indexNext == -1 || tMusicPlayer.MusicPlayerUI.listening));
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			bool recordUnavail = Id == "record" && modplayer.musicBoxesStored <= 0;
			bool activeListen = (Id == "next" || Id == "prev" || Id == "play") && tMusicPlayer.MusicPlayerUI.listening;
			bool musicAtZero = Id != "expand" && Id != "view" && Main.musicVolume <= 0f;
			bool disabled = firstOrLast | firstOrLastUnavail | recordUnavail | activeListen | musicAtZero;
			Rectangle push = new Rectangle(useAlt ? (src.X + src.Width + 2) : src.X, (IsMouseHovering && !disabled) ? (src.Y + src.Height + 2) : 0, src.Width, src.Height);
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
				default:
					return "";
			}
		}
	}

	internal class ItemSlotRow : UIElement
	{
		private int order;

		public ItemSlotRow(int order)
		{
			this.order = order;
			Height.Pixels = 50f;
			Width.Pixels = 400f;
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
			Width.Set((float)Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set((float)Main.inventoryBack9Texture.Height * scale, 0f);
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
						tMusicPlayer.SendDebugMessage($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{refItem}]", Colors.RarityGreen);
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
			ItemSlot.Draw(spriteBatch, ref musicBox, context, Utils.TopLeft(rectangle), default(Color));
			Main.inventoryBack2Texture = backup;
			Main.inventoryScale = oldScale;
			if (isSelectionSlot) {
				int index2 = Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1));
				if (musicBox.type == refItem) {
					if (modplayer.MusicBoxList.All(x => x.Type != refItem)) {
						modplayer.MusicBoxList.Add(new ItemDefinition(refItem));
						tMusicPlayer.MusicPlayerUI.canPlay[index2] = true;
						tMusicPlayer.SendDebugMessage($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{refItem}]", Colors.RarityGreen);
					}
					if (IsMouseHovering && Main.mouseRight) {
						tMusicPlayer.MusicPlayerUI.ListenDisplay = -1;
						tMusicPlayer.MusicPlayerUI.listening = false;
						tMusicPlayer.MusicPlayerUI.DisplayBox = Convert.ToInt32(Id.Substring(14));
						tMusicPlayer.MusicPlayerUI.playingMusic = tMusicPlayer.AllMusic[Convert.ToInt32(Id.Substring(14))].music;
					}
				}
				else if (musicBox.IsAir && ContainsPoint(Main.MouseScreen) && modplayer.MusicBoxList.Any(x => x.Type == refItem)) {
					modplayer.MusicBoxList.RemoveAll(x => x.Type == refItem);
					tMusicPlayer.MusicPlayerUI.canPlay[index2] = tMusicPlayer.tMPConfig.EnableAllMusicBoxes;
					tMusicPlayer.SendDebugMessage($"Removed Music Box [ID#{refItem}]", Color.IndianRed);
					if (!tMusicPlayer.MusicPlayerUI.canPlay[index2]) {
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
					float x2 = (float)(rectangle.X + rectangle.Width / 2) - (float)Main.itemTexture[type].Width * scale / 2f;
					float y2 = (float)(rectangle.Y + rectangle.Height / 2) - (float)Main.itemTexture[type].Height * scale / 2f;
					spriteBatch.Draw(Main.itemTexture[type], new Vector2(x2, y2), Main.itemTexture[type].Bounds, new Color(75, 75, 75, 75), 0f, Vector2.Zero, scale, 0, 0f);
				}
			}
			return;
		}
	}

	internal class NewUITextBox : UITextBox
	{
		internal bool focused = false;

		private int _maxLength = 60;
		private string hintText;
		internal string currentString = "";

		private int textBlinkerCount;
		private int textBlinkerState;
		internal bool unfocusOnEnter = true;
		internal bool unfocusOnTab = true;

		public event Action OnFocus;
		public event Action OnUnfocus;
		public event Action OnTextChanged;
		public event Action OnTabPressed;
		public event Action OnEnterPressed;
		public event Action OnEscPressed;

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
			Click(evt);
		}

		public override void RightClick(UIMouseEvent evt)
		{
			RightClick(evt);
			SetText("");
		}

		public void SetUnfocusKeys(bool unfocusOnEnter, bool unfocusOnTab)
		{
			this.unfocusOnEnter = unfocusOnEnter;
			this.unfocusOnTab = unfocusOnTab;
		}

		public void Unfocus()
		{
			if (focused) {
				focused = false;
				Main.blockInput = false;
				OnUnfocus?.Invoke();
			}
		}

		public void Focus()
		{
			if (!focused) {
				Main.clrInput();
				focused = true;
				Main.blockInput = true;
				OnFocus?.Invoke();
			}
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight)) {
				Unfocus();
			}
			Update(gameTime);
		}

		public override void SetText(string text, float textScale, bool large)
		{
			if (text.ToString().Length > _maxLength) {
				text = text.ToString().Substring(0, _maxLength);
			}
			if (currentString != text) {
				currentString = text;
				OnTextChanged?.Invoke();
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
			DrawSelf(spriteBatch);
			if (focused) {
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (Main.fontMouseText.MeasureString(newString.ToLower()).X < Width.Pixels - 10f) {
					if (!newString.Equals(currentString)) {
						currentString = newString.ToLower();
						OnTextChanged?.Invoke();
					}
					else {
						currentString = newString.ToLower();
					}
					MusicPlayerUI ui = tMusicPlayer.MusicPlayerUI;
					if (currentString.Length > 0) {
						List<MusicData> musicData = new List<MusicData>();
						foreach (MusicData item in tMusicPlayer.AllMusic) {
							if (item.name.ToLower().Contains(currentString)) {
								musicData.Add(item);
							}
						}
						ui.OrganizeSelection(musicData, ui.sortType, ui.filterType, false);
					}
					else {
						ui.OrganizeSelection(new List<MusicData>(tMusicPlayer.AllMusic), ui.sortType, ui.filterType, false);
					}
				}
				if (JustPressed(Keys.Tab)) {
					if (unfocusOnTab) {
						Unfocus();
					}
					OnTabPressed?.Invoke();
				}
				if (JustPressed(Keys.Escape)) {
					if (unfocusOnTab) {
						Unfocus();
					}
					OnEscPressed?.Invoke();
				}
				if (JustPressed(Keys.Enter)) {
					Main.drawingPlayerChat = false;
					if (unfocusOnEnter) {
						Unfocus();
					}
					OnEnterPressed?.Invoke();
				}
				if (++textBlinkerCount >= 20) {
					textBlinkerState = (textBlinkerState + 1) % 2;
					textBlinkerCount = 0;
				}
				Main.instance.DrawWindowsIMEPanel(new Vector2(98f, (float)(Main.screenHeight - 36)), 0f);
			}
			string displayString = currentString;
			if (textBlinkerState == 1 && focused) {
				displayString += "|";
			}
			CalculatedStyle space = GetDimensions();
			Color color2 = Color.Black;
			Vector2 drawPos = space.Position() + new Vector2(4f, 2f);
			if (currentString.Length == 0 && !focused) {
				color2 *= 0.5f;
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, hintText, drawPos, color2);
			}
			else {
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, displayString, drawPos, color2);
			}
		}
	}

	internal class UIFilter : UIImage
	{
		internal Texture2D texture;
		internal int type;

		public UIFilter(Texture2D texture, int type) : base(texture)
		{
			this.texture = texture;
			this.type = type;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (tMusicPlayer.MusicPlayerUI.filterType != 0 || type != 2) {
				Texture2D image;
				string text;
				if (type == 1) {
					switch (tMusicPlayer.MusicPlayerUI.filterType) {
						case FilterBy.Mod:
							image = MusicPlayerUI.filterTextures[4];
							text = "Filtering by mod";
							break;
						case FilterBy.Availability:
							image = MusicPlayerUI.filterTextures[5];
							text = "Filtering by stored";
							break;
						default:
							image = MusicPlayerUI.filterTextures[3];
							text = "No filtering";
							break;
					}
				}
				else if (type == 2) {
					switch (tMusicPlayer.MusicPlayerUI.filterType) {
						default:
							return;
						case FilterBy.Mod:
							image = MusicPlayerUI.filterTextures[6];
							text = (tMusicPlayer.MusicPlayerUI.FilterMod ?? "");
							break;
						case FilterBy.Availability:
							if (tMusicPlayer.MusicPlayerUI.obtained) {
								image = MusicPlayerUI.filterTextures[7];
								text = "Showing obtained";
							}
							else {
								image = MusicPlayerUI.filterTextures[8];
								text = "Showing unobtained";
							}
							break;
					}
				}
				else {
					SortBy sortType = tMusicPlayer.MusicPlayerUI.sortType;
					if (sortType == SortBy.ID) {
						image = MusicPlayerUI.filterTextures[1];
						text = "Sorting by ID";
					}
					if (sortType == SortBy.Name) {
						image = MusicPlayerUI.filterTextures[2];
						text = "Sorting by name";
					}
					else {
						image = MusicPlayerUI.filterTextures[0];
						text = "No sorting";
					}
				}
				SetImage(image);
				if (IsMouseHovering) {
					Main.hoverItemName = text;
				}
				Draw(spriteBatch);
				if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
					Main.LocalPlayer.mouseInterface = true;
				}
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
			DrawSelf(spriteBatch);
			Vector2 pos = new Vector2((float)rectangle.X + Width.Pixels * 0.75f, (float)rectangle.Y + Height.Pixels * 0.75f);
			Utils.DrawBorderString(spriteBatch, modplayer.musicBoxesStored.ToString(), pos, Color.White, 0.85f);
		}
	}

}
