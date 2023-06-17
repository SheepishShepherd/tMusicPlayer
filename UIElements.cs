using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class BackDrop : UIImage {
		public string Id { get; init; } = "";

		public bool dragging;
		private Vector2 offset;

		public BackDrop(Asset<Texture2D> texture) : base(texture) {
			Width.Set(texture.Value.Width, 0f);
			Height.Set(texture.Value.Height, 0f);
		}

		public void UpdatePanelDimensions(bool miniMode) {
			if (Id != "MusicPlayerPanel")
				return; // Only the player panel changes in size

			SetImage(miniMode ? MusicPlayerUI.panelMini : MusicPlayerUI.panelPlayer);
			Width.Set(miniMode ? MusicPlayerUI.panelMini.Value.Width : MusicPlayerUI.panelPlayer.Value.Width, 0f);
			Height.Set(miniMode ? MusicPlayerUI.panelMini.Value.Height : MusicPlayerUI.panelPlayer.Value.Height, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			Rectangle rect = GetInnerDimensions().ToRectangle();
			
			base.Draw(spriteBatch);
			if (Id == "MusicPlayerPanel" && !UI.smallPanel) {
				int musicBoxDisplayed = (UI.listening && UI.ListenDisplay != -1) ? UI.ListenDisplay : UI.DisplayBox;
				MusicData musicRef = MusicUISystem.Instance.AllMusic[musicBoxDisplayed];
				Vector2 pos = new Vector2(rect.X + 64, rect.Y + 10);
				Utils.DrawBorderString(spriteBatch, musicRef.name, pos, Color.White, 0.75f);
				pos = new Vector2(rect.X + 64, rect.Y + 30);
				Utils.DrawBorderString(spriteBatch, musicRef.Mod_DisplayName_NoChatTags(), pos, Color.White, 0.75f);
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
			if (Elements.All((UIElement x) => !x.ContainsPoint(Main.MouseScreen)))
				DragStart(evt);
		}

		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
			if (dragging)
				DragEnd(evt);
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

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			base.ScrollWheel(evt);
			if (this.Parent != null && this.Parent.IsMouseHovering)
				this.ViewPosition -= (float)evt.ScrollWheelValue / 5; // hovering over the scroll bar will make the scoll slower
		}
	}
	
	internal class HoverButton : UIElement {
		public string Id { get; init; } = "";
		public int refNum = -1; // used for the altplay feature

		internal Texture2D texture;
		internal Rectangle src;

		public HoverButton(Texture2D texture, Point coord) {
			this.texture = texture;
			if (texture == MusicPlayerUI.buttonTextures.Value) {
				Width.Set(22f, 0f);
				Height.Set(22f, 0f);
			}
			else if (texture == MusicPlayerUI.closeTextures.Value) {
				Width.Set(18f, 0f);
				Height.Set(18f, 0f);
			}
			this.src = new Rectangle((int)(coord.X * (Width.Pixels + 2)), (int)(coord.Y * (Height.Pixels + 2)), (int)Width.Pixels, (int)Height.Pixels);
		}

		public override void MouseOver(UIMouseEvent evt) {
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			bool hovering = ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface;

			Rectangle push = new Rectangle(src.X + UseAlternateTexture(), (hovering && !Disabled()) ? (src.Y + src.Height + 2) : src.Y, src.Width, src.Height);
			spriteBatch.Draw(texture, GetInnerDimensions().ToRectangle(), push, Disabled() ? new Color(60, 60, 60, 60) : Color.White);

			if (hovering) {
				Main.LocalPlayer.mouseInterface = true;
				if (tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorIsUsed && !Disabled()) {
					MusicUISystem.Instance.UIHoverText = SetHoverItemName(Id);
				}
				else if (Id == "filtermod") {
					string value = ModLoader.TryGetMod(UI.FilterMod, out Mod mod) ? MusicUISystem.Instance.AllMusic.Find(x => x.Mod == UI.FilterMod).Mod_DisplayName_NoChatTags() : UI.FilterMod;
					MusicUISystem.Instance.UIHoverText = $"{(string.IsNullOrEmpty(value) ? "Filter by Mod" : value)}";
				}
			}
		}

		public bool Disabled() {
			MusicPlayerPlayer modplayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			return Id switch {
				"altplay" => !MusicUISystem.Instance.AllMusic[refNum].canPlay || Main.musicVolume <= 0f,
				"clearfiltermod" => UI.FilterMod == "",
				"listen" => Main.musicVolume <= 0f,
				"next" => UI.FindNextIndex() == -1 || UI.listening || Main.musicVolume <= 0f,
				"play" => UI.listening || Main.musicVolume <= 0f,
				"prev" => UI.FindPrevIndex() == -1 || UI.listening || Main.musicVolume <= 0f,
				"record" => modplayer.musicBoxesStored <= 0 || Main.musicVolume <= 0f,
				_ => false,
			};
		}

		public int UseAlternateTexture() {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			return Id switch {
				"altplay" => UI.playingMusic == MusicUISystem.Instance.AllMusic[refNum].MusicID ? 24 : 0,
				"availability" => 24 * (int)UI.availabililty,
                "expand" => !UI.smallPanel ? 20 : 0,
                "listen" => UI.listening ? 24 : 0,
				"play" => UI.playingMusic > -1 ? 24 : 0,
				"record" => !UI.recording ? 24 : 0,
                "viewmode" => !UI.viewMode ? 24 : 0,
                _ => 0,
            };
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

		public ItemSlotRow(int order) {
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
		public bool IsDisplaySlot { get; init; } = false;
		public bool IsEntrySlot { get; init; } = false;
		public bool IsSelectionSlot { get; init; } = false;

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
			Width.Set((int)(TextureAssets.InventoryBack.Value.Width * scale), 0f);
			Height.Set((int)(TextureAssets.InventoryBack.Value.Height * scale), 0f);
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
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = scale;
			Rectangle rectangle = GetDimensions().ToRectangle();
			Player player = Main.LocalPlayer;
			MusicPlayerPlayer modplayer = player.GetModPlayer<MusicPlayerPlayer>();
			if (IsDisplaySlot) {
				if (UI.listening) {
					int index = MusicUISystem.Instance.AllMusic.FindIndex((MusicData musicRef) => musicRef.MusicID == Main.curMusic);
					UI.ListenDisplay = index;
					if (index != -1) {
						displayID = MusicUISystem.Instance.AllMusic[index].MusicBox;
					}
				}
				if (!UI.listening || UI.ListenDisplay == -1) {
					UI.ListenDisplay = -1;
					displayID = MusicUISystem.Instance.AllMusic[UI.DisplayBox].MusicBox;
				}
			}

			if (IsDisplaySlot && (modplayer.BoxIsCollected(displayID) || modplayer.BoxResearched(displayID))) {
				musicBox.SetDefaults(displayID);
			}
			else if (IsSelectionSlot) {
				musicBox.SetDefaults(modplayer.BoxIsCollected(slotItemID) || modplayer.BoxResearched(slotItemID) ? slotItemID : 0);
			}
			else if (IsEntrySlot) {
				if (!musicBox.IsAir) {
					if (musicBox.type != ItemID.MusicBox) {
						modplayer.MusicBoxList.Add(new ItemDefinition(musicBox.type));
						int index = MusicUISystem.Instance.AllMusic.FindIndex(x => x.MusicBox == musicBox.type);
						if (index != -1)
							MusicUISystem.Instance.AllMusic[index].canPlay = true;
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
				else if (IsDisplaySlot && UI.smallPanel) {
					MusicUISystem.Instance.UIHoverText = $"{UI.VisualBoxDisplayed().name}\n{UI.VisualBoxDisplayed().Mod_DisplayName_NoChatTags()}";
					MusicUISystem.Instance.UIHoverTextColor = ItemRarity.GetColor(musicBox.rare);
				}
				else {
					if (IsSelectionSlot && (Main.mouseItem.type == slotItemID || Main.mouseItem.IsAir) && !Main.mouseRight) {
						if (Main.mouseLeft) {
							Main.playerInventory = true;
						}
						ItemSlot.Handle(ref musicBox, context);
					}
					else if (IsEntrySlot) {
						int mouseType = Main.mouseItem.type;
						if (mouseType != 0) {
							bool ValidEntryBox = !modplayer.BoxIsCollected(mouseType) && MusicUISystem.Instance.AllMusic.Any(y => y.MusicBox == mouseType);
							bool isUnrecordedAndNotMax = mouseType == ItemID.MusicBox && modplayer.musicBoxesStored < MusicUISystem.MaxUnrecordedBoxes;
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
			TextureAssets.InventoryBack2 = IsEntrySlot ? TextureAssets.InventoryBack7 : TextureAssets.InventoryBack3;

			if (IsSelectionSlot && modplayer.BoxIsFavorited(slotItemID)) {
				TextureAssets.InventoryBack2 = modplayer.BoxIsCollected(slotItemID) || modplayer.BoxResearched(slotItemID) ? TextureAssets.InventoryBack6 : backup;
			}

			ItemSlot.Draw(spriteBatch, ref musicBox, context, Utils.TopLeft(rectangle));
			if (IsEntrySlot && musicBox.IsAir) {
				// Draw the numerical value of boxes stored
				float textScale = 0.85f;
				string text = modplayer.musicBoxesStored.ToString();
				Vector2 pos = new Vector2((int)(rectangle.Right - (FontAssets.MouseText.Value.MeasureString(text).X * textScale)) - 4, rectangle.Top + 2);
				Color textColor = new Color(150, 150, 150, 50);
				if (modplayer.musicBoxesStored == MusicUISystem.MaxUnrecordedBoxes) {
					textColor = new Color(150, 150, 50, 50);
				}
				else if (modplayer.musicBoxesStored == 0) {
					textColor = new Color(150, 50, 50, 50);
				}

				Utils.DrawBorderString(spriteBatch, text, pos, textColor, textScale);
			}

			if (IsSelectionSlot && modplayer.BoxIsFavorited(slotItemID)) {
				Texture2D texture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Rank_Light", AssetRequestMode.ImmediateLoad).Value;
				Rectangle pos = new Rectangle(rectangle.X + rectangle.Width - texture.Width, rectangle.Y, texture.Width, texture.Height);
				spriteBatch.Draw(texture, pos, Color.White);
			}

			TextureAssets.InventoryBack2 = backup;
			Main.inventoryScale = oldScale;

			if (IsSelectionSlot) {
				int index = MusicUISystem.Instance.AllMusic.FindIndex(x => x.MusicBox == slotItemID);
				MusicData musicData = MusicUISystem.Instance.AllMusic[index];
				if (musicBox.type == slotItemID) {
					if (!modplayer.BoxIsCollected(slotItemID)) {
						modplayer.MusicBoxList.Add(new ItemDefinition(slotItemID));
						musicData.canPlay = true;
						tMusicPlayer.SendDebugText($"Added [c/{Utils.Hex3(Color.DarkSeaGreen)}:{musicBox.Name}] [ID#{slotItemID}]", Colors.RarityGreen);
					}
					if (IsMouseHovering && Main.mouseRight && !UI.viewMode) {
						UI.ListenDisplay = -1;
						UI.listening = false;
						UI.DisplayBox = index;
						UI.playingMusic = musicData.MusicID;
					}
				}
				else if (musicBox.IsAir && ContainsPoint(Main.MouseScreen) && modplayer.BoxIsCollected(slotItemID)) {
					modplayer.MusicBoxList.RemoveAll(x => x.Type == slotItemID);
					tMusicPlayer.SendDebugText($"Removed Music Box [ID#{slotItemID}]", Color.IndianRed);
					if (!modplayer.BoxResearched(slotItemID)) {
						musicData.canPlay = false;
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
				if (IsDisplaySlot) {
					if (UI.listening && Main.musicVolume > 0f && UI.ListenDisplay != -1) {
						type = MusicUISystem.Instance.AllMusic[UI.ListenDisplay].MusicBox;
					}
					else {
						if (UI.DisplayBox == -1) {
							return;
						}
						type = MusicUISystem.Instance.AllMusic[UI.DisplayBox].MusicBox;
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

		private readonly Asset<Texture2D> searchBar = ModContent.Request<Texture2D>("tMusicPlayer/UI/search");

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
			if (JustPressed(Keys.Tab) || JustPressed(Keys.Escape) || (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight))) {
				Unfocus(); // Unfocus search bar when pressing Tab or Esc, or when clicking outside the input box
			}

			if (JustPressed(Keys.Enter)) {
				Main.drawingPlayerChat = false;
				Unfocus(); // Pressing enter will also unfocus the search bar, but will also close player chat
			}

			base.Update(gameTime);
		}

		public void ClearSearchText() {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			SetText("");
			UI.musicData = new List<MusicData>(MusicUISystem.Instance.AllMusic);
			UI.OrganizeSelection();
		}

		public override void SetText(string text, float textScale, bool large) {
			if (text == null)
				return;

			if (text.ToString().Length > _maxLength)
				text = text.ToString().Substring(0, _maxLength);

			if (currentString != text)
				currentString = text;
		}

		private static bool JustPressed(Keys key) => Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
				Main.LocalPlayer.mouseInterface = true;

			spriteBatch.Draw(searchBar.Value, GetDimensions().Position(), Color.White);

			if (focused) {
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(currentString).ToLower();
				if (FontAssets.MouseText.Value.MeasureString(newString).X < Width.Pixels - 10f) {
					if (!newString.Equals(currentString)) {
						// Check for a music box that contains the searchbar text within its name.
						// This will stop the user from typing a name that doesn't exist preventing a hard-lock.
						// If there is an existing music box name with the new text, update the search filter selection to reflect it
						List<MusicData> searchedData = MusicUISystem.Instance.AllMusic.Where(data => data.name.ToLower().Contains(newString)).ToList();
						if (searchedData.Count > 0) {
							currentString = newString;
							MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
							UI.musicData = searchedData;
							UI.OrganizeSelection();
						}
					}
				}

				if (++textBlinkerCount >= 20) {
					textBlinkerState = (textBlinkerState + 1) % 2;
					textBlinkerCount = 0;
				}
				Main.instance.DrawWindowsIMEPanel(new Vector2(198f, Main.screenHeight - 36), 0f);
			}

			// Draws the appropriate text in the search bar
			string displayString = currentString;
			if (textBlinkerState == 1 && focused)
				displayString += "|";

			string searchText = currentString.Length == 0 && !focused ? hintText : displayString;
			Color faded = Color.White * (currentString.Length == 0 && !focused ? 0.6f :  0.8f);
			Vector2 drawPos = GetDimensions().Position() + new Vector2(32f, 3f);
			DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.MouseText.Value, searchText, drawPos, faded);
		}
	}
}
