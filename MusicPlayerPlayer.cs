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
			MusicPlayerUI musicPlayerUI = tMusicPlayer.MusicPlayerUI;
			if (musicPlayerUI != null) {
				for (int i = 0; i < musicPlayerUI.canPlay.Count; i++) {
					if (tMusicPlayer.tMPConfig.EnableAllMusicBoxes) {
						musicPlayerUI.canPlay[i] = true;
					}
					else {
						musicPlayerUI.canPlay[i] = MusicBoxList.Any(x => x.Type == tMusicPlayer.AllMusic[i].musicbox);
					}
				}
			}
		}

		public override void PreUpdate()
		{
			if (musicBoxesStored > 0 && tMusicPlayer.MusicPlayerUI.recording && Main.curMusic > 0 && Main.rand.Next(2700) == 0) {
				int index = tMusicPlayer.AllMusic.FindIndex((MusicData x) => x.music == Main.curMusic);
				if (index != -1) {
					int musicBoxType = tMusicPlayer.AllMusic[index].musicbox;
					string name = tMusicPlayer.AllMusic[index].name;
					Main.PlaySound(mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/recorded"));
					if (MusicBoxList.All(x => x.Type != musicBoxType)) {
						MusicBoxList.Add(new ItemDefinition(musicBoxType));
					}
					else {
						player.QuickSpawnItem(musicBoxType);
					}
					tMusicPlayer.SendDebugMessage($"Music Box ({name}) obtained!", Color.BlanchedAlmond);
					tMusicPlayer.MusicPlayerUI.recording = false;
					musicBoxesStored--;
				}
			}
		}

		public override void PostUpdateEquips()
		{
			if (!Main.gameMenu && tMusicPlayer.MusicPlayerUI != null && tMusicPlayer.MusicPlayerUI.playingMusic > -1) {
				Main.musicBox2 = tMusicPlayer.AllMusic[tMusicPlayer.MusicPlayerUI.DisplayBox].music;
			}
		}
	}
}
