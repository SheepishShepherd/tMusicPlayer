using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace tMusicPlayer
{
	public class tMusicPlayer : Mod {
		internal static tMusicPlayer instance;

		public static TMPConfig tMPConfig;

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

	public class ToggleMusicPlayer : ModCommand {
		public override CommandType Type => CommandType.Chat;

		public override string Command => "musicplayer";

		public override string Description => "Toggle the visibility of the Music Player UI";

		public override void Action(CommandCaller caller, string input, string[] args) {
			MusicUISystem.Instance.MusicUI.MusicPlayerVisible = !MusicUISystem.Instance.MusicUI.MusicPlayerVisible;
		}
	}
}
