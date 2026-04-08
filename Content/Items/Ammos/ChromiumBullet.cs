using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Projectiles.RangedProj;
using ExpansionKele.Content.Customs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Ammos
{
    public class ChromiumBullet : ModItem
    {
        public override string LocalizationCategory => "Items.Ammos";
        
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 1.5f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.shoot = ModContent.ProjectileType<ChromiumBulletProjectile>();
            Item.shootSpeed = 12f;
            Item.ammo = AmmoID.Bullet;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe(200)
                .AddIngredient<ChromiumBar>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}