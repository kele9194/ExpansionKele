using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.ID;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 领主剑气 - 领主子职业的特有射弹。
    /// 
    /// 特性：
    /// - 基础伤害为武器伤害的60%
    /// - 速度固定为8
    /// - 伤害随飞行距离衰减：
    ///   128px内保持60%伤害
    ///   128px~320px线性衰减至20%
    ///   320px外保持20%
    /// </summary>
    public class LordSlashProjectile : ModProjectile
    {
        /// <summary>发射时的原始伤害（用于距离衰减计算）</summary>
        private float originalDamage;

        /// <summary>发射位置（用于距离衰减计算）</summary>
        private Vector2 firePosition;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;        // 约1.5秒存活时间
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.scale = 1f;   // 提高精度，使剑气更流畅
        }

        public override void OnSpawn(IEntitySource source)
        {
            firePosition = Projectile.Center;
            originalDamage = Projectile.damage;
        }

        public override void AI()
        {
            // 计算飞行距离
            float distance = Vector2.Distance(Projectile.Center, firePosition);

            // 距离衰减逻辑
            float damageMult;
            if (distance <= 128f)
            {
                damageMult = 1f; // 100%原始伤害（即武器伤害的60%）
            }
            else if (distance >= 320f)
            {
                damageMult = 0.333f; // 33%原始伤害（即武器伤害的20%）
            }
            else
            {
                // 128~320px 线性衰减：1.0 → 0.333
                damageMult = 1f - (distance - 128f) / 192f * 0.667f;
            }

            Projectile.damage = (int)(originalDamage * damageMult);

            // 剑气旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4;

            // 渐隐效果（最后0.5秒）
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)(255f * (1f - Projectile.timeLeft / 30f));
            }

            // 光效
            Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 0.5f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration+=20;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 剑气半透明效果
            lightColor *= (1f - Projectile.alpha / 255f);
            return base.PreDraw(ref lightColor);
        }

        public override void OnKill(int timeLeft)
        {
            // 消散时的粒子效果
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.BlueCrystalShard, 
                    Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f,
                    50, default, 1.2f
                );
                dust.noGravity = true;
            }
        }
    }
}