using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 协助效果层级数据类，存储每一层的效果信息
    /// </summary>
    public class AssistanceEffectLayer
    {
        /// <summary>
        /// Boss的NPC类型ID
        /// </summary>
        public int BossNPCType { get; set; }

        /// <summary>
        /// 层数
        /// </summary>
        public int StackCount { get; set; } = 1;

        /// <summary>
        /// 效果开始时间（游戏更新计数）
        /// </summary>
        public uint StartTime { get; set; }

        /// <summary>
        /// 效果持续时间（游戏更新计数）
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// 效果结束时间（游戏更新计数）
        /// </summary>
        public uint EndTime => StartTime + Duration;

        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        public float RemainingTimeSeconds => Math.Max(0, (EndTime - Main.GameUpdateCount) / 60f);

        /// <summary>
        /// 是否已冷却
        /// </summary>
        public bool IsExpired => Main.GameUpdateCount >= EndTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bossNPCType">关联的Boss类型</param>
        /// <param name="startTime">效果开始时间</param>
        /// <param name="duration">效果持续时间</param>
        public AssistanceEffectLayer(int bossNPCType, uint startTime, uint duration)
        {
            BossNPCType = bossNPCType;
            StartTime = startTime;
            Duration = duration;
        }
    }

    /// <summary>
    /// 管理协助效果的静态类
    /// </summary>
    public static class AssistanceEffectManager
    {
        /// <summary>
        /// 存储所有协助效果层级
        /// </summary>
        public static List<AssistanceEffectLayer> EffectLayers { get; private set; } = new List<AssistanceEffectLayer>();

        /// <summary>
        /// 根据战斗时长和Boss剩余血量计算效果持续时间（秒）
        /// 公式：战斗时长(秒)/360*300 + BOSS血量百分比*300
        /// </summary>
        /// <param name="combatDurationSeconds">战斗持续时间（秒）</param>
        /// <param name="bossHealthPercent">Boss剩余血量百分比</param>
        /// <param name="playerCount">参与战斗的玩家数量</param>
        /// <returns>效果持续时间（秒）</returns>
        public static float CalculateEffectDuration(float combatDurationSeconds, float bossHealthPercent, int playerCount)
        {
            // 基础公式：战斗时长(秒)/360*300 + BOSS血量百分比*300
            float baseDuration = combatDurationSeconds / 360f * 300f + bossHealthPercent * 300f;
            
            // 多人模式下平均分配时间
            if (playerCount > 1)
            {
                baseDuration /= playerCount;
            }

            return baseDuration;
        }

        /// <summary>
        /// 添加一个新的协助效果层级
        /// </summary>
        /// <param name="bossNPCType">关联的Boss类型</param>
        /// <param name="combatDurationSeconds">战斗持续时间（秒）</param>
        /// <param name="bossHealthPercent">Boss剩余血量百分比</param>
        /// <param name="playerCount">参与战斗的玩家数量</param>
        public static void AddEffectLayer(int bossNPCType, float combatDurationSeconds, float bossHealthPercent, int playerCount)
        {
            // 计算效果持续时间
            float durationSeconds = CalculateEffectDuration(combatDurationSeconds, bossHealthPercent, playerCount);
            
            // 转换为游戏更新计数（帧数）
            uint durationFrames = (uint)(durationSeconds * 60);
            
            // 创建新效果层级
            AssistanceEffectLayer effectLayer = new AssistanceEffectLayer(bossNPCType, Main.GameUpdateCount, durationFrames);
            
            // 添加到效果层级列表
            EffectLayers.Add(effectLayer);
            
            // 限制最多50个效果层级
            if (EffectLayers.Count > 50)
            {
                EffectLayers.RemoveAt(0);
            }
        }

        /// <summary>
        /// 更新所有效果层级，移除过期的效果层级
        /// </summary>
        public static void Update()
        {
            // 移除过期的效果层级
            EffectLayers.RemoveAll(e => e.IsExpired);
        }

        /// <summary>
        /// 获取指定Boss类型的激活效果层级总数
        /// </summary>
        /// <param name="bossNPCType">Boss的NPC类型ID</param>
        /// <returns>激活的效果层级数量</returns>
        public static int GetActiveEffectLayerCount(int bossNPCType)
        {
            int count = 0;
            foreach (var layer in EffectLayers)
            {
                if (layer.BossNPCType == bossNPCType && !layer.IsExpired)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 获取最新添加的效果层级的剩余时间（秒）
        /// </summary>
        /// <param name="bossNPCType">Boss的NPC类型ID</param>
        /// <returns>最新效果层级的剩余时间（秒）</returns>
        public static float GetLatestEffectRemainingTime(int bossNPCType)
        {
            AssistanceEffectLayer latestLayer = null;
            foreach (var layer in EffectLayers)
            {
                if (layer.BossNPCType == bossNPCType && !layer.IsExpired)
                {
                    if (latestLayer == null || layer.StartTime > latestLayer.StartTime)
                    {
                        latestLayer = layer;
                    }
                }
            }
            
            return latestLayer?.RemainingTimeSeconds ?? 0f;
        }

        /// <summary>
        /// 计算总伤害加成百分比（乘算），每层1%
        /// </summary>
        /// <returns>总伤害加成百分比</returns>
        public static float GetTotalDamageIncrease()
        {
            // 使用指数增长：1.01^层数
            return (float)Math.Pow(1.01, EffectLayers.Count);
        }

// ... existing code ...
        /// <summary>
        /// 计算总伤害减免百分比（乘算），每层1%
        /// </summary>
        /// <returns>总伤害减免百分比</returns>
        public static float GetTotalDamageReduction()
        {
            // 使用指数衰减：0.99^层数
            return (float)Math.Pow(0.99, EffectLayers.Count);
        }

    }

    /// <summary>
    /// 处理协助效果的ModPlayer类
    /// </summary>
    public class AssistanceEffectPlayer : ModPlayer
    {
        public override void PreUpdate()
        {
            // 更新协助效果（移除过期效果）
            if (Main.myPlayer == Player.whoAmI) // 只在本地玩家上执行
            {
                AssistanceEffectManager.Update();
            }
        }

        // 应用伤害加成
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            float increase = AssistanceEffectManager.GetTotalDamageIncrease();
            if (increase > 0)
            {
                damage *= increase; // 乘算加成
            }
        }

        // 应用伤害减免
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            float reduction = AssistanceEffectManager.GetTotalDamageReduction();
            if (reduction > 0)
            {
                modifiers.FinalDamage *= reduction; 
            }
        }
    }
}