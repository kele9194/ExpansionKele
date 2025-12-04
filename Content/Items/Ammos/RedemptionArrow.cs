using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.OtherItem;
using ExpansionKele.Content.Projectiles.RangedProj;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Ammos
{
    public class RedemptionArrow : ModItem
    {
        public override string LocalizationCategory => "Items.Ammos";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 2.0f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.shoot = ModContent.ProjectileType<RedemptionArrowProjectile>();
            Item.shootSpeed = 4f;
            Item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            CreateRecipe(200)
                .AddIngredient<RedemptionShard>(4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}