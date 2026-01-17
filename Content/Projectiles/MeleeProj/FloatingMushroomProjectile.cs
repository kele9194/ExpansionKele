using ExpansionKele.Content.Buff;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class FloatingMushroomProjectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240; // 4秒存在时间
            Projectile.light = 0.5f; // 微弱发光
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不与瓦片碰撞，保持悬浮
            Projectile.alpha = 30; // 轻微透明
            Projectile.scale = 0.8f; // 较小的尺寸
            Projectile.ArmorPenetration=40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 0;
        }

        public override void AI()
        {
            // 检查是否有带蘑菇标记的敌人在附近
            NPC target = FindTarget();
            if (target != null && target.active && !target.friendly && target.Distance(Projectile.Center) <= 600f)
            {
                Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = directionToTarget * 10f; // 设置追踪速度
            }
            else
            {
                // 没有目标时继续浮动
                Projectile.ai[0]++; // 计时器
                
                // 上下浮动运动
                Projectile.velocity.Y = (float)System.Math.Sin(Projectile.ai[0] * 0.05f) * 0.5f;
                
                // 轻微的水平漂移
                Projectile.velocity.X = (float)System.Math.Cos(Projectile.ai[0] * 0.03f) * 0.3f;
            }
            
            // 随着时间减少透明度
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha += 5; // 逐渐变得更透明直到消失
            }
        }

        // 查找带蘑菇标记的目标
        private NPC FindTarget()
        {
            NPC closestTarget = null;
            float closestDistance = 400f; // 搜索范围为400像素

            foreach (NPC npc in Main.npc)
            {
                // 检查是否是有效的敌人，且带有蘑菇标记
                if (npc.active && !npc.friendly && npc.Distance(Projectile.Center) <= 400f && npc.HasBuff(ModContent.BuffType<MushroomSwordMark>()))
                {
                    float distance = npc.Distance(Projectile.Center);
                    if (distance <= closestDistance)
                    {
                        closestTarget = npc;
                        closestDistance = distance;
                    }
                }
            }

            return closestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制蘑菇的特殊效果
            return true;
        }
    }
}