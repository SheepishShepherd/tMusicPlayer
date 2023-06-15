using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace tMusicPlayer
{
	public class tMusicPlayer : Mod {
		internal static tMusicPlayer instance;

		public static TMPConfig tMPConfig;

		internal static ModKeybind HidePlayerHotkey;
		internal static ModKeybind PlayStopHotkey;
		internal static ModKeybind PrevSongHotkey;
		internal static ModKeybind NextSongHotkey;
		
		public override void Load() {
			instance = this;

			// Setup hotkeys and the configs instance.
			tMPConfig = ModContent.GetInstance<TMPConfig>();
			HidePlayerHotkey = KeybindLoader.RegisterKeybind(this, "HidePlayer", Keys.Up.ToString());
			PlayStopHotkey = KeybindLoader.RegisterKeybind(this, "PlayOrStop", Keys.Down.ToString());
			PrevSongHotkey = KeybindLoader.RegisterKeybind(this, "PreviousSong", Keys.Left.ToString());
			NextSongHotkey = KeybindLoader.RegisterKeybind(this, "NextSong", Keys.Right.ToString());
		}

		public override void Unload() {
			instance = null;
			tMPConfig = null;

			HidePlayerHotkey = null;
			PlayStopHotkey = null;
			PrevSongHotkey = null;
			NextSongHotkey = null;
		}

		public static void SendDebugText(string message, Color color = default) {
			if (tMPConfig.EnableDebugMode) {
				Main.NewText(message, color);
			}
		}
	}
}
