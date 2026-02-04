using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles.RangedProj;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Ammos
{
    public class ChromiumArrow : ModItem
    {
        public override string LocalizationCategory => "Items.Ammos";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 10;
            Item.height = 28;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 1.0f;
            Item.value = Item.sellPrice(0, 0, 0, 5);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<ChromiumArrowProjectile>();
            Item.shootSpeed = 6.5f;
            Item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            CreateRecipe(125)
                .AddIngredient(ModContent.ItemType<ChromiumBar>(), 1)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}