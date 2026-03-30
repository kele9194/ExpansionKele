using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class FireworksParticle : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.MagicProj";
        private const int MAX_TIME_LEFT = 60;
        public const float GRAVITY = 0.08f;
        public static int ArmorPenetrationBonus=40;
        public override void SetStaticDefaults()
        {
            // 设置尾迹长度为 10 帧，使用尾迹模式 2（平滑渐变）
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MAX_TIME_LEFT;
            Projectile.light = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // 应用重力
            Projectile.velocity.Y += GRAVITY;

            // 逐渐减速
            Projectile.velocity *= 0.98f;

            // 旋转效果
            Projectile.rotation += 0.05f;

            // 创建尾迹粒子
            if (Main.rand.NextBool(3))
            {
                CreateParticleDust();
            }

            // 逐渐消失
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration+=9999;
        }

        private void CreateParticleDust()
        {
            Color particleColor = GetParticleColor();
            
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.FireworksRGB, 0f, 0f, 100, particleColor, 0.6f);
            dust.noGravity = true;
            dust.velocity *= 0.5f;
            dust.fadeIn = 0.5f;
        }

        private Color GetParticleColor()
        {
            byte r = (byte)Projectile.ai[0];
            byte g = (byte)Projectile.ai[1];
            byte b = (byte)Projectile.ai[2];
            
            if (r == 0 && g == 0 && b == 0)
            {
                return new Color(255, 255, 255);
            }
            
            return new Color(r, g, b);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中时产生小火花
            Color particleColor = GetParticleColor();
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, 
                    DustID.FireworksRGB, 0f, 0f, 100, particleColor, 1f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 碰撞时产生火花并消失
            Color particleColor = GetParticleColor();
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.FireworksRGB, 0f, 0f, 100, particleColor, 1f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Color particleColor = GetParticleColor();
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.FireworksRGB, 0f, 0f, 100, particleColor, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color particleColor = GetParticleColor();
            return new Color(particleColor.R, particleColor.G, particleColor.B, 255 - Projectile.alpha);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 添加光照效果
            Color particleColor = GetParticleColor();
            Vector3 lightColorVec = particleColor.ToVector3() * 0.5f;
            Lighting.AddLight(Projectile.Center, lightColorVec);
            
            return true;
        }
    }
}