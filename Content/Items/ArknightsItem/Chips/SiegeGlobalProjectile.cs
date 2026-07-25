using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 攻城手（Siege）天赋的弹幕处理类。
    /// 效果：
    /// 1. 体积判定：自动锁定范围内碰撞箱最大的敌人
    /// 2. 距离机制：64px内 ×0.5 近身惩罚；400px以上 ×1.12 远程增伤
    /// 3. 轻微追踪：对锁定目标每帧转向 ≤ 0.05 弧度（可开关）
    /// </summary>
    public class SiegeGlobalProjectile : GlobalProjectile
    {
        // 记录每个弹幕的来源武器和玩家（key = projectile.whoAmI）
        private static readonly Dictionary<int, (Item item, Player player)> projectileSourceMap
            = new Dictionary<int, (Item, Player)>();

        // 搜索最大体积目标的半径
        private const float SearchRange = 700f;
        private const float SearchRangeSq = SearchRange * SearchRange;



        // 追踪范围
        private const float HomingRange = 600f;
        private const float HomingRangeSq = HomingRange * HomingRange;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                Item sourceItem = statsSource.Item;
                Player owner = statsSource.Player;

                projectileSourceMap[projectile.whoAmI] = (sourceItem, owner);

                // 快速检查：如果不是攻城手，直接移除记录
                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                if (arkNum == 0) return;
                if ((SniperSubclass)arkNum != SniperSubclass.Siege)
                {
                    projectileSourceMap.Remove(projectile.whoAmI);
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            if (!TryGetSiegeInfo(projectile, out Item sourceItem, out _))
                return;

            // ---- 效果3：轻微追踪（制导开关控制） ----
            if (!sourceItem.GetGlobalItem<ArknightsGlobalItem>().isHoming)
                return;

            // 找到范围内体积最大的敌人作为追踪目标
            NPC target = FindLargestTarget(projectile.Center, HomingRange);
            if (target == null)
                return;

            Vector2 targetDir = target.Center - projectile.Center;
            float distToTarget = targetDir.Length();
            if (distToTarget <= 0f)
                return;

            targetDir.Normalize();
            Vector2 currentDir = projectile.velocity.SafeNormalize(Vector2.Zero);
            if (currentDir == Vector2.Zero)
                return;

            // ✅ Lerp 模式：每帧向目标方向插值 3%，然后归一化保持速度恒定
            Vector2 newDir = Vector2.Lerp(currentDir, targetDir, 0.03f);
            newDir.Normalize();
            projectile.velocity = newDir * projectile.velocity.Length();
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!TryGetSiegeInfo(projectile, out _, out Player owner))
                return;

            // ---- 效果2：距离机制 ----
            float dist = Vector2.Distance(owner.Center, target.Center);

            if (dist <= 64f)
            {
                modifiers.FinalDamage *= 0.5f;
            }
            else if (dist >= 400f)
            {
                modifiers.FinalDamage *= 1.2f;
            }

            // ---- 效果3：对最大体积敌人额外增伤 ×1.2 ----  ← 新增
            NPC largestTarget = FindLargestTarget(projectile.Center, SearchRange);
            if (largestTarget != null && largestTarget.whoAmI == target.whoAmI)
            {
                modifiers.FinalDamage *= 1.2f;
            }
        }

        public override void OnKill(Projectile projectile, int timeleft)
        {
            projectileSourceMap.Remove(projectile.whoAmI);
        }

        public override void Unload()
        {
            projectileSourceMap.Clear();
        }

        /// <summary>
        /// 尝试获取弹幕的攻城手刻印信息。如果任何校验失败，返回 false。
        /// </summary>
        private static bool TryGetSiegeInfo(Projectile projectile, out Item sourceItem, out Player owner)
        {
            sourceItem = null;
            owner = null;

            if (projectile == null || !projectile.active)
                return false;

            if (!projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo))
                return false;

            sourceItem = sourceInfo.item;
            if (sourceItem == null || sourceItem.IsAir)
                return false;

            owner = sourceInfo.player ?? Main.player[projectile.owner];
            if (owner == null || !owner.active)
                return false;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
            if (arkNum == 0)
                return false;

            if ((SniperSubclass)arkNum != SniperSubclass.Siege)
                return false;

            return true;
        }

        /// <summary>
        /// 在指定范围内找到碰撞箱（width × height）最大的 NPC。
        /// 如果多个 NPC 体积相同，选择距离最近的。
        /// </summary>
        private static NPC FindLargestTarget(Vector2 center, float range)
        {
            NPC best = null;
            int bestArea = 0;
            float bestDist = float.MaxValue;
            float rangeSq = range * range;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.life <= 0)
                    continue;

                // 距离检查
                float distSq = Vector2.DistanceSquared(center, npc.Center);
                if (distSq > rangeSq)
                    continue;

                int area = npc.width * npc.height;

                // 体积更大 → 优先选择
                if (area > bestArea)
                {
                    bestArea = area;
                    bestDist = distSq;
                    best = npc;
                }
                // 体积相同 → 选距离最近的
                else if (area == bestArea && area > 0 && distSq < bestDist)
                {
                    bestDist = distSq;
                    best = npc;
                }
            }

            return best;
        }
    }

    /// <summary>
    /// 攻城手（Siege）天赋的玩家效果处理类。
    /// 当前无额外玩家效果，保留为扩展预留。
    /// </summary>
    public class SiegePlayer : ModPlayer
    {
        /// <summary>玩家当前手持的武器是否为攻城手刻印</summary>
        public bool HasSiegeWeaponEquipped = false;

        public override void ResetEffects()
        {
            HasSiegeWeaponEquipped = false;
        }

        public override void PostUpdate()
        {
            Item heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
            {
                HasSiegeWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(heldItem);
            if (arkNum == 0)
            {
                HasSiegeWeaponEquipped = false;
                return;
            }

            HasSiegeWeaponEquipped = (SniperSubclass)arkNum == SniperSubclass.Siege;
        }
    }
}