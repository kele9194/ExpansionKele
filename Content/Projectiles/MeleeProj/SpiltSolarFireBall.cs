using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class SplitSolarFireball : ModProjectile
    {
        private bool canDamage = false;
        private int timer = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1; // 单穿
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = false; // 受重力影响需要设为false
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = false;
        }
        
        public override void AI()
        {
            timer++;
            
            // 前20帧不造成伤害
            if (timer >= 20)
            {
                Projectile.friendly = true;
            }
            
            // 添加重力效果
            Projectile.velocity.Y += 0.2f;
            
            // 添加火焰粒子效果
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
            
            // 朝向移动方向旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 前20帧不消失
            if (timer < 20)
            {
                // 反弹效果
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X * 0.8f;
                }
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y * 0.8f;
                }
                return false; // 不销毁
            }
            return base.OnTileCollide(oldVelocity); // 正常销毁
        }
        
        public override void OnKill(int timeLeft)
        {
            // 死亡时产生粒子效果
            for (int i = 0; i < 5; i++)
            {
                Dust lavaDust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.Lava, 
                    Main.rand.NextFloat(-2f, 2f), 
                    Main.rand.NextFloat(-2f, 2f), 
                    100, 
                    default, 
                    1f
                );
                lavaDust.noGravity = false;
                
                Dust sparkDust = Dust.NewDustDirect(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.Torch, 
                    Main.rand.NextFloat(-1f, 1f), 
                    Main.rand.NextFloat(-1f, 1f), 
                    100, 
                    default, 
                    0.8f
                );
                sparkDust.noGravity = true;
            }
        }
    }
}