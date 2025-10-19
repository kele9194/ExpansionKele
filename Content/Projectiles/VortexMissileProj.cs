using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
// 在文件顶部添加引用（如果尚未添加）
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles
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