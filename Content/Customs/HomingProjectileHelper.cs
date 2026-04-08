using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;

namespace ExpansionKele.Content.Customs
{
    public static class ProjectileHelper
    {
        private static Dictionary<int, ProjectileLockData> _lockStates = new Dictionary<int, ProjectileLockData>();
        
        private static int _cleanupCounter = 0;
        private const int CLEANUP_INTERVAL = 600;
        
        private struct ProjectileLockData
        {
            public int LockedTargetId;
            public float LockTimer;
            
            public ProjectileLockData(int targetId, float timer = 0f)
            {
                LockedTargetId = targetId;
                LockTimer = timer;
            }
        }

        /// <summary>
        /// 改进的Boss优先追踪方法
        /// </summary>
        public static void FindAndMoveTowardsTarget(Terraria.Projectile projectile, float speed, float maxTrackingDistance, float turnResistance, bool bossPriority = true)
        {
            NPC target = FindOptimalTarget(projectile.Center, maxTrackingDistance, bossPriority, projectile);
            if (target != null)
            {
                Vector2 direction = target.Center - projectile.Center;
                float distance = direction.Length();
                direction.Normalize();

                // 平滑追踪目标
                projectile.velocity = (projectile.velocity * (turnResistance - 1) + direction * speed) / turnResistance;

                // 更新导弹的方向
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                // 如果没有找到目标，导弹继续沿当前方向飞行
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

        }

        /// <summary>
        /// 鼠标导向的Boss优先追踪
        /// </summary>
        public static void FindAndMoveTowardsTarget(Terraria.Projectile projectile, float speed, float maxTrackingDistance, float turnResistance, Vector2 mousePosition, bool bossPriority = true)
        {
            NPC target = FindOptimalTarget(projectile.Center, maxTrackingDistance, bossPriority, projectile, mousePosition);
            
            if (target != null)
            {
                Vector2 direction = target.Center - projectile.Center;
                float distance = direction.Length();
                direction.Normalize();

                // 平滑追踪目标
                projectile.velocity = (projectile.velocity * (turnResistance - 1) + direction * speed) / turnResistance;

                // 更新导弹的方向
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                // 如果没有找到目标，导弹继续沿当前方向飞行
                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

        }

        /// <summary>
        /// 智能目标选择 - 解决Boss追踪问题的核心方法
        /// </summary>
        private static NPC FindOptimalTarget(Vector2 position, float maxDistance, bool bossPriority, Terraria.Projectile projectile, Vector2? mousePosition = null)
        {
            NPC bestTarget = null;
            float bestScore = float.MinValue;
            
            // 获取当前已锁定的目标（如果有的话）
            NPC lockedTarget = GetLockedTarget(projectile);
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!IsValidTarget(npc, position, maxDistance))
                    continue;

                // 计算目标评分
                float score = CalculateTargetScore(npc, position, bossPriority, lockedTarget, mousePosition);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = npc;
                }
            }

            // 如果找到了新目标，更新锁定状态
            if (bestTarget != null && bestTarget != lockedTarget)
            {
                SetLockedTarget(projectile, bestTarget);
            }

            return bestTarget;
        }

        /// <summary>
        /// 计算目标优先级评分
        /// </summary>
        private static float CalculateTargetScore(NPC npc, Vector2 position, bool bossPriority, NPC lockedTarget, Vector2? mousePosition)
        {
            float score = 0f;
            
            // 1. Boss优先级加成 (最高权重)
            if (bossPriority && npc.boss)
            {
                score += 3000f;
            }
            
            // 2. 当前锁定目标加成
            if (npc == lockedTarget)
            {
                score += 500f;
            }
            
            // 3. 距离因素 (较近距离得分更高)
            float distance = Vector2.Distance(npc.Center, position);
            score += Math.Max(0, 200f - distance * 0.5f);
            
            // 4. 血量因素 (高血量目标得分更高)
            score += npc.lifeMax * 0.1f;
            
            // 5. 鼠标导向因素 (如果提供了鼠标位置)
            if (mousePosition.HasValue)
            {
                Vector2 directionToNpc = npc.Center - position;
                Vector2 directionToMouse = mousePosition.Value - position;
                float angleDiff = Math.Abs(directionToNpc.ToRotation() - directionToMouse.ToRotation());
                // 角度差越小得分越高
                score += Math.Max(0, 100f - angleDiff * 20f);
            }
            
            // 6. 移动速度惩罚 (静止目标得分更高)
            score -= npc.velocity.Length() * 5f;
            
            return score;
        }

