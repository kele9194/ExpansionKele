using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.Tiles;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Placeables
{
    public class FullMoonOre : ModItem
    {
        public override string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 60; // 排序优先级高于 ExampleBar。
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月矿");
            Item.DefaultToPlaceableTile(ModContent.TileType<FullMoonOreTile>());
            Item.width = 16;
            Item.height = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }

        public override void AddRecipes()
        {
            
    }
}}