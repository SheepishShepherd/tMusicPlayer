using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace tMusicPlayer
{
	[BackgroundColor(55, 59, 80)]
	public class TMPConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ClientSide;

		//[Header("[i:576] [c/ffeb6e:Configs]")]
		[DefaultValue(true)]
		[BackgroundColor(23, 25, 81)]
		public bool EnableMoreTooltips { get; set; }

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool ForceUseHotkey { get; set; }

		[Header("Defaults")]
		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool StartWithSmall { get; set; }

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool StartWithListView { get; set; }

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool ResetPanels { get; set; }

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool EnableDebugMode { get; set; }

		public override void OnChanged() {
			if (!Main.gameMenu && !Main.dedServ && ResetPanels) {
				MusicPlayerUI UI = MusicUISystem.Instance.MusicUI;
				UI.MusicPlayerPanel.Left.Pixels = 1115f;
				UI.MusicPlayerPanel.Top.Pixels = 16f;
				UI.SelectionPanel.Left.Pixels = (Main.screenWidth / 2) - UI.SelectionPanel.Width.Pixels / 2f;
				UI.SelectionPanel.Top.Pixels = (Main.screenHeight / 2) - UI.SelectionPanel.Height.Pixels / 2f;
				ResetPanels = false;
			}
		}
	}
}
