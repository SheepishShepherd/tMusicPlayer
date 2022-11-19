using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace tMusicPlayer
{
	public class MusicPlayerPlayer : ModPlayer
	{
		public List<ItemDefinition> MusicBoxList;
		public List<ItemDefinition> MusicBoxFavs;
		public int musicBoxesStored;

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
			MusicBoxList = tag.Get<List<ItemDefinition>>("Music Boxes");
			MusicBoxFavs = tag.Get<List<ItemDefinition>>("Favorites");
			musicBoxesStored = tag.Get<int>("Stored Boxes");
		}

		public override void OnEnterWorld(Player player) {
			// Determine if the player wants to start with the small panel or large panel
			if (tMusicPlayer.tMPConfig.StartWithSmall != MusicUISystem.MusicUI.smallPanel) {
				MusicUISystem.MusicUI.SwapPanelSize();
			}

			// Add all the player's obtained musicboxes to the canPlay array for the UI
			if (MusicUISystem.MusicUI != null) {
				for (int i = 0; i < tMusicPlayer.AllMusic.Count; i++) {
					if (MusicBoxList.Any(x => x.Type == tMusicPlayer.AllMusic[i].musicbox)) {
						MusicUISystem.MusicUI.canPlay.Add(tMusicPlayer.AllMusic[i].music);
					}
				}
			}
		}

		public override void PostUpdateEquips() {
			// Currently, the best way to override music is to equip a music box in one of your accessory slots.
			// Terraria source code uses UpdateEquips and sets Main.musicBox2 to determine music.
			// By updating Main.musicBox2 again in PostUpdateEquips, the music player effectively becomes top priority.
			// MusicBox2 has its own special numbers, automatically detected within our MusicData entries
			if (!Main.gameMenu && !Main.dedServ && MusicUISystem.MusicUI != null && MusicUISystem.MusicUI.playingMusic > -1) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.music == MusicUISystem.MusicUI.playingMusic);
				Main.musicBox2 = tMusicPlayer.AllMusic[index].mainMusicBox2;
			}
		}
	}
}
