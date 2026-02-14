using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class GaussRifleProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 20;
            
            Projectile.aiStyle = 0; // 不使用标准AI样式，我们将自定义
        }
        
        public override void AI()
        {
            // 添加粒子效果
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.Electric, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 0, default, 1.2f);
                
            // 添加光照效果
            Lighting.AddLight(Projectile.Center, new Vector3(0f, 0.5f, 1f) * 0.75f); // 蓝白色光
            
            // 设置旋转角度跟随运动方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        
        public override void OnKill(int timeLeft)
        {
            // 死亡时产生爆炸效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Electric, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 0, default, 1.5f);
            }
            
            // 播放声音
            
        }
    }
}