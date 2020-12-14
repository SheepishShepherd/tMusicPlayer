using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace tMusicPlayer.Tiles
{
	// TODO: [1.4] Remove all tiles. No longer needed as the 1.4 update reimplements console music.
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
			disableSmartCursor = true;
			ModTranslation mapName = CreateMapEntryName();
			mapName.SetDefault("Music Box");
			AddMapEntry(new Color(200, 200, 200), mapName);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleOcean>());
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleOcean>();
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
			disableSmartCursor = true;
			ModTranslation mapName = CreateMapEntryName();
			mapName.SetDefault("Music Box");
			AddMapEntry(new Color(200, 200, 200), mapName);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleSpace>());
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleSpace>();
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
			disableSmartCursor = true;
			ModTranslation mapName = CreateMapEntryName();
			mapName.SetDefault("Music Box");
			AddMapEntry(new Color(200, 200, 200), mapName);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleTitle>());
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleTitle>();
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
			disableSmartCursor = true;
			ModTranslation mapName = CreateMapEntryName();
			mapName.SetDefault("Music Box");
			AddMapEntry(new Color(200, 200, 200), mapName);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, ModContent.ItemType<Items.MusicBoxConsoleTutorial>());
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = ModContent.ItemType<Items.MusicBoxConsoleTutorial>();
		}
	}
}
