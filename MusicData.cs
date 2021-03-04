using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tMusicPlayer
{
	internal class MusicData
	{
		internal int music;
		internal int musicbox;
		internal string mod;
		internal string name;
		internal int mainMusicBox2;

		public override string ToString()
		{
			return $"{name} from {mod} -- [Item#{musicbox}] MusicID: {music}({mainMusicBox2})";
		}

		public MusicData(int music, int musicbox, string mod = "Terraria", string name = "Unknown")
		{
			this.music = music;
			this.musicbox = musicbox;
			this.mod = mod;
			if (mod == "Terraria") {
				string itemNameValue = Lang.GetItemNameValue(musicbox);
				this.name = itemNameValue.Substring(itemNameValue.IndexOf("(") + 1).Replace(")", "");
				// TODO: [1.4] Add check for Otherworldly music boxes
			}
			else {
				this.name = name;
			}

			if (musicbox >= 562 && musicbox <= 574) {
				mainMusicBox2 = musicbox - 562;
			}
			else if(musicbox >= 1596 && musicbox <= 1609) {
				mainMusicBox2 = musicbox - 1586;
			}
			else if(musicbox == ItemID.MusicBoxMushrooms) {
				mainMusicBox2 = 27;
			}
			else if(musicbox == ItemID.MusicBoxPumpkinMoon) {
				mainMusicBox2 = 28;
			}
			else if(musicbox == ItemID.MusicBoxAltUnderground) {
				mainMusicBox2 = 29;
			}
			else if(musicbox == ItemID.MusicBoxFrostMoon) {
				mainMusicBox2 = 30;
			}
			else if(musicbox == ItemID.MusicBoxUndergroundCrimson) {
				mainMusicBox2 = 31;
			}
			else if(musicbox == ItemID.MusicBoxLunarBoss) {
				mainMusicBox2 = 32;
			}
			else if(musicbox == ItemID.MusicBoxMartians) {
				mainMusicBox2 = 33;
			}
			else if(musicbox == ItemID.MusicBoxPirates) {
				mainMusicBox2 = 34;
			}
			else if(musicbox == ItemID.MusicBoxHell) {
				mainMusicBox2 = 35;
			}
			else if(musicbox == ItemID.MusicBoxTowers) {
				mainMusicBox2 = 36;
			}
			else if(musicbox == ItemID.MusicBoxGoblins) {
				mainMusicBox2 = 37;
			}
			else if(musicbox == ItemID.MusicBoxSandstorm) {
				mainMusicBox2 = 38;
			}
			else if(musicbox == ItemID.MusicBoxDD2) {
				mainMusicBox2 = 39;
			}
			else {
				mainMusicBox2 = music;
			}
		}
	}
}
