using System;
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
    /// 散射手扇形AOE弹幕。
    /// 以玩家为中心，向玩家面向方向喷射扇形范围的攻击。
    /// 伤害衰减：
    /// - 0 ~ 200 像素内：75% 存储伤害
    /// - 200 ~ 400 像素内：40% 存储伤害
    /// </summary>
    public class SpreadProjectile : ModProjectile
    {
        // 扇形参数
        private const float MaxRadius = 400f;
        private const float InnerRadius = 200f;
        private const float FanHalfAngle = MathHelper.PiOver2; // 90° 半角 → 总共180°扇形

        // 缓存贴图
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
            // 不需要默认绘制
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;    // 碰撞箱很小，使用自定义碰撞
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 6;  // 短暂存在（约0.1秒），每帧检测碰撞
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 200;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // 每个敌人只命中一次
        }

        public override void AI()
        {
            // 保持弹幕中心与玩家中心同步（用于碰撞检测）
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                Projectile.Center = owner.Center;
            }

            // 淡出效果（参考炮手弹幕的透明度模式：alpha 每帧递增）
            Projectile.alpha += 8;
            if (Projectile.alpha > 255)
                Projectile.alpha = 255;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 播放散射音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);

            // 产生扇形扩散粒子效果
            float facingAngle = Projectile.ai[0];
            for (int i = 0; i < 30; i++)
            {
                // 在扇形范围内随机生成粒子
                float angleOffset = Main.rand.NextFloat(-FanHalfAngle, FanHalfAngle);
                float distance = Main.rand.NextFloat(20f, MaxRadius);
                float particleAngle = facingAngle + angleOffset;
                Vector2 offset = new Vector2(
                    (float)Math.Cos(particleAngle) * distance,
                    (float)Math.Sin(particleAngle) * distance
                );

                Dust dust = Dust.NewDustDirect(
                    Projectile.Center + offset - new Vector2(4, 4),
                    8, 8,
                    DustID.Cloud,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1f, 1f),
                    80,
                    Color.LightBlue,
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                dust.noGravity = true;
            }

            // 中心闪光粒子
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 velocity = new Vector2(
                    (float)Math.Cos(angle) * speed,
                    (float)Math.Sin(angle) * speed
                );
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center - new Vector2(2, 2),
                    4, 4,
                    DustID.AncientLight,
                    velocity.X, velocity.Y,
                    100,
                    Color.White,
                    Main.rand.NextFloat(1.0f, 1.8f)
                );
                dust.noGravity = true;
            }
        }

        /// <summary>
        /// 自定义扇形碰撞检测
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 center = Projectile.Center;
            float facingAngle = Projectile.ai[0];

            // 计算目标碰撞箱上距离中心最近的点
            float closestX = MathHelper.Clamp(center.X, targetHitbox.Left, targetHitbox.Right);
            float closestY = MathHelper.Clamp(center.Y, targetHitbox.Top, targetHitbox.Bottom);
            Vector2 closestPoint = new Vector2(closestX, closestY);

            // 计算距离
            float dist = Vector2.Distance(center, closestPoint);
            if (dist > MaxRadius) return false;

            // 计算方向角度差
            Vector2 dirToTarget = closestPoint - center;
            if (dirToTarget == Vector2.Zero) return true; // 玩家自身位置
            float angleToTarget = dirToTarget.ToRotation();
            float angleDiff = MathHelper.WrapAngle(angleToTarget - facingAngle);

            // 检查是否在扇形范围内
            if (Math.Abs(angleDiff) > FanHalfAngle) return false;

            return true;
        }

        /// <summary>
        /// 根据距离应用伤害衰减
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float dist = Vector2.Distance(Projectile.Center, target.Center);

            if (dist <= InnerRadius)
            {
                // 0 ~ 200px：75% 伤害
                modifiers.FinalDamage *= 0.75f;
            }
            else if (dist <= MaxRadius)
            {
                // 200 ~ 400px：40% 伤害
                modifiers.FinalDamage *= 0.4f;
            }
            // 超过400px的不应该进入到这里，但以防万一
        }

        /// <summary>
        /// 绘制扇形视觉效果。
        /// 重置绘制位置为玩家中心，确保视觉始终与玩家位置同步。
        /// 硬右半圆贴图：面朝右正常绘制，面朝左时水平翻转。
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner == null || !owner.active) return false;

            Texture2D tex = _cachedTexture.Value;

            // 重置绘制位置为玩家中心
            Vector2 drawPos = owner.Center - Main.screenPosition;

            // 原点设为中心底部（贴图扇形尖部）
            Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);

            // 使用 ai[0] 存储的瞄准角度旋转贴图，使扇形朝向任意方向
            float rotation = Projectile.ai[0];
            SpriteEffects effects = SpriteEffects.None;

            // 根据剩余时间计算透明度（淡出效果）
            // 根据 alpha 计算透明度（参考炮手弹幕的淡出模式）
            float opacity = (255 - Projectile.alpha) / 255f;
            Color drawColor = lightColor * opacity;

            Main.EntitySpriteDraw(
                tex,
                drawPos,
                null,
                drawColor,
                rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}