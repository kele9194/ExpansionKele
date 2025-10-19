using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace ExpansionKele.Content.Projectiles
{
    public class LifePercentageProjectile : ModProjectile
    {
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == -1 ? null : Main.npc[(int)Projectile.ai[0]];
            set => Projectile.ai[0] = value?.whoAmI ?? -1;
        }

        public ref float DelayTimer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 100;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // 初始延迟以避免立即开始追踪
            if (DelayTimer < 10)
            {
                DelayTimer += 1;
                return;
            }

            // 如果没有目标或当前目标无效，则尝试重新寻找目标
            if (HomingTarget == null || !IsValidTarget(HomingTarget))
            {
                HomingTarget = FindClosestNPC(1000f);
                if (HomingTarget == null)
                    return;
            }

            // 追踪目标
            Vector2 toTarget = HomingTarget.Center - Projectile.Center;
            float distance = toTarget.Length();
            if (distance < 1000f)
            {
                toTarget.Normalize();
                // 保持速度不变，只调整方向
                Projectile.velocity = toTarget * Projectile.velocity.Length();
            }
            else
            {
                // 如果目标超出范围，停止追踪
                HomingTarget = null;
            }

            // 旋转抛射体以匹配其速度方向
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            foreach (var npc in Main.npc)
            {
                if (IsValidTarget(npc))
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        // 优先考虑鼠标方向上的敌人
                        Vector2 toNPC = npc.Center - Main.player[Projectile.owner].MountedCenter;
                        float angleToNPC = toNPC.ToRotation();
                        float angleToMouse = Main.MouseWorld.ToRotation();
                        float angleDifference = Math.Abs(angleToNPC - angleToMouse);

                        // 如果当前目标更接近鼠标方向，则更新最近目标
                        if (closestNPC == null || angleDifference < Math.Abs((closestNPC.Center - Main.player[Projectile.owner].MountedCenter).ToRotation() - angleToMouse))
                        {
                            sqrMaxDetectDistance = sqrDistanceToTarget;
                            closestNPC = npc;
                        }
                    }
                }
            }

            return closestNPC;
        }

        private bool IsValidTarget(NPC target)
        {
            // 排除城镇NPC
            if (target.townNPC)
                return false;

            // 排除生命值过低的NPC（可以认为这些是无害的小动物）
            if (target.lifeMax < 10)
                return false;

            return target.active && !target.dontTakeDamage && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 计算基于目标生命百分比的额外伤害
            target.immune[Projectile.owner] = 2;
            int lifePercentageDamage = (int)(target.lifeMax * 0.005); // 例如，0.5%生命值
            int totalDamage = (int)(Projectile.damage * 1.25f + lifePercentageDamage);

            // 更新抛射体的伤害属性（仅用于显示）
            Projectile.damage = totalDamage;
        }
    }
}