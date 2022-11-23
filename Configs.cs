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
				MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
				if (ResettingPanels) {
					UI.MusicPlayerPanel.Left.Pixels = 1115f;
					UI.MusicPlayerPanel.Top.Pixels = 16f;
					UI.selectionPanel.Left.Pixels = (Main.screenWidth / 2) - UI.selectionPanel.Width.Pixels / 2f;
					UI.selectionPanel.Top.Pixels = (Main.screenHeight / 2) - UI.selectionPanel.Height.Pixels / 2f;
					UI.musicEntryPanel.Left.Pixels = UI.selectionPanel.Left.Pixels + UI.selectionPanel.Width.Pixels - UI.musicEntryPanel.Width.Pixels - 10f;
					UI.musicEntryPanel.Top.Pixels = UI.selectionPanel.Top.Pixels - UI.musicEntryPanel.Height.Pixels;
					ResettingPanels = false;
				}
			}
		}
	}
}
