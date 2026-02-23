using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
// 在文件顶部添加引用（如果尚未添加）
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    // 追踪导弹
    public class VortexHomingProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.scale = 0.9f; // 稍小一点以区分
        }

        public override void AI()
        {
            // 前5帧不追踪
            if (Projectile.timeLeft > 595)
            {
                // 只旋转，不追踪
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                
                // 添加粒子效果
                if (Main.rand.NextBool(4))
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 
                        Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 0, default(Color), 1f);
                }
                return;
            }

            // 定义追踪参数
            float maxTrackingDistance = 640f; // 从Data类中获取的最大追踪距离
            float speed = 20f;
            float turnResistance = 10f;
            Vector2 mousePosition = Main.MouseWorld;

            // 追踪目标
            ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed, maxTrackingDistance, turnResistance, mousePosition);
            
            // 添加粒子效果
            if (Main.rand.NextBool(4))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 
                    Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, default(Color), 1f);
            }
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.velocity.Y != 0)
            {
                modifiers.FinalDamage *= 1.5f; // 增加200%伤害
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 200, 255, 255) * 0.7f;
        }
    }
}