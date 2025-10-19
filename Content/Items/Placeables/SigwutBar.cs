using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExpansionKele.Content.Items.Placeables
{
    public class SigwutBar : ModItem
    {
        public override string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 61; // 高于 FullMoonOre。
        }
        public override void SetDefaults()
        {
            //base.Item.SetNameOverride("西格武特锭");
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
            Recipe.Create(ModContent.ItemType<SigwutBar>())
            .AddIngredient(ItemID.MeteoriteBar, 1)
            .AddIngredient(ItemID.Bone, 1)
            .AddIngredient(ItemID.HellstoneBar, 1) 
            .AddTile(TileID.Hellforge)         
            .Register();
        }
    }
}