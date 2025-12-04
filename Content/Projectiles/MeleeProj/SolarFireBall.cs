using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class SolarFireball : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1; // 单穿
            Projectile.timeLeft = 600;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.scale = 1.5f; // 贴图放大1.5倍
            Projectile.extraUpdates=2;
            
        }
        
        public override void AI()
        {
            // 添加火焰粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            
            // 朝向移动方向旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnSplitFireballs(target.Center, hit.Knockback);
        }
        
        public override void OnKill(int timeLeft)
        {
            // 碰撞到物块时也分裂
            if (timeLeft <= 0)
            {
                SpawnSplitFireballs(Projectile.Center, 0f);
            }
            
            // 死亡时产生爆炸效果
            for (int i = 0; i < 10; i++)
            {
                Dust lavaDust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.Lava, 
                    Main.rand.NextFloat(-3f, 3f), 
                    Main.rand.NextFloat(-3f, 3f), 
                    100, 
                    default, 
                    1.5f
                );
                lavaDust.noGravity = false;
                
                Dust sparkDust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.Torch, 
                    Main.rand.NextFloat(-2f, 2f), 
                    Main.rand.NextFloat(-2f, 2f), 
                    100, 
                    default, 
                    1f
                );
                sparkDust.noGravity = true;
            }
            
            // 播放声音
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
        
        private void SpawnSplitFireballs(Vector2 position, float parentKnockback)
        {
            int splitCount = Main.rand.Next(3, 5); // 3-4个小火球
            
            for (int i = 0; i < splitCount; i++)
            {
                // 只在向上正负30度之间随机角度
                float rotation = Main.rand.NextFloat(-MathHelper.Pi / 4, MathHelper.Pi / 4) - MathHelper.PiOver2;
                // 固定速度为4
                float speed = 6f;
                Vector2 velocity = rotation.ToRotationVector2() * speed;
                
                // 创建分裂火球
                Projectile splitFireball = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    position,
                    velocity,
                    ModContent.ProjectileType<SplitSolarFireball>(),
                    (int)(Projectile.damage * 0.5f), // 30%的伤害
                    parentKnockback,
                    Projectile.owner
                );
                // 设置分裂火球略小的缩放比例
                splitFireball.scale = Main.rand.NextFloat(0.4f, 0.9f);
            }
        }
    }
}