using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    public class SolarSword : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons";

        public override void SetDefaults()
        {
            // 基本属性设置
            Item.width = 140;
            Item.height = 140;
            Item.damage = ExpansionKele.ATKTool(300,400);
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = false;
            Item.knockBack = 6f;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SolarFireball>();
            Item.shootSpeed = 15f;
            Item.scale=0.75f;
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // 添加剑挥舞时的火焰粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Lava, 0f, 0f, 0, default, 1f);
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中敌人时产生爆炸效果
            // 在目标位置产生爆炸粒子效果
            for (int i = 0; i < 20; i++)
            {
                Dust lavaDust = Dust.NewDustDirect(
                    target.position, 
                    target.width, 
                    target.height, 
                    DustID.Lava, 
                    Main.rand.NextFloat(-5f, 5f), 
                    Main.rand.NextFloat(-5f, 5f), 
                    100, 
                    default, 
                    2f
                );
                lavaDust.noGravity = false; // 让熔岩粒子受重力影响，形成下落效果
                
                // 添加一些火花效果
                Dust sparkDust = Dust.NewDustDirect(
                    target.position, 
                    target.width, 
                    target.height, 
                    DustID.Torch, 
                    Main.rand.NextFloat(-3f, 3f), 
                    Main.rand.NextFloat(-3f, 3f), 
                    100, 
                    default, 
                    1.5f
                );
                sparkDust.noGravity = true;
            }
            
            // 造成额外的爆炸伤害（与武器伤害相同）
            int explosionDamage = (int)(damageDone*0.8f);
            player.ApplyDamageToNPC(target, explosionDamage, 0f, 0, false);
            
            // 施加破晓效果（Daybreak debuff），持续3秒（180 ticks）
            target.AddBuff(BuffID.Daybreak, 180);
            
            // 播放爆炸声音
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, target.Center);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FieryGreatsword, 1) // 使用火山作为合成材料之一
                .AddIngredient(ItemID.FragmentSolar, 5) // 日耀碎片*5
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
        
        // 添加发射火球的功能
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 50%概率发射火球
            if (Main.rand.NextBool(2))
            {
                Projectile.NewProjectile(source, position, velocity, type, (int)(damage * 0.8f), knockback, player.whoAmI);
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
}