using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Magic
{
    public class Fireworks : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Magic";
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(FireworksProjectile.PARTICLE_COUNT),
            ValueUtils.FormatFraction(FireworksProjectile.CanDamageFrac),
            ValueUtils.FormatValue(FireworksProjectile.damageRatio,true),
            ValueUtils.FormatValue(FireworksParticle.ArmorPenetrationBonus)

        );

        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.damage = ExpansionKele.ATKTool(35, 42);
            Item.DamageType = DamageClass.Magic;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FireworksProjectile>();
            Item.shootSpeed = 18f;
            Item.mana = 5;
            Item.maxStack = 9999;
            Item.consumable = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 发射烟花弹幕
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe(200)
                .AddIngredient(ItemID.Torch, 10)
                .AddIngredient(ItemID.ExplosivePowder, 5)
                .AddIngredient(ItemID.ManaCrystal, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}