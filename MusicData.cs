using Terraria;
using Terraria.ID;
using Terraria.Localization;
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
		internal LocalizedText name;

		internal string Name => name.Value.Contains('(') ? name.Value.Substring(name.Value.IndexOf("(") + 1).Replace(")", "") : name.Value;

		/// <summary> If applicable, the name of the composer that made this music. </summary>
		internal string composer;

		/// <summary> Determines if the Music Player is able to play this music. </summary>
		internal bool CanPlay(MusicPlayerPlayer modplayer) => modplayer.BoxIsCollected(MusicBox) || modplayer.BoxResearched(MusicBox);

		/// <summary> Gets the index of this MusicData within <see cref="MusicUISystem.AllMusic"/></summary>
		internal int GetIndex => MusicUISystem.Instance.AllMusic.IndexOf(this);

		/// <summary> The display name of the mod. </summary>
		internal string Mod_DisplayName => ModLoader.TryGetMod(Mod, out Mod mod) ? mod.DisplayName : Mod;

		internal string Mod_DisplayName_NoChatTags() {
			string editedName = "";

			for (int c = 0; c < Mod_DisplayName.Length; c++) {
				// Add each character one by one to find chattags in order
				// Chat tags cannot be contained inside other chat tags so no need to worry about overlap
				editedName += Mod_DisplayName[c];
				if (editedName.Contains("[i:") && editedName.EndsWith("]")) {
					// Update return name if a complete item chat tag is found
					editedName = editedName.Substring(0, editedName.IndexOf("[i:"));
					continue;
				}
				if (editedName.Contains("[i/") && editedName.EndsWith("]")) {
					// Update return name if a complete item chat tag is found
					editedName = editedName.Substring(0, editedName.IndexOf("[i/"));
					continue;
				}
				if (editedName.Contains("[c/") && editedName.Contains(":") && editedName.EndsWith("]")) {
					// Color chat tags are edited differently as we want to keep the text that's nested inside them
					string part1 = editedName.Substring(0, editedName.IndexOf("[c/"));
					string part2 = editedName.Substring(editedName.IndexOf(":") + 1);
					part2 = part2.Substring(0, part2.Length - 1);
					editedName = part1 + part2;
					continue;
				}
			}
			return editedName;
		}

		public override string ToString() => $"[i:{MusicBox}] [{Mod}] {Name}{(string.IsNullOrEmpty(composer) ? " " : $" by {composer} ")}(MusicID: #{MusicID})";

		// Vanilla method
		internal MusicData(int music, int musicbox, int mainMusicBox2) {
			this.MusicID = music;
			this.OutputValue = mainMusicBox2;
			this.MusicBox = musicbox;

			this.Mod = music >= 58 && music <= 84 ? "Terraria Otherworld" : "Terraria";
			this.name = Lang.GetItemName(musicbox);
		}

		// Mod method
		public MusicData(int music, int musicbox, string mod, LocalizedText name) {
			this.MusicID = music;
			this.OutputValue = music;
			this.MusicBox = musicbox;

			this.Mod = mod;
			this.name = name;
		}
	}
}
