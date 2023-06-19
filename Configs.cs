﻿using System.ComponentModel;
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

		private bool HideOnStart;
		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool StartHidden {
			get => HideOnStart;
			set {
				HideOnStart = value;
				if (!value)
					DisableStartHiddenPrompt = false;
			}
		}

		[DefaultValue(false)]
		[BackgroundColor(23, 25, 81)]
		public bool DisableStartHiddenPrompt { get; set; }

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
				MusicUISystem.Instance.MusicUI.ResetPanelPositionsToDefault();
				ResetPanels = false;
			}
		}
	}
}
