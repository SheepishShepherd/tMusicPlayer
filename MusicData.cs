using Terraria;
using Terraria.ModLoader;

namespace tMusicPlayer
{
	internal class MusicData {
		/// <summary> The music ID assigned. This is used as a unique identifier. </summary>
		internal int MusicID { get; init; }

		/// <summary> The numerical value used for 'Main.musicBox2', the value used for equipping music boxes as an accessory. </summary>
		internal int OutputValue { get; init; }

		/// <summary> The item ID of the music box that plays this music. </summary>
		internal int MusicBox { get; init; }

		/// <summary> The internal name of the mod that the music originates from. </summary>
		internal string Mod { get; init; }

		/// <summary> The name provided to the music box. </summary>
		internal string name;

		/// <summary> If applicable, the name of the composer that made this music. </summary>
		internal string composer;

		/// <summary> Determines if the Music Player is able to play this music. </summary>
		internal bool canPlay = false;

		internal int GetIndex => MusicUISystem.Instance.AllMusic.IndexOf(this);

		internal Mod GetMod => ModLoader.TryGetMod(Mod, out Mod mod) ? mod : null;

		public override string ToString() => $"[i:{MusicBox}] [{Mod}] {name}{(string.IsNullOrEmpty(composer) ? " " : $" by {composer} ")}(MusicID: #{MusicID})";

		// Vanilla method
		internal MusicData(int music, int musicbox, int mainMusicBox2) {
			this.MusicID = music;
			this.OutputValue = mainMusicBox2;
			this.MusicBox = musicbox;

			string itemNameValue = Lang.GetItemNameValue(musicbox);
			if (itemNameValue.Contains("Otherworldly")) {
				this.Mod = "Terraria Otherworld";
			}
			else {
				this.Mod = "Terraria";
			}
			this.name = itemNameValue.Substring(itemNameValue.IndexOf("(") + 1).Replace(")", "");
		}

		// Mod method
		public MusicData(int music, int musicbox, string mod, string name) {
			this.MusicID = music;
			this.OutputValue = music;
			this.MusicBox = musicbox;

			this.Mod = mod;
			this.name = name;
		}
	}
}
