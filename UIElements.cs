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
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class MusicPlayerElement : UIElement {
		internal MusicPlayerUI UI => MusicUISystem.Instance.MusicUI;

		internal Rectangle Inner => GetInnerDimensions().ToRectangle();

		internal Player LocalPlayer => Main.LocalPlayer;

		internal MusicPlayerPlayer LocalModPlayer => LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
	}

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
			if (Id == "MusicPlayerPanel" && !UI.MiniModePlayer && UI.VisualBoxDisplayed is not null) {
				string Snippet(string text) => text.Length > 22 ? string.Concat(text.AsSpan(0, 22), "...") : text; // limit text size and add '...' for titles too long
				Vector2 pos = new Vector2(rect.X + 64, rect.Y + 10);
				Utils.DrawBorderString(spriteBatch, Snippet(UI.VisualBoxDisplayed.name), pos, ItemRarity.GetColor(UI.DisplayMusicSlot.SlotItem.rare), 0.75f);
				pos = new Vector2(rect.X + 64, rect.Y + 28);
				Utils.DrawBorderString(spriteBatch, Snippet(UI.VisualBoxDisplayed.Mod_DisplayName_NoChatTags()), pos, Color.White, 0.75f);
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
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
				PlayerInput.LockVanillaMouseScroll("tMusicPlayer panel");

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
	
	internal class HoverButton : MusicPlayerElement {
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
			bool hovering = ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface;
			Rectangle push = new Rectangle(src.X + UseAlternateTexture(), (hovering && !Disabled()) ? (src.Y + src.Height + 2) : src.Y, src.Width, src.Height);
			spriteBatch.Draw(texture, Inner, push, Disabled() ? new Color(60, 60, 60, 60) : Color.White);

			if (hovering) {
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
			return Id switch {
				"altplay" => !MusicUISystem.Instance.AllMusic[refNum].CanPlay(LocalModPlayer) || Main.musicVolume <= 0f,
				"clearfiltermod" => UI.FilterMod == "",
				"listen" => Main.musicVolume <= 0f,
				"next" => UI.FindNext() == null || UI.IsListening || Main.musicVolume <= 0f,
				"play" => UI.IsListening || Main.musicVolume <= 0f,
				"prev" => UI.FindPrev() == null || UI.IsListening || Main.musicVolume <= 0f,
				"record" => LocalModPlayer.musicBoxesStored <= 0 || Main.musicVolume <= 0f,
				_ => false,
			};
		}

		public int UseAlternateTexture() {
			return Id switch {
				"altplay" => UI.CurrentlyPlaying == MusicUISystem.Instance.AllMusic[refNum].MusicID ? 24 : 0,
				"availability" => 24 * (int)UI.availabililty,
                "expand" => !UI.MiniModePlayer ? 20 : 0,
                "listen" => UI.IsListening ? 24 : 0,
				"play" => UI.IsPlayingMusic ? 24 : 0,
				"record" => !UI.IsRecording ? 24 : 0,
                "viewmode" => UI.IsGridMode ? 24 : 0,
                _ => 0,
            };
        }

		public string SetHoverItemName(string ID) {
			if (Id == "availability") {
				return UI.availabililty switch {
					ProgressBy.Obtained => "Showing obtained music boxes",
					ProgressBy.Unobtained => "Showing unobtained music boxes",
					_ => "No availability filter"
				};
			}

            return ID switch {
                "expand" => (UI.MiniModePlayer ? "Maximize" : "Minimize") ?? "",
                "play" => (UI.IsPlayingMusic ? "Stop" : "Play") ?? "",
                "listen" => (UI.IsPlayingMusic ? "Stop" : "Play") ?? "",
                "record" => (UI.IsListening ? "Disable" : "Enable") + " Listening",
                "prev" => "Previous Song",
                "next" => "Next Song",
                "view" => (UI.SelectionPanelVisible ? "Close" : "Open") + " Selection List",
				"showFavorites" => "Show favorited music",
				"sortbyid" => "Sort by ID",
                "sortbyname" => "Sort by name",
                "filtermod" => $"{(UI.FilterMod == "" ? "Filter by Mod" : $"{UI.FilterMod}")}",
                "clearfiltermod" => "Clear mod filter",
                "viewmode" => UI.IsListMode ? "Change to Grid mode" : "Change to List mode",
				"ejectMusicBoxes" =>
					"Stored music boxes can record songs if recording is enabled\n" +
					"Up to 20 music boxes can be held at once\n" +
					"Left-click to eject one music box into your inventory\n" +
					"Right click to place all of your stored music boxes in your inventory",
                _ => ""
            };
        }
	}

	internal class ItemSlotRow : MusicPlayerElement {
		private int order;

		public ItemSlotRow(int order) {
			this.order = order;
			Width.Set(UI.SelectionList.Width.Pixels, 0f);
			Height.Set(50f, 0f);
		}

		public override int CompareTo(object obj) {
			ItemSlotRow other = obj as ItemSlotRow;
			return order.CompareTo(other.order);
		}
	}

	internal class MusicBoxSlot : MusicPlayerElement {
		public bool IsDisplaySlot { get; init; } = false;
		public bool IsEntrySlot { get; init; } = false;
		public bool IsSelectionSlot { get; init; } = false;

		internal MusicData SlotMusicData { get; set; } = null;

		internal int SlotItemID => SlotMusicData is null ? ItemID.MusicBox : SlotMusicData.MusicBox; // What the item id the slot is assigned to

		internal Item SlotItem; // The actually item within the slot

		internal Func<Item, bool> ValidItems;

		private readonly int context = ItemSlot.Context.BankItem;
		private readonly float scale;

		public MusicBoxSlot(float scale) {
			this.scale = scale;
			SlotItem = new Item(0);
			Width.Set((int)(TextureAssets.InventoryBack.Value.Width * scale), 0f);
			Height.Set((int)(TextureAssets.InventoryBack.Value.Height * scale), 0f);

			if (scale == 1f) {
				IsDisplaySlot = true;
				ValidItems = (Item item) => false;
			}
			else {
				IsEntrySlot = true;
				ValidItems = delegate (Item item) {
					bool ValidEntryBox = !LocalModPlayer.BoxIsCollected(item.type) && MusicUISystem.Instance.AllMusic.Any(y => y.MusicBox == item.type);
					bool isUnrecordedAndNotMax = item.type == ItemID.MusicBox && LocalModPlayer.musicBoxesStored < MusicUISystem.MaxUnrecordedBoxes;
					return item.IsAir || ValidEntryBox || isUnrecordedAndNotMax;
				};
			}
		}

		public MusicBoxSlot(MusicData musicData) {
			IsSelectionSlot = true;
			ValidItems = (Item item) => item.IsAir || item.type == SlotItemID; 
			this.scale = 0.85f;
			this.SlotMusicData = musicData;
			SlotItem = new Item(0);
			Width.Set((int)(TextureAssets.InventoryBack.Value.Width * scale), 0f);
			Height.Set((int)(TextureAssets.InventoryBack.Value.Height * scale), 0f);
		}

		public override void LeftClick(UIMouseEvent evt) {
			if (IsSelectionSlot && Main.keyState.IsKeyDown(Keys.LeftAlt)) {
				if (LocalModPlayer.BoxIsFavorited(SlotItemID)) {
					LocalModPlayer.MusicBoxFavs.RemoveAll(x => x.Type == SlotItemID);
				}
				else {
					LocalModPlayer.MusicBoxFavs.Add(new ItemDefinition(SlotItemID));
				}
			}
		}

		public override void RightClick(UIMouseEvent evt) {
			if (IsSelectionSlot) {
				if (UI.IsGridMode)
					UI.UpdateMusicPlayedViaSelectionMenu(SlotMusicData); // Right-clicking a slot in grid view will play that music
			}
		}

		public override void Draw(SpriteBatch spriteBatch) {
			// Create the item to fill the slot
			if (IsDisplaySlot && UI.VisualBoxDisplayed is not null) {
				SlotItem.SetDefaults(UI.VisualBoxDisplayed.MusicBox); // set item default to the UI's visual display music box
			}
			else if (IsSelectionSlot) {
				SlotItem.SetDefaults(SlotMusicData.CanPlay(LocalModPlayer) == true ? SlotItemID : 0); // selection slot is either the assigned item or empty
			}
			else if (IsEntrySlot && !SlotItem.IsAir) {
				if (SlotItem.type != ItemID.MusicBox) {
					LocalModPlayer.MusicBoxList.Add(new ItemDefinition(SlotItem.type));
					tMusicPlayer.SendDebugText($"[i:{SlotItem.type}] [#{SlotItem.type}] was added (via entry slot)", Colors.RarityGreen);
				}
				else if (LocalModPlayer.musicBoxesStored < MusicUISystem.MaxUnrecordedBoxes) {
					LocalModPlayer.musicBoxesStored++;
				}
				SlotItem.TurnToAir(); // entry slot should always be empty, but allows unrecorded and unobtained music boxes before turning them into air
			}

			// Item slot drawing
			float oldScale = Main.inventoryScale; // back up these values to change later
			Asset<Texture2D> backup = TextureAssets.InventoryBack2;
			Main.inventoryScale = scale;
			if (IsEntrySlot) {
				TextureAssets.InventoryBack2 = TextureAssets.InventoryBack7;
			}
			else if (IsDisplaySlot) {
				TextureAssets.InventoryBack2 = TextureAssets.InventoryBack3;
			}
			else {
				TextureAssets.InventoryBack2 = SlotMusicData.CanPlay(LocalModPlayer) ? TextureAssets.InventoryBack3 : TextureAssets.InventoryBack4;
			}
			ItemSlot.Draw(spriteBatch, ref SlotItem, context, Inner.TopLeft()); // Draw the item slot!
			TextureAssets.InventoryBack2 = backup; // reset values
			Main.inventoryScale = oldScale;

			// Draws the assigned item in the item slot
			if (SlotItem.IsAir) {
				string texturePath = SlotItemID < ItemID.Count ? $"Terraria/Images/Item_{SlotItemID}" : ItemLoader.GetItem(SlotItemID).Texture;
				Texture2D musicBoxTexture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
				float x2 = Inner.X + Inner.Width / 2 - musicBoxTexture.Width * scale / 2f;
				float y2 = Inner.Y + Inner.Height / 2 - musicBoxTexture.Height * scale / 2f;
				spriteBatch.Draw(musicBoxTexture, new Vector2(x2, y2), musicBoxTexture.Bounds, new Color(75, 75, 75, 75), 0f, Vector2.Zero, scale, 0, 0f);
			}

			// Draws extra bits over the item slot
			// EntrySlots will have the numerical value of boxes stored
			// Selection slots will have a star if the music box was favorite
			if (IsEntrySlot && SlotItem.IsAir) {
				string text = LocalModPlayer.musicBoxesStored.ToString();
				Vector2 pos = new Vector2((int)(Inner.Right - (FontAssets.MouseText.Value.MeasureString(text).X * scale)) - 4, Inner.Top + 2);
				Color textColor = new Color(150, 150, 150, 50);
				if (LocalModPlayer.musicBoxesStored == MusicUISystem.MaxUnrecordedBoxes) {
					textColor = new Color(150, 150, 50, 50);
				}
				else if (LocalModPlayer.musicBoxesStored == 0) {
					textColor = new Color(150, 50, 50, 50);
				}

				Utils.DrawBorderString(spriteBatch, text, pos, textColor, scale);
			}
			else if (IsSelectionSlot && LocalModPlayer.BoxIsFavorited(SlotItemID)) {
				Texture2D texture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Rank_Light", AssetRequestMode.ImmediateLoad).Value;
				Rectangle pos = new Rectangle(Inner.X + Inner.Width - texture.Width, Inner.Y, texture.Width, texture.Height);
				spriteBatch.Draw(texture, pos, Color.White);
			}

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				// Hover Text Handling
				if (IsEntrySlot && tMusicPlayer.tMPConfig.EnableMoreTooltips && Main.SmartCursorIsUsed) {
					MusicUISystem.Instance.UIHoverText = "Insert a music box you do not already own!";
				}
				else if (IsDisplaySlot && UI.MiniModePlayer) {
					MusicUISystem.Instance.UIHoverText = $"{UI.VisualBoxDisplayed.name}\n{UI.VisualBoxDisplayed.Mod_DisplayName_NoChatTags()}";
					MusicUISystem.Instance.UIHoverTextColor = ItemRarity.GetColor(SlotItem.rare);
				}

				// Item & Music Box Handling
				if (IsSelectionSlot && Main.keyState.IsKeyDown(Keys.LeftAlt)) {
					Main.cursorOverride = 3; // Holding alt over a selection slot will allow you to favorite it, instead of picking it up
				}
				else if (!Main.mouseRight && (ValidItems == null || ValidItems(Main.mouseItem))) {
					ItemSlot.Handle(ref SlotItem, context); // right-click disabled

					// Determine if it was added or removed if from a selection slot
					if (IsSelectionSlot) {
						if (SlotItem.type == SlotItemID && !LocalModPlayer.BoxIsCollected(SlotItemID)) {
							LocalModPlayer.MusicBoxList.Add(new ItemDefinition(SlotItemID));
							tMusicPlayer.SendDebugText($"[i:{SlotItemID}] [#{SlotItemID}] was added (via selection panel)", Colors.RarityGreen);
						}
						else if (SlotItem.IsAir && LocalModPlayer.BoxIsCollected(SlotItemID)) {
							Main.playerInventory = true; // when removing a music box, the inventory should be open
							LocalModPlayer.MusicBoxList.RemoveAll(x => x.Type == SlotItemID);
							tMusicPlayer.SendDebugText($"[i:{SlotItemID}] [#{SlotItemID}] was removed (via selection panel)", Color.LightCoral);
							if (!LocalModPlayer.BoxResearched(SlotItemID)) {
								// only change display box if the music box is not already researched
								if (UI.DisplayBox.MusicBox == SlotItemID) {
									if (UI.FindPrev() is MusicData prev) {
										UI.DisplayBox = prev;
										UI.IsPlayingMusic = false;
									}
									else if (UI.FindNext() is MusicData next) {
										UI.DisplayBox = next;
										UI.IsPlayingMusic = false;
									}
									else {
										UI.IsPlayingMusic = false;
										UI.IsListening = true;
									}
								}
							}
						}
					}
				}
			}
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
			ClearSearchText();
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
			if (focused) {
				Vector2 MousePosition = new Vector2(Main.mouseX, Main.mouseY);
				if (JustPressed(Keys.Tab) || JustPressed(Keys.Escape) || (!ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight))) {
					Unfocus(); // Unfocus search bar when pressing Tab or Esc, or when clicking outside the input box
				}

				if (JustPressed(Keys.Enter)) {
					Main.drawingPlayerChat = false;
					Unfocus(); // Pressing enter will also unfocus the search bar, but will also close player chat
				}
			}

			base.Update(gameTime);
		}

		public void ClearSearchText() {
			MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
			SetText("");
			UI.SortedMusicData = new List<MusicData>(MusicUISystem.Instance.AllMusic);
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
							UI.SortedMusicData = searchedData;
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
