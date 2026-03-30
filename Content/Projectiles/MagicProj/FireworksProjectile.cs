using ExpansionKele.Content.Customs;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class FireworksProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.MagicProj";
        private const int EXPLOSION_RADIUS = 200;
        public const int PARTICLE_COUNT = 15;
        public static readonly int[] CanDamageFrac = new int[2] { 1, 3 };
        public static float damageRatio =0.4f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        // ... existing code ...
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 1;
        }

// ... existing code ...

        // ... existing code ...
         public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
            
            if (Projectile.timeLeft < 150 && Projectile.velocity.Length() < Projectile.ai[1] * 2f)
            {
                float accelerationFactor = 1.005f;
                Projectile.velocity *= accelerationFactor;
            }
            
            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[1] = Projectile.velocity.Length();
            }
            
            if (Projectile.ai[0] == 0)
            {
                Color randomColor = GetRandomFireworkColor();
                Projectile.ai[0] = randomColor.PackedValue;
            }
            
            if (Main.rand.NextBool(2))
            {
                CreateTrailDust();
            }
        }
// ... existing code ...
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(Projectile.owner == Main.myPlayer){
            Explode();
            }
        }

        public override void OnKill(int timeLeft)
        {
            if(Projectile.owner == Main.myPlayer){
            Explode();
            }
        }
        private void CreateTrailDust()
        {
            Color trailColor = GetStoredColor();
            
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.FireworksRGB, 0f, 0f, 100, trailColor, 0.8f);
            dust.noGravity = true;
            dust.velocity *= 0.3f;
            dust.fadeIn = 0.5f;
        }
        
        // ... existing code ...
        private Color GetStoredColor()
        {
            uint packedValue = (uint)Projectile.ai[0];
            Color color = new Color(
                (byte)(packedValue & 0xFF),
                (byte)((packedValue >> 8) & 0xFF),
                (byte)((packedValue >> 16) & 0xFF),
                (byte)((packedValue >> 24) & 0xFF)
            );
            
            if (color.R == 0 && color.G == 0 && color.B == 0)
            {
                return GetRandomFireworkColor();
            }
            
            return color;
        }
// ... existing code ...
        private void Explode()
        {
            // 播放爆炸音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 创建爆炸视觉效果
            for (int i = 0; i < 15; i++)
            {
                Dust explosionDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.FireworksRGB, 0f, 0f, 100, default, 2f);
                explosionDust.noGravity = true;
                explosionDust.velocity *= 3f;
            }

            // 向四周发射彩色粒子
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < PARTICLE_COUNT; i++)
                {
                    float angle = MathHelper.ToRadians(i * (360f / PARTICLE_COUNT));
                    Vector2 velocity = new Vector2(
                        (float)System.Math.Cos(angle) * Main.rand.NextFloat(6f, 10f),
                        (float)System.Math.Sin(angle) * Main.rand.NextFloat(6f, 10f)
                    );

                    // 随机颜色
                    Color randomColor = GetRandomFireworkColor();
                    
                    // 只有 1/3 的概率造成伤害
                    bool canDamage = Main.rand.NextFloat() < ValueUtils.ConvertToFloat(CanDamageFrac);
                    
                    int particle = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<FireworksParticle>(),
                        canDamage ? ValueUtils.ProbabilisticRound(Projectile.damage *damageRatio) : 0,
                        Projectile.knockBack,
                        Projectile.owner,
                        randomColor.PackedValue
                    );

                    // 设置粒子的颜色数据
                    Main.projectile[particle].ai[0] = randomColor.R;
                    Main.projectile[particle].ai[1] = randomColor.G;
                    Main.projectile[particle].ai[2] = randomColor.B;
                }
            }
        }
// ... existing code ...

        private Color GetRandomFireworkColor()
        {
            Color[] colors = new Color[]
            {
                new Color(255, 0, 0),      // 红色
                new Color(255, 165, 0),    // 橙色
                new Color(255, 255, 0),    // 黄色
                new Color(0, 255, 0),      // 绿色
                new Color(0, 255, 255),    // 青色
                new Color(0, 0, 255),      // 蓝色
                new Color(128, 0, 128),    // 紫色
                new Color(255, 0, 255),    // 粉色
                new Color(255, 255, 255)   // 白色
            };

            return colors[Main.rand.Next(colors.Length)];
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制发光效果
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.8f, 0.2f));
            return true;
        }
    }
}