using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace tMusicPlayer.Items
{
	public class AncientWizardTech : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Able to bring magic of the past");
		}

		public override void SetDefaults()
		{
			item.width = 40;
			item.height = 40;
			item.value = Item.buyPrice(0, 1, 0, 0);
			item.rare = ItemRarityID.Orange;
		}
	}

	public class MusicBoxConsoleOcean : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Music Box (Ocean Console)");
		}

		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = ModContent.TileType<Tiles.MusicBoxConsoleOcean>();
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.LightRed;
			item.value = Item.buyPrice(0, 10, 0, 0);
			item.accessory = true;
		}

		public override void AddRecipes()
		{
			ModRecipe val = new ModRecipe(mod);
			val.AddIngredient(ItemID.MusicBoxOcean, 1);
			val.AddIngredient(ModContent.ItemType<AncientWizardTech>(), 1);
			val.AddTile(TileID.Tables);
			val.AddTile(TileID.Chairs);
			val.SetResult(this, 1);
			val.AddRecipe();
		}
	}

	public class MusicBoxConsoleSpace : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Music Box (Space Console)");
		}

		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = ModContent.TileType<Tiles.MusicBoxConsoleSpace>();
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.LightRed;
			item.value = Item.buyPrice(0, 10, 0, 0);
			item.accessory = true;
		}

		public override void AddRecipes()
		{
			ModRecipe val = new ModRecipe(mod);
			val.AddIngredient(ItemID.MusicBoxSpace, 1);
			val.AddIngredient(ModContent.ItemType<AncientWizardTech>(), 1);
			val.AddTile(TileID.Tables);
			val.AddTile(TileID.Chairs);
			val.SetResult(this, 1);
			val.AddRecipe();
		}
	}

	public class MusicBoxConsoleTitle : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Music Box (Title Console)");
		}

		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = ModContent.TileType<Tiles.MusicBoxConsoleTitle>();
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.LightRed;
			item.value = Item.buyPrice(0, 10, 0, 0);
			item.accessory = true;
		}

		public override void AddRecipes()
		{
			ModRecipe val = new ModRecipe(mod);
			val.AddIngredient(ItemID.MusicBoxTitle, 1);
			val.AddIngredient(ModContent.ItemType<AncientWizardTech>(), 1);
			val.AddTile(TileID.Tables);
			val.AddTile(TileID.Chairs);
			val.SetResult(this, 1);
			val.AddRecipe();
		}
	}

	public class MusicBoxConsoleTutorial : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Music Box (Tutorial Console)");
		}

		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = ModContent.TileType<Tiles.MusicBoxConsoleTutorial>();
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.LightRed;
			item.value = Item.buyPrice(0, 10, 0, 0);
			item.accessory = true;
		}

		public override void AddRecipes()
		{
			ModRecipe val = new ModRecipe(mod);
			val.AddIngredient(ItemID.MusicBoxAltOverworldDay, 1);
			val.AddIngredient(ItemID.MusicBoxAltUnderground, 1);
			val.AddIngredient(ModContent.ItemType<AncientWizardTech>(), 1);
			val.AddTile(TileID.Tables);
			val.AddTile(TileID.Chairs);
			val.SetResult(this, 1);
			val.AddRecipe();
		}
	}

}
