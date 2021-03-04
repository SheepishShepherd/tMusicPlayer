using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace tMusicPlayer
{
	[BackgroundColor(55, 59, 80, 255)]
	[Label("Configure your Music!")]
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
		
		[DefaultValue(5)]
		[Label("MusicPlayer maximum music boxes for recording")]
		[BackgroundColor(76, 168, 84, 255)]
		[Tooltip("Set the maximum amount of music boxes able to be stored")]
		public int MaxStorage { get; set; }

		[Header("[i:3625] [c/ffeb6e:Debugging]")]
		[DefaultValue(false)]
		[Label("Enable Debug Messages")]
		[BackgroundColor(189, 183, 107, 255)]
		public bool EnableDebugMode { get; set; }

		[DefaultValue(false)]
		[Label("Reset Music Player panel positions")]
		[BackgroundColor(189, 183, 107, 255)]
		public bool ResettingPanels { get; set; }

		public override void OnChanged()
		{
			if (!Main.gameMenu) {
				MusicPlayerUI musicPlayerUI = tMusicPlayer.MusicPlayerUI;
				if (ResettingPanels) {
					musicPlayerUI.MusicPlayerPanel.Left.Pixels = 500f;
					musicPlayerUI.MusicPlayerPanel.Top.Pixels = 6f;
					musicPlayerUI.selectionPanel.Left.Pixels = (float)(Main.screenWidth / 2) - musicPlayerUI.selectionPanel.Width.Pixels / 2f;
					musicPlayerUI.selectionPanel.Top.Pixels = (float)(Main.screenHeight / 2) - musicPlayerUI.selectionPanel.Height.Pixels / 2f;
					musicPlayerUI.musicEntryPanel.Left.Pixels = musicPlayerUI.selectionPanel.Left.Pixels + musicPlayerUI.selectionPanel.Width.Pixels - musicPlayerUI.musicEntryPanel.Width.Pixels - 10f;
					musicPlayerUI.musicEntryPanel.Top.Pixels = musicPlayerUI.selectionPanel.Top.Pixels - musicPlayerUI.musicEntryPanel.Height.Pixels;
					ResettingPanels = false;
				}
				/*
				if (EnableAllMusicBoxes) {
					for (int j = 0; j < musicPlayerUI.canPlay.Count; j++) {
						musicPlayerUI.canPlay[j] = true;
					}
					tMusicPlayer.SendDebugText("EnableAllMusicBoxes enabled. All music in canPlay set to true.");
				}
				else { // Do the code below 
				*/
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
				// --end-- above code in else statement if bringing back EnableAllMusicBoxes
			}
		}
	}
}
