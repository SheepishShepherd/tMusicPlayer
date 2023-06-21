using Microsoft.CodeAnalysis;
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
		private bool StartHidden_value;
		private bool DisableStartHiddenPrompt_value;

		[Header("Defaults")]

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool ResetPanels {
			get => ResetPanels_value;
			set => ResetPanels_value = !Main.gameMenu && value; // do not allow change if not in a world
		}

		[DefaultValue(true)]
		[BackgroundColor(23, 25, 81)]
		public bool StartHidden {
			get => StartHidden_value;
			set {
				StartHidden_value = value;
				if (!value)
					DisableStartHiddenPrompt = false; // if false, disable the prompt as well
			}
		}

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool DisableStartHiddenPrompt { 
			get => DisableStartHiddenPrompt_value;
			set {
				if (StartHidden || !value)
					DisableStartHiddenPrompt_value = value; // only allow this to be true if StartHidden is also true
			}
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
