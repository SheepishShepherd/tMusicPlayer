using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace tMusicPlayer
{
	public class MusicPlayerPlayer : ModPlayer {
		public List<ItemDefinition> MusicBoxList;
		public List<ItemDefinition> MusicBoxFavs;
		public int musicBoxesStored;

		public bool BoxIsCollected(int type) => MusicBoxList.Any(item => item.Type == type);
		public bool BoxResearched(int type) => Player.difficulty == PlayerDifficultyID.Creative && Player.creativeTracker.ItemSacrifices.TryGetSacrificeNumbers(type, out int count, out int max) && count >= max;
		public bool BoxIsFavorited(int type) => MusicBoxFavs.Any(item => item.Type == type);

		public override void Initialize() {
			MusicBoxList = new List<ItemDefinition>();
			MusicBoxFavs = new List<ItemDefinition>();
			musicBoxesStored = 0;
		}

        public override void SaveData(TagCompound tag) {
			tag["Music Boxes"] = MusicBoxList;
			tag["Favorites"] = MusicBoxFavs;
			tag["Stored Boxes"] = musicBoxesStored;
		}

        public override void LoadData(TagCompound tag) {
			MusicBoxList = tag.GetList<ItemDefinition>("Music Boxes").ToList();
			MusicBoxFavs = tag.GetList<ItemDefinition>("Favorites").ToList();
			musicBoxesStored = tag.Get<int>("Stored Boxes");
		}

		public override void OnEnterWorld() {
			if (!tMusicPlayer.tMPConfig.DisableStartHiddenPrompt)
				Main.NewText("Music player is hidden. To unhide it, use mod command: /musicplayer (This message can be disabled in the tMusicPlayer configs)", Color.Khaki);

			if (MusicUISystem.Instance.MusicUI is MusicPlayerUI UI && tMusicPlayer.tMPConfig.StartWithSmall != UI.MiniModePlayer)
				UI.MiniModePlayer = !UI.MiniModePlayer; // Determine if the player wants to start with the small panel or large panel
		}

		public override void PostUpdateEquips() {
			// Currently, the best way to override music is to equip a music box in one of your accessory slots.
			// Terraria source code uses UpdateEquips and sets Main.musicBox2 to determine music.
			// By updating Main.musicBox2 again in PostUpdateEquips, the music player effectively becomes top priority.
			// MusicBox2 has its own special numbers, automatically detected within our MusicData entries
			if (MusicUISystem.Instance.MusicUI is not MusicPlayerUI UI)
				return;

			if (!Main.gameMenu && !Main.dedServ && UI.IsPlayingMusic) {
				int index = MusicUISystem.Instance.AllMusic.FindIndex(x => x.MusicID == UI.CurrentlyPlaying);
				Main.musicBox2 = MusicUISystem.Instance.AllMusic[index].OutputValue;
			}
		}

		public override bool ShiftClickSlot(Item[] inventory, int context, int slot) {
			// This contains the logic for shift-clicking music boxes into the UI
			// The Selection panel must be open and no chests can be open for the shift click to work
			if (MusicUISystem.Instance.MusicUI.SelectionPanelVisible && Player.chest == -1) {
				int type = inventory[slot].type;
				if (type == ItemID.MusicBox && musicBoxesStored < 20) {
					musicBoxesStored++;
					inventory[slot].TurnToAir();
					SoundEngine.PlaySound(SoundID.Grab);
					return true;
				}
				else if (MusicUISystem.Instance.AllMusic.Find(x => x.MusicBox == type) is MusicData data && !BoxResearched(type) && !BoxIsCollected(type)) {
					MusicBoxList.Add(new ItemDefinition(type));
					inventory[slot].TurnToAir();
					SoundEngine.PlaySound(SoundID.Grab);
					tMusicPlayer.SendDebugText($"[i:{type}] [#{type}] was added (via shift-click)", Colors.RarityGreen);
					return true;
				}
			}
			return false;
		}

		public override bool HoverSlot(Item[] inventory, int context, int slot) {
			if (MusicUISystem.Instance.MusicUI.SelectionPanelVisible && Player.chest == -1 && ItemSlot.ShiftInUse) {
				int type = inventory[slot].type;
				if (type == ItemID.MusicBox && musicBoxesStored < 20) {
					Main.cursorOverride = 9;
					return true;
				}
				else if (MusicUISystem.Instance.AllMusic.Any(x => x.MusicBox == type) && !BoxResearched(type) && !BoxIsCollected(type)) {
					Main.cursorOverride = 9;
					return true;
				}
			}
			return false;
		}
	}
}
