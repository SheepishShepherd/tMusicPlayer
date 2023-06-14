using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class BackDrop : UIElement {
		public string Id { get; init; } = "";

		public bool dragging;
		private Vector2 offset;

		public BackDrop() { }

		public override void Draw(SpriteBatch spriteBatch) {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			Rectangle rect = GetInnerDimensions().ToRectangle();
			if (Id == "SelectionPanel") {
				// Draw the panel backdrop
				Texture2D texture = MusicPlayerUI.panelTextures[3].Value;
				spriteBatch.Draw(texture, rect, Color.White);
			}
			else if (Id == "MusicPlayerPanel") {
				// Draw the panel backdrop, depending on what size the player has selected
				Texture2D texture = UI.smallPanel ? MusicPlayerUI.panelTextures[1].Value : MusicPlayerUI.panelTextures[2].Value;
				spriteBatch.Draw(texture, rect, Color.White);
			}
			else if (Id == "MusicEntry") {
				// Draw the panel backdrop
				Texture2D obj = MusicPlayerUI.panelTextures[1].Value;
				Rectangle pos = new Rectangle(rect.X + obj.Bounds.Height + 2, rect.Y, obj.Width, obj.Height);
				spriteBatch.Draw(obj, pos, obj.Bounds, Color.White, MathHelper.ToRadians(90), Vector2.Zero, SpriteEffects.None, 0f);

				// Draw an empty music box to display how many the player has stored
				Texture2D texture = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.MusicBox}", AssetRequestMode.ImmediateLoad).Value;
				pos = new Rectangle((int)(Left.Pixels + Width.Pixels / 2), (int)(Top.Pixels + Height.Pixels - texture.Height * 1.5f), texture.Width, texture.Height);
				spriteBatch.Draw(texture, pos, Color.White);

				// Draw the numerical value of boxes stored
				Vector2 pos2 = new Vector2(pos.X + texture.Width - 15, pos.Y + texture.Height - 10);
				Utils.DrawBorderString(spriteBatch, Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>().musicBoxesStored.ToString(), pos2, Color.White, 0.85f);
			}
			base.Draw(spriteBatch);
			if (Id == "MusicPlayerPanel" && !UI.smallPanel) {
				int musicBoxDisplayed = (UI.listening && UI.ListenDisplay != -1) ? UI.ListenDisplay : UI.DisplayBox;
				MusicData musicRef = tMusicPlayer.AllMusic[musicBoxDisplayed];
				Vector2 pos = new Vector2(rect.X + 64, rect.Y + 10);
				Utils.DrawBorderString(spriteBatch, musicRef.name, pos, Color.White, 0.75f);
				pos = new Vector2(rect.X + 64, rect.Y + 30);
				Utils.DrawBorderString(spriteBatch, musicRef.mod, pos, Color.White, 0.75f);
			}
			
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				// Needed to remove mousetext from outside sources when using the UI
				Main.player[Main.myPlayer].mouseInterface = true;
				Main.mouseText = true;
				// Item icons such as hovering over a bed will not appear
				Main.LocalPlayer.cursorItemIconEnabled = false;
				Main.LocalPlayer.cursorItemIconID = -1;
				Main.ItemIconCacheUpdate(0);
			}
		}

		public override void LeftMouseDown(UIMouseEvent evt) {
			base.LeftMouseDown(evt);
			if (Elements.All((UIElement x) => !x.IsMouseHovering)) {
				DragStart(evt);
			}
		}

		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
			if (dragging) {
				DragEnd(evt);
			}
		}

		private void DragStart(UIMouseEvent evt) {
			CalculatedStyle dimensions2 = GetDimensions();
			Rectangle dimensions = dimensions2.ToRectangle();
			offset = new Vector2(evt.MousePosition.X - dimensions.Left, evt.MousePosition.Y - dimensions.Top);
			dragging = true;
		}

		private void DragEnd(UIMouseEvent evt) {
			Vector2 end = evt.MousePosition;
			dragging = false;
			Left.Set(end.X - offset.X, 0f);
			Top.Set(end.Y - offset.Y, 0f);
			Recalculate();
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (IsMouseHovering) {
				PlayerInput.LockVanillaMouseScroll("tMusicPlayer panel");
			}
			if (dragging) {
				Left.Set(Main.mouseX - offset.X, 0f);
				Top.Set(Main.mouseY - offset.Y, 0f);
				Recalculate();
			}

			// Look into this. What is it doing exactly? Can I make this more efficient?
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

	internal class FixedUIScrollbar : UIScrollbar {
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = MusicUISystem.Instance.MP_UserInterface;
			base.DrawSelf(spriteBatch);
			UserInterface.ActiveInstance = temp;
		}

		public override void LeftMouseDown(UIMouseEvent evt) {
			UserInterface temp = UserInterface.ActiveInstance;
			UserInterface.ActiveInstance = MusicUISystem.Instance.MP_UserInterface;
			base.LeftMouseDown(evt);
			UserInterface.ActiveInstance = temp;
		}
	}
	
	internal class HoverButton : UIImage {
		public string Id { get; init; } = "";
		public int refNum = -1; // used for the altplay feature

		internal Texture2D texture;
		internal Rectangle src;

		public HoverButton(Texture2D texture, Rectangle src) : base(texture) {
			this.texture = texture;
			this.src = src;
		}

		public int UseAlternateTexture() {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			if (Id == "altplay") {
				return UI.playingMusic == refNum ? 24 : 0;
			}
			else if (Id == "availability") {
				return 24 * (int)UI.availabililty;
			}

			return Id switch {
                "expand" => !UI.smallPanel ? 20 : 0,
                "play" => UI.playingMusic > -1 ? 24 : 0,
                "listen" => UI.listening ? 24 : 0,
                "record" => !UI.recording ? 24 : 0,
                "viewmode" => !UI.viewMode ? 24 : 0,
                _ => 0,
            };
        }

		public override void Draw(SpriteBatch spriteBatch) {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			int selectedMusic = tMusicPlayer.AllMusic[UI.DisplayBox].music;

			int firstBox = UI.musicData[0].music;
			int lastBox = UI.musicData[UI.musicData.Count - 1].music;
			bool firstOrLast = (Id == "prev" && selectedMusic == firstBox) || (Id == "next" && selectedMusic == lastBox);
			bool firstOrLastUnavail = (Id == "prev" && (UI.FindPrevIndex() == -1 || UI.listening)) || (Id == "next" && (UI.FindNextIndex() == -1 || UI.listening));
			bool recordUnavail = Id == "record" && modplayer.musicBoxesStored <= 0;
			bool activeListen = (Id == "next" || Id == "prev" || Id == "play") && UI.listening;
			bool musicAtZero = (Id == "next" || Id == "prev" || Id == "play" || Id == "listen" || Id == "record" || Id == "altplay") && Main.musicVolume <= 0f;
			bool clearModDisabled = Id == "clearfiltermod" && UI.FilterMod == "";
			bool cannotPlayListMusic = Id == "altplay" && !UI.canPlay[refNum];
			bool disabled = firstOrLast | firstOrLastUnavail | recordUnavail | activeListen | musicAtZero | clearModDisabled | cannotPlayListMusic;

			Rectangle push = new Rectangle(src.X + UseAlternateTexture(), (IsMouseHovering && !disabled) ? (src.Y + src.Height + 2) : src.Y, src.Width, src.Height);
			spriteBatch.Draw(texture, GetInnerDimensions().ToRectangle(), push, disabled ? new Color(60, 60, 60, 60) : Color.White);

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorIsUsed && !disabled) {
					MusicUISystem.Instance.UIHoverText = SetHoverItemName(Id);
				}
				else if (Id == "filtermod") {
					MusicUISystem.Instance.UIHoverText = $"{(UI.FilterMod == "" ? "Filter by Mod" : $"{UI.FilterMod}")}";
				}
			}
		}

		public string SetHoverItemName(string ID) {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			if (Id == "availability") {
				return UI.availabililty switch {
					ProgressBy.Obtained => "Showing obtained music boxes",
					ProgressBy.Unobtained => "Showing unobtained music boxes",
					_ => "No availability filter"
				};
			}

            return ID switch {
                "expand" => (UI.smallPanel ? "Maximize" : "Minimize") ?? "",
                "play" => ((UI.playingMusic >= 0) ? "Stop" : "Play") ?? "",
                "listen" => ((UI.playingMusic >= 0) ? "Stop" : "Play") ?? "",
                "record" => (UI.listening ? "Disable" : "Enable") + " Listening",
                "prev" => "Previous Song",
                "next" => "Next Song",
                "view" => (UI.SelectionPanelVisible ? "Close" : "Open") + " Selection List",
				"showFavorites" => "Show favorited music",
				"sortbyid" => "Sort by ID",
                "sortbyname" => "Sort by name",
                "filtermod" => $"{(UI.FilterMod == "" ? "Filter by Mod" : $"{UI.FilterMod}")}",
                "clearfiltermod" => "Clear mod filter",
                "viewmode" => UI.viewMode ? "Change to Grid mode" : "Change to List mode",
				"ejectMusicBoxes" =>
						"Stored music boxes can record songs if recording is enabled\n" +
						"Up to 20 music boxes can be held at once\n" +
						"Left-click to eject one music box into your inventory\n" +
						"Right click to place all of your stored music boxes in your inventory",
                _ => ""
            };
        }
	}

	internal class ItemSlotRow : UIElement {
		private int order;

		public ItemSlotRow(int order, float width, float height) {
			this.order = order;
			Width.Pixels = 400f;
			Height.Pixels = 50f;
		}

		public override int CompareTo(object obj) {
			ItemSlotRow other = obj as ItemSlotRow;
			return order.CompareTo(other.order);
		}
	}

	internal class MusicBoxSlot : UIElement {
		public string Id { get; set; } = "";

		internal Item musicBox; // The item in this slot
		internal int displayID; // What the slot should display (only for the Display Slot in the small musicplayer)
		internal int slotItemID; // What the item id the slot is assigned to
		private readonly int context;
		private readonly float scale;

		public MusicBoxSlot(int refItem, float scale) {
			context = 4;
			this.scale = scale;
			displayID = refItem;
			this.slotItemID = refItem;
			musicBox = new Item();
			musicBox.SetDefaults(0, false);
			Width.Set(TextureAssets.InventoryBack.Value.Width * scale, 0f);
			Height.Set(TextureAssets.InventoryBack.Value.Height * scale, 0f);
		}

		public override void LeftClick(UIMouseEvent evt) {
			if (Main.keyState.IsKeyDown(Keys.LeftAlt)) {

				MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
				if (modplayer.BoxIsFavorited(slotItemID)) {
					modplayer.MusicBoxFavs.RemoveAll(x => x.Type == slotItemID);
				}
				else {
					modplayer.MusicBoxFavs.Add(new ItemDefinition(slotItemID));
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch) {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			bool isSelectionSlot = Id.Contains("SelectionSlot");
			bool isDisplaySlot = Id == "DisplaySlot";
			bool isEntrySlot = Id == "EntrySlot";
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = scale;
			Rectangle rectangle = GetDimensions().ToRectangle();
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();
			if (isDisplaySlot) {
				if (UI.listening) {
					int index = tMusicPlayer.AllMusic.FindIndex((MusicData musicRef) => musicRef.music == Main.curMusic);
					UI.ListenDisplay = index;
					if (index != -1) {
						displayID = tMusicPlayer.AllMusic[index].musicbox;
					}
				}
				if (!UI.listening || UI.ListenDisplay == -1) {
					UI.ListenDisplay = -1;
					displayID = tMusicPlayer.AllMusic[UI.DisplayBox].musicbox;
				}
			}

			if (isDisplaySlot && (modplayer.BoxIsCollected(displayID) || modplayer.BoxResearched(displayID))) {
				musicBox.SetDefaults(displayID);
			}
			else if (isSelectionSlot) {
				musicBox.SetDefaults(modplayer.BoxIsCollected(slotItemID) || modplayer.BoxResearched(slotItemID) ? slotItemID : 0);
			}
			else if (isEntrySlot) {
				if (!musicBox.IsAir) {
					if (musicBox.type != ItemID.MusicBox) {
						modplayer.MusicBoxList.Add(new ItemDefinition(musicBox.type));
						UI.canPlay[tMusicPlayer.AllMusic.FindIndex(x => x.musicbox == musicBox.type)] = true;
						tMusicPlayer.SendDebugText($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{slotItemID}]", Colors.RarityGreen);
					}
					else if (modplayer.musicBoxesStored < MusicUISystem.MaxUnrecordedBoxes) {
						modplayer.musicBoxesStored++;
					}
					musicBox.TurnToAir();
				}
			}

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				player.mouseInterface = true;
				if (Main.keyState.IsKeyDown(Keys.LeftAlt)) {
					Main.cursorOverride = 3; 
				}
				else {
					if (isSelectionSlot && (Main.mouseItem.type == slotItemID || Main.mouseItem.IsAir) && !Main.mouseRight) {
						if (Main.mouseLeft) {
							Main.playerInventory = true;
						}
						ItemSlot.Handle(ref musicBox, context);
					}
					else if (isEntrySlot) {
						int mouseType = Main.mouseItem.type;
						if (mouseType != 0) {
							bool ValidEntryBox = !modplayer.BoxIsCollected(mouseType) && tMusicPlayer.AllMusic.Any(y => y.musicbox == mouseType);
							bool isUnrecordedAndNotMax = mouseType == 576 && modplayer.musicBoxesStored < MusicUISystem.MaxUnrecordedBoxes;
							if (ValidEntryBox | isUnrecordedAndNotMax) {
								ItemSlot.Handle(ref musicBox, context);
							}
						}
						else {
							ItemSlot.Handle(ref musicBox, context);
						}
						if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorIsUsed) {
							MusicUISystem.Instance.UIHoverText = "Insert a music box you do not already own!";
						}
					}
				}
			}

			Asset<Texture2D> backup = TextureAssets.InventoryBack2;
			TextureAssets.InventoryBack2 = isEntrySlot ? TextureAssets.InventoryBack7 : TextureAssets.InventoryBack3;

			if (isSelectionSlot && modplayer.BoxIsFavorited(slotItemID)) {
				TextureAssets.InventoryBack2 = modplayer.BoxIsCollected(slotItemID) || modplayer.BoxResearched(slotItemID) ? TextureAssets.InventoryBack6 : backup;
			}

			ItemSlot.Draw(spriteBatch, ref musicBox, context, Utils.TopLeft(rectangle));

			if (isSelectionSlot && modplayer.BoxIsFavorited(slotItemID)) {
				Texture2D texture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Rank_Light", AssetRequestMode.ImmediateLoad).Value;
				Rectangle pos = new Rectangle(rectangle.X + rectangle.Width - texture.Width, rectangle.Y, texture.Width, texture.Height);
				spriteBatch.Draw(texture, pos, Color.White);
			}

			TextureAssets.InventoryBack2 = backup;
			Main.inventoryScale = oldScale;

			if (isSelectionSlot) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.musicbox == slotItemID);
				int musicID = tMusicPlayer.AllMusic[index].music;
				if (musicBox.type == slotItemID) {
					if (!modplayer.BoxIsCollected(slotItemID)) {
						modplayer.MusicBoxList.Add(new ItemDefinition(slotItemID));
						UI.canPlay[index] = true;
						tMusicPlayer.SendDebugText($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{slotItemID}]", Colors.RarityGreen);
					}
					if (IsMouseHovering && Main.mouseRight && Id.Contains("Grid")) {
						UI.ListenDisplay = -1;
						UI.listening = false;
						UI.DisplayBox = index;
						UI.playingMusic = musicID;
					}
				}
				else if (musicBox.IsAir && ContainsPoint(Main.MouseScreen) && modplayer.BoxIsCollected(slotItemID)) {
					modplayer.MusicBoxList.RemoveAll(x => x.Type == slotItemID);
					tMusicPlayer.SendDebugText($"Removed Music Box [ID#{slotItemID}]", Color.IndianRed);
					if (!modplayer.BoxResearched(slotItemID)) {
						UI.canPlay[index] = false;
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
					type = slotItemID;
				}

				if (type > 0) {
					string texturePath = type < ItemID.Count ? $"Terraria/Images/Item_{type}" : ItemLoader.GetItem(type).Texture;
					Texture2D texture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
					float x2 = (rectangle.X + rectangle.Width / 2) - texture.Width * scale / 2f;
					float y2 = (rectangle.Y + rectangle.Height / 2) - texture.Height * scale / 2f;
					spriteBatch.Draw(texture, new Vector2(x2, y2), texture.Bounds, new Color(75, 75, 75, 75), 0f, Vector2.Zero, scale, 0, 0f);
				}
			}
			return;
		}
	}

	internal class SearchBar : UITextBox {
		internal bool focused = false;

		private readonly int _maxLength = 40;
		private readonly string hintText;
		internal string currentString = "";

		private int textBlinkerCount;
		private int textBlinkerState;

		public SearchBar(string hintText, string text = "") : base(text) {
			this.hintText = hintText;
			currentString = text;
			SetPadding(0f);
			BackgroundColor = Color.White;
			BorderColor = Color.White;
		}

		public override void LeftClick(UIMouseEvent evt) {
			Focus();
		}

		public override void RightClick(UIMouseEvent evt) {
			SetText("");
		}

		public void Unfocus() {
			if (focused) {
				focused = false;
				Main.blockInput = false;
			}
		}

		public void Focus() {
			if (!focused) {
				Main.clrInput();
				focused = true;
				Main.blockInput = true;
			}
		}

		public override void Update(GameTime gameTime) {
			Vector2 MousePosition = new Vector2(Main.mouseX, Main.mouseY);
			if (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight)) {
				Unfocus();
			}
			base.Update(gameTime);
		}

		public override void SetText(string text, float textScale, bool large) {
			if (text == null)
				return;

			if (text.ToString().Length > _maxLength) {
				text = text.ToString().Substring(0, _maxLength);
			}
			if (currentString != text) {
				currentString = text;
			}
		}

		private static bool JustPressed(Keys key) => Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);

		protected override void DrawSelf(SpriteBatch spriteBatch) {
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
					MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
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
}
