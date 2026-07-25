using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 神射手（Marksman）天赋的弹幕处理类。
    /// 效果：
    /// 1. 对防御最低的敌人 → 伤害 ×1.06
    /// 2. 轻微追踪 → 向防御最低的敌人转向（可开关）
    /// 3. 未击杀补偿 → 射弹未能击败敌人时，攻速 ×1.1 buff，持续2秒
    /// 4. 距离增伤 → 400px 开始增伤，640px 达到 ×1.15
    /// </summary>
    public class MarksmanGlobalProjectile : GlobalProjectile
    {
        // 记录每个弹幕的来源武器和玩家（key = projectile.whoAmI）
        private static readonly Dictionary<int, (Item item, Player player)> projectileSourceMap
            = new Dictionary<int, (Item, Player)>();

        // 记录每个弹幕是否击杀过 NPC
        private static readonly HashSet<int> projectileKilledNpc = new HashSet<int>();

        // 最大追踪范围
        private const float HomingRange = 600f;

        // 扫描 NPC 的最大范围（用于寻找最低防御目标）
        private const float ScanRange = 700f;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                Item sourceItem = statsSource.Item;
                Player owner = statsSource.Player;

                projectileSourceMap[projectile.whoAmI] = (sourceItem, owner);

                // 快速检查：如果不是神射手，直接移除记录以减少内存占用
                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                // 非神射手弹幕 → 直接移除记录，不参与后续处理
                if (arkNum == 0 || (SniperSubclass)arkNum != SniperSubclass.Marksman)
                {
                    projectileSourceMap.Remove(projectile.whoAmI);
                }
            }
        }

        public override void AI(Projectile projectile)
        {
            if (!TryGetMarksmanInfo(projectile, out Item sourceItem, out _))
                return;

            // 检查制导开关（isHoming）
            if (!sourceItem.GetGlobalItem<ArknightsGlobalItem>().isHoming)
                return;

            // 寻找范围内防御最低的敌人进行轻微追踪
            NPC target = FindLowestDefenseTarget(projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 targetDir = target.Center - projectile.Center;
                targetDir.Normalize();
                Vector2 currentDir = projectile.velocity.SafeNormalize(Vector2.Zero);

                // 轻微追踪：插值系数 0.03，比速射狙的 0.05 更轻微
                Vector2 newDir = Vector2.Lerp(currentDir, targetDir, 0.03f);
                newDir.Normalize();
                projectile.velocity = newDir * projectile.velocity.Length();
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!TryGetMarksmanInfo(projectile, out Item sourceItem, out Player owner))
                return;

            // ---- 效果1：对防御最低的敌人 ×1.06 ----
            NPC lowestDef = FindLowestDefenseTarget(projectile.Center, ScanRange);
            if (lowestDef != null && lowestDef.whoAmI == target.whoAmI)
            {
                modifiers.FinalDamage *= 1.06f;
            }

            // ---- 效果4：距离增伤 ----
            // 用玩家到目标的距离计算
            float distToTarget = Vector2.Distance(owner.Center, target.Center);
            if (distToTarget > 400f)
            {
                // 400px ~ 640px 线性插值 1.0 → 1.15
                float t = MathF.Min((distToTarget - 400f) / 240f, 1f);
                float distanceMultiplier = 1f + t * 0.15f;
                modifiers.FinalDamage *= distanceMultiplier;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!TryGetMarksmanInfo(projectile, out _, out _))
                return;

            // 检查这次攻击是否击杀了 NPC
            // 用 target.active 和 target.life 判断死亡
            if (!target.active || target.life <= 0)
            {
                projectileKilledNpc.Add(projectile.whoAmI);
            }
        }

                public override void OnKill(Projectile projectile, int timeleft)
        {
            // ---- 效果3：未击杀补偿 ----
            // 先从字典中取出记录，再清理，避免重复获取
            if (projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo))
            {
                projectileSourceMap.Remove(projectile.whoAmI);

                // 防御性校验：确认来源物品确实是神射手刻印
                Item sourceItem = sourceInfo.item;
                if (sourceItem != null && !sourceItem.IsAir)
                {
                    int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                    if (arkNum != 0 && (SniperSubclass)arkNum == SniperSubclass.Marksman)
                    {
                        // 射弹未能击败任何敌人 → 给予攻速 buff
                        if (!projectileKilledNpc.Contains(projectile.whoAmI))
                        {
                            Player owner = sourceInfo.player ?? Main.player[projectile.owner];
                            if (owner != null && owner.active)
                            {
                                // 添加 MarksmanBuff，持续 2 秒（120 ticks）
                                owner.AddBuff(ModContent.BuffType<MarksmanBuff>(), 120);
                            }
                        }
                    }
                }
            }

            // 清理记录
            projectileKilledNpc.Remove(projectile.whoAmI);
        }

        /// <summary>
        /// 尝试获取弹幕的神射手刻印信息。
        /// 如果任何校验失败，返回 false。
        /// </summary>
        private static bool TryGetMarksmanInfo(Projectile projectile, out Item sourceItem, out Player owner)
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

            if ((SniperSubclass)arkNum != SniperSubclass.Marksman)
                return false;

            return true;
        }

        /// <summary>
        /// 在指定范围内寻找防御力最低的敌人。
        /// </summary>
        private static NPC FindLowestDefenseTarget(Vector2 center, float range)
        {
            NPC best = null;
            float lowestDefense = float.MaxValue;
            float rangeSq = range * range;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.life <= 0)
                    continue;

                // 距离检查（用平方距离避免开方）
                float distSq = Vector2.DistanceSquared(center, npc.Center);
                if (distSq > rangeSq)
                    continue;

                // 比较防御力（用 defense 属性）
                if (npc.defense < lowestDefense)
                {
                    lowestDefense = npc.defense;
                    best = npc;
                }
            }

            return best;
        }

        public override void Unload()
        {
            projectileSourceMap.Clear();
            projectileKilledNpc.Clear();
        }
    }
}