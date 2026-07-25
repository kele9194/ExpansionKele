using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 强攻手斩切抛射体：
    /// - 直接生成在敌人身上，造成溅射传导伤害
    /// - 不造成暴击伤害
    /// - 免疫敌人防御（无视护甲）
    /// </summary>
    public class CenturionSlashProj : ModProjectile
    {
        /// <summary>使用死神镰刀抛射体纹理（旋转刀片效果，类似斩切）</summary>
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

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;          // 不穿透，每个敌人单独生成
            Projectile.timeLeft = 10;            // 短存在时间，瞬间命中后消失
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 0;              // 轻微透明
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // 每目标仅命中一次
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            // 抛射体静止在生成位置（敌人身上），仅做旋转动画
            Projectile.velocity = Vector2.Zero;
            Projectile.Opacity -= 0.1f;            // 轻微透明
        }

        /// <summary>
        /// 修改命中效果：
        /// - 免疫敌人防御（高额护甲穿透 + 防御效果归零）
        /// - 暴击倍率归零（不造成暴击伤害）
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.DefenseEffectiveness *= 0f;
            modifiers.DisableCrit(); // 暴击不造成额外伤害
        }

        /// <summary>
        /// 命中时产生斩击粒子特效
        /// </summary>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    target.position, target.width, target.height,
                    DustID.SilverFlame, 0f, 0f, 100, default, 0.8f
                );
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }

        /// <summary>
        /// 绘制为银白色斩击效果
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            // 使用缓存的贴图
            Texture2D tex = _cachedTexture.Value;
            
            // 原点设为中心，实现居中绘制
            Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);
            
            // 根据 alpha 值计算透明度，实现渐隐效果
            float opacity = Projectile.Opacity;
            
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