        /// <summary>
        /// 目标有效性验证
        /// </summary>
        private static bool IsValidTarget(NPC npc, Vector2 position, float maxDistance)
        {
            if (!npc.active || npc.friendly || npc.type == NPCID.TargetDummy || 
                npc.immortal || npc.dontTakeDamage)
                return false;

            float distance = Vector2.Distance(npc.Center, position);
            if (distance > maxDistance)
                return false;

            return true;
        }

        #region 目标锁定系统
        
        /// <summary>
        /// 获取当前锁定的目标
        /// </summary>
        private static NPC GetLockedTarget(Terraria.Projectile projectile)
        {
            if (_lockStates.TryGetValue(projectile.whoAmI, out ProjectileLockData lockData))
            {
                int targetId = lockData.LockedTargetId;
                if (targetId >= 0 && targetId < Main.maxNPCs && Main.npc[targetId].active)
                {
                    return Main.npc[targetId];
                }
                else
                {
                    ClearLockedTarget(projectile);
                }
            }
            return null;
        }
        
        private static void SetLockedTarget(Terraria.Projectile projectile, NPC target)
        {
            if (_lockStates.TryGetValue(projectile.whoAmI, out ProjectileLockData existingData))
            {
                _lockStates[projectile.whoAmI] = new ProjectileLockData(target.whoAmI, existingData.LockTimer);
            }
            else
            {
                _lockStates[projectile.whoAmI] = new ProjectileLockData(target.whoAmI, 0f);
            }
        }
        
        public static void ClearLockedTarget(Terraria.Projectile projectile)
        {
            if (_lockStates.ContainsKey(projectile.whoAmI))
            {
                _lockStates.Remove(projectile.whoAmI);
            }
        }
        
        public static void CleanupLockData()
        {
            var keysToRemove = new List<int>();
            
            foreach (var kvp in _lockStates)
            {
                int projIndex = kvp.Key;
                if (projIndex < 0 || projIndex >= Main.maxProjectiles || !Main.projectile[projIndex].active)
                {
                    keysToRemove.Add(projIndex);
                }
            }
            
            foreach (int key in keysToRemove)
            {
                _lockStates.Remove(key);
            }
        }
        
        public static void CleanupAllLockData()
        {
            _lockStates.Clear();
        }
        
        internal static void PeriodicCleanup()
        {
            _cleanupCounter++;
            if (_cleanupCounter >= CLEANUP_INTERVAL)
            {
                CleanupLockData();
                _cleanupCounter = 0;
            }
        }
        
        internal static int GetLockDataCount()
        {
            return _lockStates.Count;
        }
        
        #endregion

        #region 原有方法保留（兼容性）
        
