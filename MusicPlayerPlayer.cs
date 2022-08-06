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
		public int musicBoxesStored;

		public override void Initialize() {
			MusicBoxList = new List<ItemDefinition>();
			musicBoxesStored = 0;
		}

        public override void SaveData(TagCompound tag) {
			tag["Music Boxes"] = MusicBoxList;
			tag["Stored Boxes"] = musicBoxesStored;
		}

        public override void LoadData(TagCompound tag) {
			MusicBoxList = tag.Get<List<ItemDefinition>>("Music Boxes");
			musicBoxesStored = tag.Get<int>("Stored Boxes");
		}

		public override void OnEnterWorld(Player player) {
			// When entering a world, we must setup the players music boxes and determine whether they can be played or not.
			// This is important wince we can change "Unlock all music boxes" in the configs while outside of a world.
			MusicPlayerUI musicPlayerUI = MusicUISystem.MusicUI;
			if (tMusicPlayer.tMPConfig.StartWithSmall != MusicUISystem.MusicUI.smallPanel) {
				MusicUISystem.MusicUI.SwapPanelSize();
			}
			if (musicPlayerUI != null) {
				for (int i = 0; i < tMusicPlayer.AllMusic.Count; i++) {
					if (MusicBoxList.Any(x => x.Type == tMusicPlayer.AllMusic[i].musicbox)) {
						musicPlayerUI.canPlay.Add(tMusicPlayer.AllMusic[i].music);
					}
				}
				/*
				if (tMusicPlayer.tMPConfig.EnableAllMusicBoxes) musicPlayerUI.canPlay[i] = true;
				else musicPlayerUI.canPlay[i] = MusicBoxList.Any(x => x.Type == tMusicPlayer.AllMusic[i].musicbox);
				*/
				// Remove above line if bringing back EnableAllMusicBoxes
			}
		}

		public override void PostUpdateEquips() {
			// Currently, the best way to override music is to equip a music box in one of your accessory slots.
			// Terraria source code uses UpdateEquips and sets Main.musicBox2 to determine music.
			// By updating Main.musicBox2 again in PostUpdateEquips, the music player effectively becomes top priority.
			// MusicBox2 has its own special numbers, automatically detected within our MusicData entries
			if (!Main.gameMenu && MusicUISystem.MusicUI != null && MusicUISystem.MusicUI.playingMusic > -1) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.music == MusicUISystem.MusicUI.playingMusic);
				Main.musicBox2 = tMusicPlayer.AllMusic[index].mainMusicBox2;
			}
		}
	}
}
