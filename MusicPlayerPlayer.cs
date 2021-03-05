using Microsoft.Xna.Framework;
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

		public override void Initialize()
		{
			MusicBoxList = new List<ItemDefinition>();
			musicBoxesStored = 0;
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound {
				{ "Music Boxes", MusicBoxList },
				{ "Stored Boxes", musicBoxesStored }
			};
			return tag;
		}

		public override void Load(TagCompound tag)
		{
			MusicBoxList = tag.Get<List<ItemDefinition>>("Music Boxes");
			musicBoxesStored = tag.Get<int>("Stored Boxes");
		}

		public override void OnEnterWorld(Player player)
		{
			// When entering a world, we must setup the players music boxes and determine whether they can be played or not.
			// This is important wince we can change "Unlock all music boxes" in the configs while outside of a world.
			MusicPlayerUI musicPlayerUI = tMusicPlayer.MusicPlayerUI;
			musicPlayerUI.smallPanel = tMusicPlayer.tMPConfig.StartWithSmall;
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

		public override void PreUpdate()
		{
			// This code mimics the "Music Box Recording" process.
			// Check if we have music boxes at the ready, if the player is in record mode and music is currently playing.
			// If all of those apply, we also go a rand check which will trigger the "recording" code.
			if (musicBoxesStored > 0 && tMusicPlayer.MusicPlayerUI.recording && Main.curMusic > 0 && Main.rand.Next(2700) == 0) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.music == Main.curMusic); // Make sure curMusic is a music box.
				if (index != -1) {
					int musicBoxType = tMusicPlayer.AllMusic[index].musicbox;
					Main.PlaySound(mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/recorded")); // TODO: [1.4] Proper PlaySound
					if (MusicBoxList.All(x => x.Type != musicBoxType)) {
						// If we don't have it in our music player, automatically add it in.
						MusicBoxList.Add(new ItemDefinition(musicBoxType));
					}
					else {
						// If we do have it already, spawn the item.
						player.QuickSpawnItem(musicBoxType);
					}
					tMusicPlayer.SendDebugText($"Music Box ({tMusicPlayer.AllMusic[index].name}) obtained!", Color.BlanchedAlmond);

					// Automatically turn recording off and reduce the amount of stored music boxes by 1.
					tMusicPlayer.MusicPlayerUI.recording = false;
					musicBoxesStored--;
				}
			}
		}

		public override void PostUpdateEquips()
		{
			// Currently, the best way to override music is to equip a music box in one of your accessory slots.
			// Terraria source code uses UpdateEquips and sets Main.musicBox2 to determine music.
			// By updating Main.musicBox2 again in PostUpdateEquips, the music player effectively becomes top priority.
			// MusicBox2 has its own special numbers, automatically detected within our MusicData entries
			if (!Main.gameMenu && tMusicPlayer.MusicPlayerUI != null && tMusicPlayer.MusicPlayerUI.playingMusic > -1) {
				int index = tMusicPlayer.AllMusic.FindIndex(x => x.music == tMusicPlayer.MusicPlayerUI.playingMusic);
				Main.musicBox2 = tMusicPlayer.AllMusic[index].mainMusicBox2;
			}
		}
	}
}
