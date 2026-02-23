using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    // 主导弹（基于原版夜明弹，但可自定义属性）
    public class VortexMainProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // 使用原版夜明弹的纹理
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            // 复制原版夜明弹的基本属性
            Projectile.CloneDefaults(ProjectileID.VortexBeaterRocket);
            
            // 修改我们想要自定义的属性
            Projectile.penetrate = 3; // 穿透3个敌人
            Projectile.timeLeft = 600;
            Projectile.width = 16;
            Projectile.height = 16;
        }

        public override void AI()
        {
            // 使用原版夜明弹的AI，但我们可以添加额外的效果
            // 旋转导弹以匹配其速度方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 添加粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 
                    Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, default(Color), 1.2f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 255) * 0.8f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用原版夜明弹的绘制效果
            return true;
        }
    }
}