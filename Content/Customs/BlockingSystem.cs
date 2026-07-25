using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 阻挡机制核心静态类。
    /// 负责计算阻挡名单（Blocking List）和阻挡效果系数（Blocking Effects）。
    /// 
    /// 核心规则：
    /// - 阻挡名单：从玩家周围 500px 内选出距离最近的 N 个敌人（N = 阻挡上限）
    /// - 阻挡效果：敌我距离越近效果越强，0~100px 满效，100~300px 线性衰减，>300px 无效
    /// 
    /// 挡位与效果对照：
    ///   挡1（无畏者等）：+20%增伤，-10%减伤
    ///   挡2（强攻手等）：+15%增伤，-15%减伤
    ///   挡3（重装铁卫）：+8% 增伤，-25%减伤
    ///   挡4（守护者等）：+0% 增伤，-35%减伤
    /// </summary>
    public static class BlockingSystem
    {
        // ======================== 常量定义 ========================

        /// <summary>观察圈范围（px）- 在此范围内的敌人才会被纳入阻挡名单候选</summary>
        public const float BLOCKING_RANGE = 500f;

        /// <summary>满效范围（px）- 在此距离内阻挡效果 100% 生效</summary>
        public const float EFFECT_FULL_RANGE = 100f;

        /// <summary>效果边界（px）- 超过此距离阻挡效果完全消失</summary>
        public const float EFFECT_MAX_RANGE = 300f;

        /// <summary>衰减区间长度（px）- 用于线性插值计算</summary>
        public const float EFFECT_DECAY_RANGE = EFFECT_MAX_RANGE - EFFECT_FULL_RANGE;

        // ======================== 阻挡名单计算 ========================

        /// <summary>
        /// 获取指定玩家的当前阻挡名单。
        /// 每帧调用，从玩家周围 500px 内选出距离最近的 N 个活着的敌人。
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="blockingCap">阻挡上限（N）</param>
        /// <returns>按距离排序的阻挡名单（最近的在最前）</returns>
        public static List<NPC> GetBlockingList(Player player, int blockingCap)
        {
            var result = new List<NPC>();
            if (blockingCap <= 0) return result;

            var candidates = new List<(NPC npc, float distSq)>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.chaseable || npc.friendly || npc.dontTakeDamage)
                    continue;

                float distSq = Vector2.DistanceSquared(player.Center, npc.Center);
                if (distSq <= BLOCKING_RANGE * BLOCKING_RANGE)
                {
                    candidates.Add((npc, distSq));
                }
            }

            // 按距离升序排列（最近的在最前）
            candidates.Sort((a, b) => a.distSq.CompareTo(b.distSq));

            int count = Math.Min(blockingCap, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                result.Add(candidates[i].npc);
            }

            return result;
        }

        // ======================== 效果系数计算 ========================

        /// <summary>
        /// 根据距离计算阻挡效果系数（0.0 ~ 1.0）。
        /// </summary>
        /// <param name="distance">玩家与目标之间的距离（px）</param>
        /// <returns>效果系数：0~100px=1.0，100~300px 线性衰减到0，>300px=0</returns>
        public static float GetEffectFactor(float distance)
        {
            if (distance <= EFFECT_FULL_RANGE) return 1f;
            if (distance >= EFFECT_MAX_RANGE) return 0f;
            // 线性衰减：(300 - distance) / (300 - 100)
            return (EFFECT_MAX_RANGE - distance) / EFFECT_DECAY_RANGE;
        }

        // ======================== 挡位效果查询 ========================

        /// <summary>
        /// 根据阻挡上限（挡位）获取最大增伤百分比。
        /// </summary>
        public static float GetMaxDamageBonus(int blockingCap)
        {
            return blockingCap switch
            {
                1 => 0.25f,  // 挡1：+20%
                2 => 0.15f,  // 挡2：+15%
                3 => 0.1f,  // 挡3：+8%
                4 => 0f,     // 挡4：+0%
                _ => 0f
            };
        }

        /// <summary>
        /// 根据阻挡上限（挡位）获取最大减伤百分比。
        /// </summary>
        public static float GetMaxDamageReduction(int blockingCap)
        {
            return blockingCap switch
            {
                1 => 0.10f,  // 挡1：-10%
                2 => 0.2f,  // 挡2：-15%
                3 => 0.25f,  // 挡3：-25%
                4 => 0.35f,  // 挡4：-35%
                _ => 0f
            };
        }

        /// <summary>
        /// 获取子职业对应的阻挡上限。
        /// </summary>
        public static int GetBlockingCap(GuardSubclass subclass)
        {
            return subclass switch
            {
                GuardSubclass.Centurion    => 2,   // 强攻手：挡2
                GuardSubclass.Dreadnought  => 1,   // 无畏者：挡1
                GuardSubclass.Fighter      => 1,   // 斗士：挡1
                GuardSubclass.ArtsFighter  => 1,   // 术战者：挡1
                GuardSubclass.Lord         => 2,   // 领主：挡2
                GuardSubclass.Swordmaster  => 1,   // 剑豪：挡1
                GuardSubclass.Musha        => 1,   // 武者：挡1
                GuardSubclass.Reaper       => 2,   // 收割者：挡2
                GuardSubclass.Instructor   => 2,   // 教官：挡2
                GuardSubclass.Liberator    => 1,   // 解放者：挡1
                GuardSubclass.Crusher      => 2,   // 重剑手：挡2
                GuardSubclass.Earthshaker  => 1,   // 撼地者：挡1
                GuardSubclass.PrimalGuard  => 1,   // 本源近卫：挡1
                GuardSubclass.Mercenary    => 1,   // 佣兵：挡1
                _                          => 0
            };
        }

        // ======================== 团队统合阻挡（团队名单） ========================

        /// <summary>团队阻挡集合：team → (npc.whoAmI 集合），仅用于其他机制查询</summary>
        private static Dictionary<int, HashSet<int>> _teamBlockingSets = new();

        /// <summary>
        /// 由 BlockingTeamSystem.PostUpdatePlayers() 调用。
        /// 收集各队所有玩家的个人阻挡名单，取并集。
        /// </summary>
        public static void RebuildTeamData()
        {
            _teamBlockingSets.Clear();

            var tempSets = new Dictionary<int, HashSet<int>>();

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead) continue;

                int team = p.team;
                var bp = p.GetModPlayer<BlockingPlayer>();

                if (!tempSets.ContainsKey(team))
                    tempSets[team] = new HashSet<int>();

                foreach (var npc in bp.CurrentBlockingList)
                {
                    if (npc.active)
                        tempSets[team].Add(npc.whoAmI);
                }
            }

            _teamBlockingSets = tempSets;
        }

        /// <summary>
        /// 检查目标 NPC 是否被该玩家所在队伍的任意队友阻挡。
        /// </summary>
        public static bool IsInTeamBlockingSet(NPC npc, int team)
        {
            return _teamBlockingSets.TryGetValue(team, out var set) && set.Contains(npc.whoAmI);
        }
    }

    /// <summary>
    /// 负责每帧在所有玩家更新完毕后，统合各队的阻挡数据。
    /// </summary>
    public class BlockingTeamSystem : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            // 在所有玩家的 PostUpdate() 执行完毕后重建团队数据
            BlockingSystem.RebuildTeamData();
        }
    }
}