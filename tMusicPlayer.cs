using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace tMusicPlayer
{
	public class tMusicPlayer : Mod {
		internal static tMusicPlayer instance;

		internal static TMPConfig tMPConfig;

		internal static ModKeybind ListenModeHotkey;
		internal static ModKeybind PlayStopHotkey;
		internal static ModKeybind PrevSongHotkey;
		internal static ModKeybind NextSongHotkey;
		
		public override void Load() {
			instance = this;

			// Setup hotkeys and the configs instance.
			ListenModeHotkey = KeybindLoader.RegisterKeybind(this, "ToggleListen", Keys.Up.ToString());
			PlayStopHotkey = KeybindLoader.RegisterKeybind(this, "PlayOrStop", Keys.Down.ToString());
			PrevSongHotkey = KeybindLoader.RegisterKeybind(this, "PreviousSong", Keys.Left.ToString());
			NextSongHotkey = KeybindLoader.RegisterKeybind(this, "NextSong", Keys.Right.ToString());
		}

		public override void Unload() {
			instance = null;
			tMPConfig = null;

			ListenModeHotkey = null;
			PlayStopHotkey = null;
			PrevSongHotkey = null;
			NextSongHotkey = null;
		}

		public static void SendDebugText(int itemID, string status, string via, Color color) {
			string LangDebug = "Mods.tMusicPlayer.DebugMessages";
			string GetLang(string endKey) => Language.GetTextValue(LangDebug + "." + endKey);
			if (tMPConfig.EnableDebugMode)
				Main.NewText(Language.GetTextValue(LangDebug + ".EntryState", itemID, GetLang(status), GetLang(via)), color);
		}
	}

	internal class MusicPlayer_BuilderToggle : BuilderToggle {
		public static LocalizedText OnText { get; private set; }
		public static LocalizedText OffText { get; private set; }

		public override void SetStaticDefaults() {
			OnText = this.GetLocalization(nameof(OnText));
			OffText = this.GetLocalization(nameof(OffText));
		}

		public override string DisplayValue() => CurrentState == 0 ? OnText.Value : OffText.Value;
		public override Color DisplayColorTexture() => CurrentState == 0 ? Color.White : new Color(150, 150, 150);

		public override string Texture => "tMusicPlayer/UI/BuilderToggle_MusicPlayer";
		public override string HoverTexture => "tMusicPlayer/UI/BuilderToggle_MusicPlayer_Hover";

		public override bool Active() => true;

		public bool MusicPlayerVisible => Active() && CurrentState == 0;
	}
}
