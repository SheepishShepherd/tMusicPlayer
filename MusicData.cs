using Terraria;
using Terraria.ID;

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
			return $"{name} from {mod} [Item#{musicbox}] -- [MusicID#{music}]";
		}

		// Vanilla method
		internal MusicData(int music, int musicbox, int mainMusicBox2)
        {
			this.music = music;
			this.musicbox = musicbox;
			this.mainMusicBox2 = mainMusicBox2;

			string itemNameValue = Lang.GetItemNameValue(musicbox);
			if (itemNameValue.Contains("Otherworldly")) {
				this.mod = "Terraria Otherworld";
			}
			else {
				this.mod = "Terraria";
			}
			this.name = itemNameValue.Substring(itemNameValue.IndexOf("(") + 1).Replace(")", "");
		}

		public MusicData(int music, int musicbox, string mod, string name)
		{
			this.music = music;
			this.musicbox = musicbox;
			this.mod = mod;
			this.name = name;

			mainMusicBox2 = music;
		}
	}
}
