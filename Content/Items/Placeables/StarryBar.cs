using ExpansionKele.Content.Customs;
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
		Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
        Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }

        public override void AddRecipes()
        {
            
        }
    }
}