        private static NPC FindNearestAerialTarget(Vector2 position, float maxDistance)
        {
            NPC nearestTarget = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && (npc.type != NPCID.TargetDummy) && !npc.immortal && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(npc.Center, position);
                    if (distance < minDistance && distance <= maxDistance)
                    {
                        minDistance = distance;
                        nearestTarget = npc;
                    }
                }
            }

            return nearestTarget;
        }

        private static float AngleBetween(Vector2 vector1, Vector2 vector2)
        {
            float angle1 = vector1.ToRotation();
            float angle2 = vector2.ToRotation();
            return Math.Abs(angle1 - angle2);
        }

        private static NPC FindNearestAerialTargetTowardsMouse(Vector2 position, float maxDistance, Vector2 mousePosition)
        {
            NPC nearestTarget = null;
            float minAngleDifference = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && (npc.type != NPCID.TargetDummy) && !npc.immortal && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(npc.Center, position);
                    if (distance <= maxDistance)
                    {
                        Vector2 directionToNpc = npc.Center - position;
                        Vector2 directionToMouse = mousePosition - position;
                        float angleDifference = AngleBetween(directionToNpc, directionToMouse);

                        if (angleDifference < minAngleDifference)
                        {
                            minAngleDifference = angleDifference;
                            nearestTarget = npc;
                        }
                    }
                }
            }

            return nearestTarget;
        }

        private static NPC FindNearestAerialTargetByHealth(Vector2 position, float maxDistance)
        {
            NPC nearestTarget = null;
            int maxHealth = int.MinValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && (npc.type != NPCID.TargetDummy) && !npc.immortal && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(npc.Center, position);
                    if (distance <= maxDistance && npc.lifeMax > maxHealth)
                    {
                        maxHealth = npc.lifeMax;
                        nearestTarget = npc;
                    }
                }
            }

            return nearestTarget;
        }
        
        #endregion

        /// <summary>
        /// 高级追踪方法 - 包含完整的Boss追踪优化
        /// </summary>
        public static void AdvancedHoming(Terraria.Projectile projectile, float speed, float maxTrackingDistance, 
                                        float turnResistance, Vector2? mousePosition = null, 
                                        bool bossPriority = true, float lockDuration = 180f)
        {
            // 检查是否需要重新选择目标
            if (ShouldRetarget(projectile, lockDuration))
            {
                ClearLockedTarget(projectile);
            }
            
            NPC target = FindOptimalTarget(projectile.Center, maxTrackingDistance, bossPriority, projectile, mousePosition);
            
            if (target != null)
            {
                float currentSpeed = projectile.velocity.Length();
                float targetAngle = projectile.AngleTo(target.Center);
                
                // 根据距离动态调整转向速度
                float distance = Vector2.Distance(projectile.Center, target.Center);
                float normalizedDistance = Math.Min(1f, distance / maxTrackingDistance);
                float turnSpeed = MathHelper.Lerp(MathHelper.ToRadians(5f), MathHelper.ToRadians(1f), normalizedDistance);
                
                projectile.velocity = projectile.velocity.ToRotation().AngleTowards(targetAngle, turnSpeed).ToRotationVector2() * currentSpeed;
                projectile.rotation = projectile.velocity.ToRotation();
                
                // 更新锁定计时器
                UpdateLockTimer(projectile);
            }
            else
            {
                projectile.rotation = projectile.velocity.ToRotation();
            }
        }

        /// <summary>
        /// 判断是否需要重新选择目标
        /// </summary>
        private static bool ShouldRetarget(Terraria.Projectile projectile, float lockDuration)
        {
            // 如果没有锁定目标，需要重新选择
            NPC lockedTarget = GetLockedTarget(projectile);
            if (lockedTarget == null)
                return true;
                
            // 如果锁定目标死亡或无效，需要重新选择
            if (!lockedTarget.active || lockedTarget.life <= 0)
                return true;
                
            // 如果锁定时间过长，允许重新选择
            if (_lockStates.TryGetValue(projectile.whoAmI, out ProjectileLockData lockData))
            {
                if (lockData.LockTimer >= lockDuration)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// 更新锁定计时器
        /// </summary>
        private static void UpdateLockTimer(Terraria.Projectile projectile)
        {
            if (_lockStates.TryGetValue(projectile.whoAmI, out ProjectileLockData lockData))
            {
                lockData.LockTimer += 1f;
                _lockStates[projectile.whoAmI] = lockData;
            }
        }
    }
    public class ExpansionKeleGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        public override void OnKill(Projectile projectile, int version)
        {
            ProjectileHelper.ClearLockedTarget(projectile);
        }
        
        public override void PostAI(Projectile projectile)
        {
            if (projectile.whoAmI == 0)
            {
                ProjectileHelper.PeriodicCleanup();
            }
        }
    }
}