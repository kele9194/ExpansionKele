using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 召唤栏位计算器 - 用于计算玩家的召唤栏使用情况
    /// </summary>
    public static class MinionSlotCalculator
    {
        /// <summary>
        /// 计算玩家当前空余的召唤栏位数量
        /// </summary>
        /// <param name="player">要计算的玩家</param>
        /// <returns>空余的召唤栏位数量</returns>
        public static float CalculateAvailableMinionSlots(Player player)
        {
            float minionSlotsUsed = 0f;
            
            // 遍历所有弹幕，统计当前玩家已使用的召唤槽数量
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && 
                    !Main.projectile[i].hostile && 
                    Main.projectile[i].owner == player.whoAmI && 
                    Main.projectile[i].minionSlots > 0f)
                {
                    minionSlotsUsed += Main.projectile[i].minionSlots;
                }
            }
            
            // 计算剩余可用的召唤槽位数
            float availableSlots = player.maxMinions - minionSlotsUsed;
            
            // 确保返回值不小于0
            return availableSlots < 0f ? 0f : availableSlots;
        }

        /// <summary>
        /// 计算玩家当前已使用的召唤栏位数量
        /// </summary>
        /// <param name="player">要计算的玩家</param>
        /// <returns>已使用的召唤栏位数量</returns>
        public static float CalculateUsedMinionSlots(Player player)
        {
            float minionSlotsUsed = 0f;
            
            // 遍历所有弹幕，统计当前玩家已使用的召唤槽数量
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && 
                    !Main.projectile[i].hostile && 
                    Main.projectile[i].owner == player.whoAmI && 
                    Main.projectile[i].minionSlots > 0f)
                {
                    minionSlotsUsed += Main.projectile[i].minionSlots;
                }
            }
            
            return minionSlotsUsed;
        }

        /// <summary>
        /// 检查玩家是否有足够的空余召唤栏位
        /// </summary>
        /// <param name="player">要检查的玩家</param>
        /// <param name="requiredSlots">需要的召唤栏位数量</param>
        /// <returns>是否有足够的空余栏位</returns>
        public static bool HasEnoughMinionSlots(Player player, float requiredSlots)
        {
            return CalculateAvailableMinionSlots(player) >= requiredSlots;
        }

        /// <summary>
        /// 获取玩家召唤栏使用率（0-1之间的数值）
        /// </summary>
        /// <param name="player">要计算的玩家</param>
        /// <returns>召唤栏使用率</returns>
        public static float GetMinionSlotUsageRatio(Player player)
        {
            if (player.maxMinions <= 0)
                return 0f;
                
            float usedSlots = CalculateUsedMinionSlots(player);
            return usedSlots / player.maxMinions;
        }

        /// <summary>
        /// 计算基于空余召唤栏位的伤害加成系数
        /// </summary>
        /// <param name="player">要计算的玩家</param>
        /// <param name="baseMultiplier">基础乘数</param>
        /// <param name="perSlotBonus">每个空余栏位的加成</param>
        /// <returns>最终的伤害加成系数</returns>
        public static float CalculateSlotBasedDamageMultiplier(Player player, float baseMultiplier = 1f, float perSlotBonus = 0.1f)
        {
            float availableSlots = CalculateAvailableMinionSlots(player);
            return baseMultiplier + (availableSlots * perSlotBonus);
        }
    }
}