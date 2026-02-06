using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class RippleProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("荡漾水弹");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2; // 可穿透一个敌人（总共可击中2个目标）
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.netUpdate = true;
        }

        // ... existing code ...
        // ... existing code ...
        public override void AI()
        {
            // 添加水弹特效
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // 使射弹朝向移动方向旋转
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // 获取玩家到鼠标的方向矢量A
            Player player = Main.player[Projectile.owner];
            Vector2 mousePosition = Main.MouseWorld;
            Vector2 playerToMouse = mousePosition - player.Center;
            
            // 确保方向矢量不为零
            if (playerToMouse != Vector2.Zero)
            {
                playerToMouse.Normalize();
                
                // 计算弹幕到玩家-鼠标方向线的垂直距离
                Vector2 playerToProjectile = Projectile.Center - player.Center;
                // 使用叉积计算垂直距离
                float perpendicularDistance = Vector2.Dot(playerToProjectile, new Vector2(-playerToMouse.Y, playerToMouse.X));
                
                // 创建两个力：一个与A矢量同向，另一个与其垂直
                // 大幅减小力的强度以实现轻微调整效果
                Vector2 parallelForce = playerToMouse * 0.1f; // 与A矢量同向的力
                Vector2 perpendicularForce = new Vector2(-playerToMouse.Y, playerToMouse.X) * (-perpendicularDistance * 0.001f); // 垂直于A矢量的力，根据距离调整，使弹幕回到线上
                
                // 施加力到弹幕速度上
                Projectile.velocity += parallelForce + perpendicularForce;
                
                // 限制最大速度以避免加速过快
                float maxSpeed = 16f;
                if (Projectile.velocity.Length() > maxSpeed)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * maxSpeed;
                }
            }
        }
// ... existing code ...
                //

        public override void OnKill(int timeLeft)
        {
            // 消失时产生水花效果
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }
    }
}