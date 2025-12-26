using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.Chat;
using Terraria.ID;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 用于显示Boss战信息的工具类
    /// </summary>
    public static class BossCombatDisplay
    {
        /// <summary>
        /// 显示Boss战信息到聊天框
        /// </summary>
        /// <param name="bossNPCType">Boss的NPC类型ID</param>
        /// <param name="startTime">战斗开始时间（游戏更新计数）</param>
        /// <param name="trackedBoss">追踪的Boss NPC实例</param>
        /// <param name="playerCountAtStart">战斗开始时的玩家数量</param>
        /// <param name="resultType">战斗结果类型</param>
        public static void DisplayBossCombatInfo(int bossNPCType, uint startTime, NPC trackedBoss, int playerCountAtStart, CombatResultType resultType)
        {
            float healthPercent = 0f;
            if (trackedBoss != null && trackedBoss.active)
            {
                healthPercent = (float)trackedBoss.life / trackedBoss.lifeMax * 100;
            }
            
            float durationSeconds = (Main.GameUpdateCount - startTime) / 60f;

            string resultText = "";
            Color messageColor = Color.White;
            switch (resultType)
            {
                case CombatResultType.PlayerDefeat:
                    resultText = "玩家失败";
                    messageColor = Color.Red;
                    break;
                case CombatResultType.BossDefeated:
                    resultText = "Boss被击败";
                    messageColor = Color.Green;
                    break;
                case CombatResultType.BossDisappeared:
                    resultText = "Boss消失";
                    messageColor = Color.Yellow;
                    break;
            }

            string message = $"Boss: {Lang.GetNPCName(bossNPCType)}, 战斗时长: {durationSeconds:F2}秒, Boss剩余血量: {healthPercent:F2}%, 玩家人数: {playerCountAtStart}, 结果: {resultText}";
            
            // 在单人游戏中显示给本地玩家，在多人游戏中可能需要网络消息
            if (Main.netMode == NetmodeID.SinglePlayer) // 单人模式
            {
                Main.NewText(message, messageColor);
            }
            else // 多人模式
            {
                // 发送给所有玩家
                if (Main.netMode == NetmodeID.MultiplayerClient) // 客户端
                {
                    // 客户端不能直接发送全局消息，需要通过服务器
                    // 这里只是简单显示给本地客户端
                    Main.NewText(message, messageColor);
                }
                else if (Main.netMode == NetmodeID.Server) // 服务器
                {
                    // 在服务器上广播消息给所有玩家
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), messageColor);
                }
            }
        }
    }
}