using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class FragmentsEmergence : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("碎片浮现");
            // Tooltip.SetDefault("挥舞过程中发射碎片，碎片前进过程中不断减速到0后滞留，滞留时期碎片伤害变成原来2倍");
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.damage = ExpansionKele.ATKTool(12,13);
            Item.knockBack = 6f;
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<FragmentsEmergenceProjectile>();
            Item.shootSpeed = 30f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            Main.projectile[proj].originalDamage=damage;
            return false;
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 当剑刃击中敌人时，迸发出彩色弹幕
            for (int i = 0; i < 3; i++)
            {
                // 随机选择一种颜色
                int colorIndex = Main.rand.Next(6);
                
                // 随机方向，斜向上
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -2f));
                
                // 在击中位置生成弹幕
                Projectile.NewProjectile(
                    player.GetSource_OnHit(target), 
                    target.Center, 
                    velocity, 
                    ModContent.ProjectileType<FragmentsEmergenceHitProjectile>(), 
                    damageDone*2/5, 
                    0f, 
                    player.whoAmI,
                    colorIndex // 传递颜色索引
                );
            }
        }

        public override void AddRecipes()
        {
            // 金阔剑配方
            CreateRecipe()
                .AddIngredient(ItemID.GoldBroadsword, 1)
                .AddIngredient(ItemID.Diamond, 1)
                .AddIngredient(ItemID.Ruby, 1)
                .AddIngredient(ItemID.Sapphire, 1)
                .AddIngredient(ItemID.Emerald, 1)
                .AddIngredient(ItemID.Topaz, 1)
                .AddIngredient(ItemID.Amethyst, 1)
                .AddTile(TileID.Anvils)
                .Register();
                
            // 铂金阔剑配方
            CreateRecipe()
                .AddIngredient(ItemID.PlatinumBroadsword, 1)
                .AddIngredient(ItemID.Diamond, 1)
                .AddIngredient(ItemID.Ruby, 1)
                .AddIngredient(ItemID.Sapphire, 1)
                .AddIngredient(ItemID.Emerald, 1)
                .AddIngredient(ItemID.Topaz, 1)
                .AddIngredient(ItemID.Amethyst, 1)
                .AddTile(TileID.Anvils)
                .Register();
                
        }
    }
}