using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace tMusicPlayer.Tiles
{
	public class MusicBoxConsoleOcean : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin =  new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile((int)Type);
			base.disableSmartCursor = true;
			ModTranslation val = this.CreateMapEntryName((string)null);
			val.SetDefault("Music Box");
			this.AddMapEntry(new Color(200, 200, 200), val);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleOcean>(), 1, false, 0, false, false);
		}

		public override void MouseOver(int i, int j)
		{
			Player localPlayer = Main.LocalPlayer;
			localPlayer.noThrow = 2;
			localPlayer.showItemIcon = true;
			localPlayer.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleOcean>();
		}
	}

	public class MusicBoxConsoleSpace : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile((int)Type);
			base.disableSmartCursor = true;
			ModTranslation val = this.CreateMapEntryName((string)null);
			val.SetDefault("Music Box");
			this.AddMapEntry(new Color(200, 200, 200), val);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleSpace>(), 1, false, 0, false, false);
		}

		public override void MouseOver(int i, int j)
		{
			Player localPlayer = Main.LocalPlayer;
			localPlayer.noThrow = 2;
			localPlayer.showItemIcon = true;
			localPlayer.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleSpace>();
		}
	}

	public class MusicBoxConsoleTitle : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile((int)Type);
			base.disableSmartCursor = true;
			ModTranslation val = this.CreateMapEntryName((string)null);
			val.SetDefault("Music Box");
			this.AddMapEntry(new Color(200, 200, 200), val);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleTitle>(), 1, false, 0, false, false);
		}

		public override void MouseOver(int i, int j)
		{
			Player localPlayer = Main.LocalPlayer;
			localPlayer.noThrow = 2;
			localPlayer.showItemIcon = true;
			localPlayer.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleTitle>();
		}
	}

	public class MusicBoxConsoleTutorial : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile((int)Type);
			base.disableSmartCursor = true;
			ModTranslation val = this.CreateMapEntryName((string)null);
			val.SetDefault("Music Box");
			this.AddMapEntry(new Color(200, 200, 200), val);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleTutorial>(), 1, false, 0, false, false);
		}

		public override void MouseOver(int i, int j)
		{
			Player localPlayer = Main.LocalPlayer;
			localPlayer.noThrow = 2;
			localPlayer.showItemIcon = true;
			localPlayer.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleTutorial>();
		}
	}
}
