using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Placeables
{
    public class PsionicOre : ModItem
    {
        public override string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 105; // 排序优先级
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.PsionicOreTile>());
            Item.width = 16;
            Item.height = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = ItemUtils.CalculateValueFromRecipes(this); // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }

        public override void AddRecipes()
        {
        }
    }
}