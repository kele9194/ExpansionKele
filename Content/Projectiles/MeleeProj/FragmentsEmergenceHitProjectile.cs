using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class FragmentsEmergenceHitProjectile : ModProjectile
    {
        
        private readonly Color[] colors = {
            new Color(128, 0, 128),   // 紫色
            new Color(255, 255, 0),   // 黄色
            new Color(0, 255, 0),     // 绿色
            new Color(0, 0, 255),     // 蓝色
            new Color(255, 0, 0),     // 红色
            new Color(255, 255, 255)  // 白色
        };

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("碎片浮现击中效果");
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration=20;
        }

        public override void AI()
        {
            // 添加粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default(Color), 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // 应用重力
            Projectile.velocity.Y += 0.2f;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // 随机选择一种颜色并保持不变
            int colorIndex = (int)Projectile.ai[0]; // 使用ai[0]存储颜色索引
            if (colorIndex < 0 || colorIndex >= colors.Length)
                colorIndex = 0;

            return colors[colorIndex];
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 碰撞时产生粒子效果并消失
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default(Color), 1f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
            Projectile.Kill();
            return false;
        }
    }
}