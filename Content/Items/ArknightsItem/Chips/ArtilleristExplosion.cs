using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ID;
using ReLogic.Content;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 炮手爆炸抛射体（带贴图绘制，居中显示）
    /// 贴图会随 alpha 渐隐消失
    /// </summary>
    public class ArtilleristExplosion : ModProjectile
    {
        // 缓存贴图资源，提高性能
        private static Asset<Texture2D> _cachedTexture;

        public override void Load()
        {
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }

        public override void Unload()
        {
            _cachedTexture = null;
        }

        public override void SetStaticDefaults()
        {
            // 不标记为不显示，因为现在要绘制贴图
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;   // 3格
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;           // 不穿透，只命中一次
            Projectile.timeLeft = 20;            
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;                // 初始完全可见，逐渐淡出
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;  // 每个敌人只生效一次
        }

        public override void AI()
        {
            Projectile.alpha += 13;  // 20 tick / 255 ≈ 13 per tick
            if (Projectile.alpha > 255)
            {
                Projectile.alpha = 255;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.DefenseEffectiveness *= 0.5f;
        }

        // 爆炸视觉特效
        public override void OnSpawn(IEntitySource source)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            // 产生爆炸粒子效果（Dust）
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center - new Vector2(8, 8),
                    16, 16,
                    DustID.Torch,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100,
                    Color.Orange,
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                dust.noGravity = true;
            }

            // 产生烟雾
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center - new Vector2(12, 12),
                    24, 24,
                    DustID.Smoke,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    100,
                    Color.LightGray,
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用缓存的贴图
            Texture2D tex = _cachedTexture.Value;
            
            // 原点设为中心，实现居中绘制
            Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);
            
            // 根据 alpha 值计算透明度，实现渐隐效果
            float opacity = (255 - Projectile.alpha) / 255f;
            
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * opacity,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            
            return false; // 已手动绘制，阻止默认绘制
        }
        /// <summary>
        /// 圆形碰撞检测：判断目标 hitbox 是否与爆炸圆相交
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Projectile.width / 2f;  // 碰撞半径
            
            // 计算目标矩形上距离圆心最近的点
            float closestX = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
            float closestY = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
            
            // 计算圆心到最近点的距离平方
            float distanceX = Projectile.Center.X - closestX;
            float distanceY = Projectile.Center.Y - closestY;
            float distanceSquared = distanceX * distanceX + distanceY * distanceY;
            
            // 如果距离小于半径，则发生碰撞
            return distanceSquared <= radius * radius;
        }
    }
}