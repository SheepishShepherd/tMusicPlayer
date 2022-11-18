using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace tMusicPlayer
{
	[BackgroundColor(55, 59, 80, 255)]
	[Label("Personal Configs")]
	public class TMPConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Header("[i:576] [c/ffeb6e:Configs]")]
		[DefaultValue(true)]
		[Label("Smart Cursor: Enable extra tooltips")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("While smart cursor is toggled, most buttons show extra information about them")]
		public bool EnableMoreTooltips { get; set; }

		/*
		[DefaultValue(false)]
		[Label("Unlock all music in the selection list")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("All music can be played from the MusicPlayer, regaurdless of whether it was obtained or not.")]
		public bool EnableAllMusicBoxes { get; set; }
		*/

		[DefaultValue(false)]
		[Label("Hide MusicPlayer until hotkey pressed")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("The MusicPlayer will not show or hide unless the hotkey is pressed to toggle between the states.")]
		public bool ForceUseHotkey { get; set; }

		[DefaultValue(false)]
		[Label("Start with the small panel")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("")]
		public bool StartWithSmall { get; set; }

		[Header("[i:3625] [c/ffeb6e:Debugging]")]
		[DefaultValue(false)]
		[Label("Enable Debug Messages")]
		[BackgroundColor(189, 183, 107, 255)]
		public bool EnableDebugMode { get; set; }

		[DefaultValue(false)]
		[Label("Reset Music Player panel positions")]
		[BackgroundColor(189, 183, 107, 255)]
		public bool ResettingPanels { get; set; }

		public override void OnChanged() {
			if (!Main.gameMenu && !Main.dedServ) {
				MusicPlayerUI UI = MusicUISystem.MusicUI;
				if (ResettingPanels) {
					UI.MusicPlayerPanel.Left.Pixels = 1115f;
					UI.MusicPlayerPanel.Top.Pixels = 16f;
					UI.selectionPanel.Left.Pixels = (Main.screenWidth / 2) - UI.selectionPanel.Width.Pixels / 2f;
					UI.selectionPanel.Top.Pixels = (Main.screenHeight / 2) - UI.selectionPanel.Height.Pixels / 2f;
					UI.musicEntryPanel.Left.Pixels = UI.selectionPanel.Left.Pixels + UI.selectionPanel.Width.Pixels - UI.musicEntryPanel.Width.Pixels - 10f;
					UI.musicEntryPanel.Top.Pixels = UI.selectionPanel.Top.Pixels - UI.musicEntryPanel.Height.Pixels;
					ResettingPanels = false;
				}
				/*
				if (EnableAllMusicBoxes) {
					for (int j = 0; j < musicPlayerUI.canPlay.Count; j++) {
						musicPlayerUI.canPlay[j] = true;
					}
					tMusicPlayer.SendDebugText("EnableAllMusicBoxes enabled. All music in canPlay set to true.");
				}
				else {
					MusicPlayerPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MusicPlayerPlayer>();
					for (int i = 0; i < musicPlayerUI.canPlay.Count; i++) {
						musicPlayerUI.canPlay[i] = (modPlayer.MusicBoxList.FindIndex((ItemDefinition x) => x.Type == tMusicPlayer.AllMusic[i].musicbox) != -1);
					}
					Main.NewText("EnableAllMusicBoxes disabled. Music in canPlay restored appropriately", Color.White);
					int next = tMusicPlayer.MusicPlayerUI.FindNextIndex();
					int prev = tMusicPlayer.MusicPlayerUI.FindPrevIndex();
					if (modPlayer.MusicBoxList.FindIndex((ItemDefinition x) => x.Type == tMusicPlayer.MusicPlayerUI.DisplayBox) == -1) {
						if (next != -1) {
							tMusicPlayer.MusicPlayerUI.DisplayBox = next;
						}
						else if (prev != -1) {
							tMusicPlayer.MusicPlayerUI.DisplayBox = prev;
						}
						else {
							musicPlayerUI.listening = true;
						}
					}
				}
				*/
			}
		}
	}

	[BackgroundColor(55, 59, 80, 255)]
	[Label("Global Configs (Reloading Required)")]
	public class TMPServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[ReloadRequired]
		[Range(3, 20)]
		[DefaultValue(5)]
		[Label("MusicPlayer maximum music boxes for recording")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("Set the maximum amount of music boxes able to be stored")]
		public int MaxStorage { get; set; }

		[DefaultValue(false)]
		[Label("MusicPlayer includes researched music boxes")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("If you have researched a music box, it will automatically fill in!")]
		public bool IncludeResearched { get; set; }

		public override void OnChanged() {

		}
	}
}
