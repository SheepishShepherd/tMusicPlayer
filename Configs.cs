using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace tMusicPlayer
{
	[BackgroundColor(55, 59, 80)]
	public class TMPConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public override void OnLoaded() => tMusicPlayer.tMPConfig = this;

		private bool ResetPanels_value;

		[Header("Defaults")]

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool ResetPanels {
			get => ResetPanels_value;
			set => ResetPanels_value = !Main.gameMenu && value; // do not allow change if not in a world
		}

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool StartWithSmall { get; set; }

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool StartWithListView { get; set; }

		[Header("Accessibility")]

		[DefaultValue(true)]
		[BackgroundColor(23, 25, 81)]
		public bool EnableMoreTooltips { get; set; }

		[DefaultValue(true)]
		[BackgroundColor(23, 25, 81)]
		public bool HoverTextPopOut { get; set; }

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool EnableDebugMode { get; set; }

		public override void OnChanged() {
			if (!Main.gameMenu && !Main.dedServ && ResetPanels) {
				MusicUISystem.Instance.MusicUI.ResetPanelPositionsToDefault();
				ResetPanels = false;
			}
		}
	}
}
