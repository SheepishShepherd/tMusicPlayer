using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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
		public string Id { get; init; } = "";

		public bool dragging;
		private Vector2 offset;

		public BackDrop() { }

		public override void Draw(SpriteBatch spriteBatch)
		{
			MusicPlayerUI UI = MusicUISystem.MusicUI;
			Rectangle rect = GetInnerDimensions().ToRectangle();
			if (Id == "SelectionPanel") {
				Texture2D texture = MusicPlayerUI.panelTextures[3].Value;
				spriteBatch.Draw(texture, rect, Color.White);
			}
			else if (Id == "MusicPlayerPanel") {
				Texture2D texture = UI.smallPanel ? MusicPlayerUI.panelTextures[1].Value : MusicPlayerUI.panelTextures[2].Value;
				spriteBatch.Draw(texture, rect, Color.White);
			}
			else if (Id == "MusicEntry") {
				Texture2D obj = MusicPlayerUI.panelTextures[1].Value;
				// TODO[?]: Simplify/make better later
				spriteBatch.Draw(obj, new Rectangle(rect.X + obj.Bounds.Height + 2, rect.Y, obj.Width, obj.Height), obj.Bounds, Color.White, MathHelper.ToRadians(90), Vector2.Zero, SpriteEffects.None, 0f);
			}
			base.Draw(spriteBatch);
			if (Id == "MusicPlayerPanel" && !UI.smallPanel) {
				int musicBoxDisplayed = (UI.listening && UI.ListenDisplay != -1) ? UI.ListenDisplay : UI.DisplayBox;
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
				Main.LocalPlayer.cursorItemIconEnabled = false;
				Main.LocalPlayer.cursorItemIconID = -1;
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
		public string Id { get; init; } = "";

		public FixedUIScrollbar()
		{
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = MusicUISystem.MP_UserInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = MusicUISystem.MP_UserInterface;
			base.MouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
	

	internal class HoverButton : UIImage
	{
		public string Id { get; init; } = "";

		internal Texture2D texture;
		internal Rectangle src;

		public HoverButton(Texture2D texture, Rectangle src) : base(texture)
		{
			this.texture = texture;
			this.src = src;
		}

		public bool UseAlternateTexture()
		{
			MusicPlayerUI UI = MusicUISystem.MusicUI;
			if (Id.Contains("altplay")) {
				int num = Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1));
				return UI.playingMusic == num;
			}
			switch (Id) {
				case "expand":
					return !UI.smallPanel;
				case "play":
					return UI.playingMusic > -1;
				case "listen":
					return UI.listening;
				case "record":
					return !UI.recording;
				case "viewmode":
					return !UI.viewMode;
				default:
					return false;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			MusicPlayerUI UI = MusicUISystem.MusicUI;
			bool useAlt = UseAlternateTexture();
			int selectedMusic = tMusicPlayer.AllMusic[UI.DisplayBox].music;
			int firstBox = UI.musicData[0].music;
			int lastBox = UI.musicData[UI.musicData.Count - 1].music;
			bool firstOrLast = (Id == "prev" && selectedMusic == firstBox) || (Id == "next" && selectedMusic == lastBox);
			int indexPrev = UI.FindPrevIndex();
			int indexNext = UI.FindNextIndex();
			bool firstOrLastUnavail = (Id == "prev" && (indexPrev == -1 || UI.listening)) || (Id == "next" && (indexNext == -1 || UI.listening));
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			bool recordUnavail = Id == "record" && modplayer.musicBoxesStored <= 0;
			bool activeListen = (Id == "next" || Id == "prev" || Id == "play") && UI.listening;
			bool musicAtZero = Id != "expand" && Id != "view" && Main.musicVolume <= 0f;
			bool clearModDisabled = Id == "clearfiltermod" && UI.FilterMod == "";
			bool cannotPlayListMusic = Id.Contains("altplay") && !UI.canPlay.Contains(Convert.ToInt32(Id.Substring(Id.IndexOf("_") + 1)));
			bool disabled = firstOrLast | firstOrLastUnavail | recordUnavail | activeListen | musicAtZero | clearModDisabled | cannotPlayListMusic;
			Rectangle push = new Rectangle(useAlt ? (src.X + src.Width + 2) : src.X, (IsMouseHovering && !disabled) ? (src.Y + src.Height + 2) : src.Y, src.Width, src.Height);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			spriteBatch.Draw(texture, innerDimensions.ToRectangle(), push, disabled ? new Color(60, 60, 60, 60) : Color.White);
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorEnabled && !disabled) {
					Main.hoverItemName = SetHoverItemName(Id);
				}
				else if (Id == "filtermod") {
					Main.hoverItemName = $"{(UI.FilterMod == "" ? "Filter by Mod" : $"{UI.FilterMod}")}";
				}
			}
		}

		public string SetHoverItemName(string ID)
		{
			MusicPlayerUI UI = MusicUISystem.MusicUI;
			switch (ID) {
				case "expand":
					return (UI.smallPanel ? "Maximize" : "Minimize") ?? "";
				case "play":
					return ((UI.playingMusic >= 0) ? "Stop" : "Play") ?? "";
				case "listen":
					return ((UI.playingMusic >= 0) ? "Stop" : "Play") ?? "";
				case "record":
					return (UI.listening ? "Disable" : "Enable") + " Listening";
				case "prev":
					return "Previous Song";
				case "next":
					return "Next Song";
				case "view":
					return (UI.selectionVisible ? "Close" : "Open") + " Selection List";
				case "sortby":
					return $"Sorted by {(UI.sortType == SortBy.ID ? "ID" : "Name")}";
				case "filtermod":
					return $"{(UI.FilterMod == "" ? "Filter by Mod" : $"{UI.FilterMod}")}";
				case "clearfiltermod":
					return "Clear mod filter";
				case "availability":
					return $"Showing all {(UI.availabililty == ProgressBy.Obtained ? "obtained " : "")}{(UI.availabililty == ProgressBy.Unobtained ? "unobtained " : "")}music boxes";
				case "viewmode":
					return UI.viewMode ? "Change to Grid mode" : "Change to List mode";
				default:
					return "";
			}
		}
	}

	internal class ItemSlotRow : UIElement
	{
		public string Id { get; init; } = "";

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
		public string Id { get; set; } = "";

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
			Width.Set(TextureAssets.InventoryBack.Value.Width * scale, 0f);
			Height.Set(TextureAssets.InventoryBack.Value.Height * scale, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			MusicPlayerUI UI = MusicUISystem.MusicUI;
			bool isSelectionSlot = Id.Contains("SelectionSlot");
			bool isDisplaySlot = Id == "DisplaySlot";
			bool isEntrySlot = Id == "EntrySlot";
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = scale;
			CalculatedStyle dimensions = GetDimensions();
			Rectangle rectangle = dimensions.ToRectangle();
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			if (isDisplaySlot) {
				if (UI.listening) {
					int index = tMusicPlayer.AllMusic.FindIndex((MusicData musicRef) => musicRef.music == Main.curMusic);
					UI.ListenDisplay = index;
					if (index != -1) {
						itemID = tMusicPlayer.AllMusic[index].musicbox;
					}
				}
				if (!UI.listening || UI.ListenDisplay == -1) {
					UI.ListenDisplay = -1;
					itemID = tMusicPlayer.AllMusic[UI.DisplayBox].musicbox;
				}
			}
			if (!isEntrySlot) {
				// TODO: Implement 'IncludeResearch' better?
				bool isResearched = tMusicPlayer.tMPServerConfig.IncludeResearched && modplayer.Player.difficulty == PlayerDifficultyID.Creative && modplayer.Player.creativeTracker.ItemSacrifices.SacrificesCountByItemIdCache.ContainsKey(itemID);
				bool HasMusicBox = modplayer.MusicBoxList.Any(item => item.Type == itemID);
				musicBox.SetDefaults(HasMusicBox || isResearched ? itemID : 0);
			}
			else {
				if (!musicBox.IsAir) {
					if (musicBox.type != ItemID.MusicBox) {
						modplayer.MusicBoxList.Add(new ItemDefinition(musicBox.type));
						MusicUISystem.MusicUI.canPlay.Add(musicBox.type);
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
					if (Main.mouseLeft) {
						Main.playerInventory = true;
					}
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
			Asset<Texture2D> backup = TextureAssets.InventoryBack2;
			TextureAssets.InventoryBack2 = (isEntrySlot ? TextureAssets.InventoryBack7 : TextureAssets.InventoryBack3);
			ItemSlot.Draw(spriteBatch, ref musicBox, context, Utils.TopLeft(rectangle));
			TextureAssets.InventoryBack2 = backup;
			Main.inventoryScale = oldScale;
			if (isSelectionSlot) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.musicbox == refItem);
				int musicID = tMusicPlayer.AllMusic[index].music;
				if (musicBox.type == refItem) {
					if (modplayer.MusicBoxList.All(x => x.Type != refItem)) {
						modplayer.MusicBoxList.Add(new ItemDefinition(refItem));
						UI.canPlay.Add(musicID);
						tMusicPlayer.SendDebugText($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{refItem}]", Colors.RarityGreen);
					}
					if (IsMouseHovering && Main.mouseRight && Id.Contains("Grid")) {
						UI.ListenDisplay = -1;
						UI.listening = false;
						UI.DisplayBox = index;
						UI.playingMusic = musicID;
					}
				}
				else if (musicBox.IsAir && ContainsPoint(Main.MouseScreen) && modplayer.MusicBoxList.Any(x => x.Type == refItem)) {
					modplayer.MusicBoxList.RemoveAll(x => x.Type == refItem);
					UI.canPlay.Remove(musicID); // = tMusicPlayer.tMPConfig.EnableAllMusicBoxes;
					tMusicPlayer.SendDebugText($"Removed Music Box [ID#{refItem}]", Color.IndianRed);
					if (!UI.canPlay.Contains(musicID)) {
						int next = UI.FindNextIndex();
						int prev = UI.FindPrevIndex();
						if (next != -1) {
							UI.DisplayBox = next;
						}
						else if (prev != -1) {
							UI.DisplayBox = prev;
						}
						else {
							UI.playingMusic = -1;
							UI.listening = true;
						}
					}
				}
			}
			if (musicBox.IsAir) {
				int type;
				if (isDisplaySlot) {
					if (UI.listening && Main.musicVolume > 0f && UI.ListenDisplay != -1) {
						type = tMusicPlayer.AllMusic[UI.ListenDisplay].musicbox;
					}
					else {
						if (UI.DisplayBox == -1) {
							return;
						}
						type = tMusicPlayer.AllMusic[UI.DisplayBox].musicbox;
					}
				}
				else {
					type = refItem;
				}
				if (type > 0) {
					Texture2D texture = ModContent.Request<Texture2D>($"Terraria/Images/Item_{type}", AssetRequestMode.ImmediateLoad).Value;
					float x2 = (rectangle.X + rectangle.Width / 2) - texture.Width * scale / 2f;
					float y2 = (rectangle.Y + rectangle.Height / 2) - texture.Height * scale / 2f;
					spriteBatch.Draw(texture, new Vector2(x2, y2), texture.Bounds, new Color(75, 75, 75, 75), 0f, Vector2.Zero, scale, 0, 0f);
				}
			}
			return;
		}
	}

	internal class NewUITextBox : UITextBox
	{
		public string Id { get; init; } = "";

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
			Texture2D searchbar = ModContent.Request<Texture2D>("tMusicPlayer/UI/search").Value;
			spriteBatch.Draw(searchbar, GetDimensions().Position(), Color.White);
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
			if (focused) {
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString);
				if (FontAssets.MouseText.Value.MeasureString(newString.ToLower()).X < Width.Pixels - 10f) {
					if (!newString.Equals(currentString)) {
						// Stops the user from typing a name that doesn't exist. Prevents a gamebreaking hard-lock.
						bool nameCheck = false;
						foreach (MusicData item in tMusicPlayer.AllMusic.ToArray()) {
							if (item.name.ToLower().Contains(newString)) {
								nameCheck = true;
								break;
							}
						}
						if (nameCheck) {
							currentString = newString.ToLower();
						}
						else {
							newString = currentString;
                        }
					}
					MusicPlayerUI UI = MusicUISystem.MusicUI;
					if (currentString.Length >= 0) {
						List<MusicData> musicData = new List<MusicData>();
						foreach (MusicData item in tMusicPlayer.AllMusic.ToArray()) {
							if (item.name.ToLower().Contains(currentString)) {
								musicData.Add(item);
							}
						}
						UI.musicData = musicData;
						UI.OrganizeSelection(UI.sortType, UI.availabililty, UI.FilterMod);
					}
					else {
						UI.OrganizeSelection(UI.sortType, UI.availabililty, UI.FilterMod);
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
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.MouseText.Value, hintText, drawPos, color2);
			}
			else {
				color2 *= 0.8f;
				DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.MouseText.Value, displayString, drawPos, color2);
			}
		}
	}

	internal class ListenStorageSlot : UIImage
	{
		public string Id { get; init; } = "";

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
					SoundEngine.PlaySound(SoundID.Grab);
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
						$"Up to {tMusicPlayer.tMPServerConfig.MaxStorage} music boxes can be held at once\n" + 
						"Right click to take out one music box\nLeft click to take all of them out";
				}
			}
			base.DrawSelf(spriteBatch);
			Vector2 pos = new Vector2(rectangle.X + Width.Pixels * 0.75f, rectangle.Y + Height.Pixels * 0.75f);
			Utils.DrawBorderString(spriteBatch, modplayer.musicBoxesStored.ToString(), pos, Color.White, 0.85f);
		}
	}

}
