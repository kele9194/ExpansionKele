using ExpansionKele.Content.Items.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Placeables
{
    public class StarryBar : ModItem
    {
        public override string LocalizationCategory => "Items.Placeables";
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 61; // 高于 FullMoonOre。
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("星元锭");
        Item.DefaultToPlaceableTile(ModContent.TileType<StarryBarTile>());
            base.Item.width = 30;
		base.Item.height = 24;
		base.Item.useStyle = ItemUseStyleID.Swing;
		base.Item.useTurn = true;
		base.Item.useAnimation = 15;
		base.Item.useTime = 10;
		base.Item.autoReuse = true;
		base.Item.consumable = true;
		base.Item.maxStack = 9999;
		base.Item.value = Item.sellPrice(0, 0,40);
		base.Item.rare = ItemRarityID.LightPurple;
        }

        public override void AddRecipes()
        {
            
        }
    }
}