using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Chat;
using Terraria.ID;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 战斗结果类型
    /// </summary>
    public enum CombatResultType
    {
        /// <summary>
        /// 玩家失败（所有玩家死亡）
        /// </summary>
        PlayerDefeat,
        
        /// <summary>
        /// Boss被击败
        /// </summary>
        BossDefeated,
        
        /// <summary>
        /// Boss消失（如传送或逃跑）
        /// </summary>
        BossDisappeared
    }

    /// <summary>
    /// 存储Boss战失败时的相关数据
    /// </summary>
    public class BossCombatData
    {
        /// <summary>
        /// Boss的NPC类型ID
        /// </summary>
        public int BossNPCType { get; set; }

        /// <summary>
        /// 战斗持续时间（帧）
        /// </summary>
        public uint CombatDurationFrames { get; set; }

        /// <summary>
        /// 战斗持续时间（秒）
        /// </summary>
        public float CombatDurationSeconds => CombatDurationFrames / 60f;

        /// <summary>
        /// Boss剩余血量比例（0-1之间）
        /// </summary>
        public float BossRemainingHealthPercent { get; set; }

        /// <summary>
        /// 参与战斗的玩家数量
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        /// 战斗结束时间（Unix时间戳）
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 战斗结果类型
        /// </summary>
        public CombatResultType ResultType { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bossNPCType">Boss的NPC类型ID</param>
        /// <param name="combatDurationFrames">战斗持续时间（帧）</param>
        /// <param name="bossRemainingHealthPercent">Boss剩余血量比例</param>
        /// <param name="playerCount">参与战斗的玩家数量</param>
        /// <param name="resultType">战斗结果类型</param>
        public BossCombatData(int bossNPCType, uint combatDurationFrames, float bossRemainingHealthPercent, int playerCount, CombatResultType resultType)
        {
            BossNPCType = bossNPCType;
            CombatDurationFrames = combatDurationFrames;
            BossRemainingHealthPercent = Math.Max(0f, Math.Min(1f, bossRemainingHealthPercent)); // 确保在0-1范围内
            PlayerCount = playerCount;
            ResultType = resultType;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 默认构造函数（用于序列化）
        /// </summary>
        public BossCombatData() { }
    }

    /// <summary>
    /// 管理Boss战数据的静态类
    /// </summary>
    public static class BossCombatDataManager
    {
        /// <summary>
        /// 存储所有的Boss战数据
        /// </summary>
        public static List<BossCombatData> CombatRecords { get; private set; } = new List<BossCombatData>();

        /// <summary>
        /// 添加一条新的Boss战记录
        /// </summary>
        /// <param name="data">Boss战数据</param>
        public static void AddCombatRecord(BossCombatData data)
        {
            CombatRecords.Add(data);
        }

        /// <summary>
        /// 清空所有记录
        /// </summary>
        public static void ClearRecords()
        {
            CombatRecords.Clear();
        }

        /// <summary>
        /// 获取特定Boss类型的战斗记录
        /// </summary>
        /// <param name="bossNPCType">Boss的NPC类型ID</param>
        /// <returns>该Boss的所有战斗记录</returns>
        public static List<BossCombatData> GetCombatRecordsForBoss(int bossNPCType)
        {
            return CombatRecords.FindAll(record => record.BossNPCType == bossNPCType);
        }
    }

    /// <summary>
    /// 用于监控Boss战并在各种情况下记录数据和显示信息的ModPlayer类
    /// </summary>
    public class BossCombatMonitor : ModPlayer
    {
        // 跟踪当前Boss战的信息
        private static int currentBossNPCType = -1;
        private static uint combatStartTime = 0;
        private static int playerCountAtStart = 0;
        private static bool inBossCombat = false;
        private static NPC trackedBoss = null;
        // 添加一个标志，防止重复记录战斗结果
        private static bool combatResultRecorded = false;

        public override void PreUpdate()
        {
            // 检查是否在Boss战中
            CheckBossCombatStatus();

            // 如果在Boss战中，检查是否所有玩家都死亡了
            if (inBossCombat && AreAllPlayersDead() && !combatResultRecorded)
            {
                // 记录Boss战失败数据
                RecordBossCombatData(CombatResultType.PlayerDefeat);

                // 显示Boss战信息
                BossCombatDisplay.DisplayBossCombatInfo(currentBossNPCType, combatStartTime, trackedBoss, playerCountAtStart, CombatResultType.PlayerDefeat);

                // 设置标志，防止重复记录
                combatResultRecorded = true;

                // 重置状态
                ResetCombatState();
            }
            
            // 如果在Boss战中但Boss已经不存在了（被击败或者消失了）
            if (inBossCombat && (trackedBoss == null || !trackedBoss.active) && !combatResultRecorded)
            {
                // 记录Boss消失数据
                RecordBossCombatData(CombatResultType.BossDisappeared);

                // 显示Boss战信息
                BossCombatDisplay.DisplayBossCombatInfo(currentBossNPCType, combatStartTime, trackedBoss, playerCountAtStart, CombatResultType.BossDisappeared);

                // 设置标志，防止重复记录
                combatResultRecorded = true;

                // 重置状态
                ResetCombatState();
            }
        }

        /// <summary>
        /// 当NPC受到伤害时调用此方法，用于检测Boss是否被击败
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="damage"></param>
        /// <param name="hitDirection"></param>
        /// <param name="crit"></param>
        public override void OnHitNPC(NPC npc, NPC.HitInfo hitInfo, int damageDone)
        {
            // 如果击中的NPC是当前追踪的Boss并且Boss被击败了
            if (inBossCombat && npc == trackedBoss && npc.life <= 0 && !combatResultRecorded)
            {
                // 记录Boss被击败数据
                RecordBossCombatData(CombatResultType.BossDefeated);

                // 显示Boss战信息
                BossCombatDisplay.DisplayBossCombatInfo(currentBossNPCType, combatStartTime, trackedBoss, playerCountAtStart, CombatResultType.BossDefeated);

                // 设置标志，防止重复记录
                combatResultRecorded = true;

                // 重置状态
                ResetCombatState();
            }
        }

        /// <summary>
        /// 检查Boss战状态
        /// </summary>
        private void CheckBossCombatStatus()
        {
            // 寻找Boss NPC
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.boss)
                {
                    // 如果之前不在Boss战中，现在进入Boss战
                    if (!inBossCombat)
                    {
                        inBossCombat = true;
                        currentBossNPCType = npc.type;
                        combatStartTime = Main.GameUpdateCount;
                        playerCountAtStart = CountActivePlayers();
                        trackedBoss = npc;
                        // 重新开始战斗时重置记录标志
                        combatResultRecorded = false;
                    }
                    return;
                }
            }

            // 如果没有找到Boss且之前在Boss战中，重置状态
            if (inBossCombat)
            {
                ResetCombatState();
            }
        }

        /// <summary>
        /// 检查是否所有玩家都已死亡
        /// </summary>
        /// <returns>如果所有玩家都已死亡返回true，否则返回false</returns>
        private bool AreAllPlayersDead()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 记录Boss战数据
        /// </summary>
        /// <param name="resultType">战斗结果类型</param>
        private void RecordBossCombatData(CombatResultType resultType)
        {
            float healthPercent = 0f;
            if (trackedBoss != null && trackedBoss.active)
            {
                healthPercent = (float)trackedBoss.life / trackedBoss.lifeMax;
            }
            
            uint duration = Main.GameUpdateCount - combatStartTime;

            BossCombatData data = new BossCombatData(
                currentBossNPCType,
                duration,
                healthPercent,
                playerCountAtStart,
                resultType
            );

            BossCombatDataManager.AddCombatRecord(data);
        }

        /// <summary>
        /// 重置战斗状态
        /// </summary>
        private void ResetCombatState()
        {
            inBossCombat = false;
            currentBossNPCType = -1;
            combatStartTime = 0;
            playerCountAtStart = 0;
            trackedBoss = null;
            // 重置记录标志
            combatResultRecorded = false;
        }

        /// <summary>
        /// 计算活跃玩家数量
        /// </summary>
        /// <returns>活跃玩家的数量</returns>
        private int CountActivePlayers()
        {
            int count = 0;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    count++;
                }
            }
            return count;
        }
    }
}