using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
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
		internal string Name { get; init; }

		/// <summary> The display name provided by the item's localization. </summary>
		internal string DisplayName => Name.Contains('(') ? Name.Substring(Name.IndexOf("(") + 1).Replace(")", "") : Name;

		/// <summary> Determines if the Music Player is able to play this music. </summary>
		internal bool CanPlay(MusicPlayerPlayer modplayer) => modplayer.BoxIsCollected(MusicBox) || modplayer.BoxResearched(MusicBox);

		/// <summary> Gets the index of this MusicData within <see cref="MusicUISystem.AllMusic"/></summary>
		internal int GetIndex => MusicUISystem.Instance.AllMusic.IndexOf(this);

		/// <summary> The display name of the mod. </summary>
		internal string Mod_DisplayName => ModLoader.TryGetMod(Mod, out Mod mod) ? mod.DisplayName : Mod;

		internal string Mod_DisplayName_NoChatTags => MusicUISystem.RemoveChatTags(Mod_DisplayName);

		internal Color MusicBox_Rarity => ItemRarity.GetColor(ContentSamples.ItemsByType[MusicBox].rare);

		public override string ToString() => $"[#{MusicID}] [i:{MusicBox}] [{Mod}] {DisplayName}";

		// Unknown method; used as a substitute for music without assigned music boxes
		internal MusicData() {
			this.MusicID = -1;
			this.OutputValue = -1;
			this.MusicBox = ItemID.MusicBox;

			this.Mod = "???";
			this.Name = Language.GetText("Mods.tMusicPlayer.MusicData.UnknownBox").Value;
		}

		// Vanilla method
		internal MusicData(int music, int musicbox, int mainMusicBox2) {
			this.MusicID = music;
			this.OutputValue = mainMusicBox2;
			this.MusicBox = musicbox;

			this.Mod = music >= 58 && music <= 84 ? "Terraria Otherworld" : "Terraria";
			this.Name = Lang.GetItemName(musicbox).Value;
		}

		// Mod method
		public MusicData(int music, int musicbox, string mod, string name) {
			this.MusicID = music;
			this.OutputValue = music;
			this.MusicBox = musicbox;

			this.Mod = mod;
			this.Name = ItemLoader.GetItem(musicbox).DisplayName.GetTranslation(Language.ActiveCulture);
		}
	}
}
