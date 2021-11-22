using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace tMusicPlayer
{
	internal class MusicUISystem : ModSystem
	{
		public static MusicUISystem Instance { get; private set; }

		internal static UserInterface MP_UserInterface;
		internal static MusicPlayerUI MusicUI;

		public override void Load()
		{
			// Setup the Music Player UI.
			if (!Main.dedServ) {
				MusicUI = new MusicPlayerUI();
				MusicUI.Activate();
				MP_UserInterface = new UserInterface();
				MP_UserInterface.SetState(MusicUI);
			}
		}

		public override void Unload()
		{
			MP_UserInterface = null;
			MusicUI = null;
		}

		public override void UpdateUI(GameTime gameTime)
		{
			// Update Music Player UI as long as it exists.
			UserInterface mP_UserInterface = MP_UserInterface;
			if (mP_UserInterface != null) {
				mP_UserInterface.Update(gameTime);
			}
		}

		//int lastSeenScreenWidth;
		//int lastSeenScreenHeight;
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			// Draws the Music Player UI.
			int index = layers.FindIndex((GameInterfaceLayer layer) => layer.Name.Equals("Vanilla: Inventory"));
			if (index != -1) {
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Music Player",
					delegate {
						if (MusicUI.mpToggleVisibility) {
							MP_UserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
				/*
				// TODO: reimplement right click to play song text in selection menu
				layers.Insert(index + 1, new LegacyGameInterfaceLayer(
					"tMusicPlayer: Right-click text",
					delegate {
						//if (ExampleUI.Visible) {
						//	  _exampleUserInterface.Draw(Main.spriteBatch, new GameTime());
						//}
						return true;
					},
					InterfaceScaleType.UI)
				);
				*/
			}
		}
	}
}
