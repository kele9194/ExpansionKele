using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    public class DefenseModifierPlayer : ModPlayer
    {
        // 基础防御加成（加法）
        public int additiveDefenseBonus = 0;
        
        // 防御乘数（乘法）
        public float defenseMultiplier = 1.0f;
        
        // 标记是否启用自定义防御修改
        public bool hasAdditiveDefenseBonus = false;
        public bool hasDefenseMultiplier = false;

        public override void ResetEffects()
        {
            // 每帧重置防御修改为默认值
            additiveDefenseBonus = 0;
            defenseMultiplier = 1.0f;
            hasAdditiveDefenseBonus = false;
            hasDefenseMultiplier = false;
        }

        public override void PostUpdateEquips()
        {
            // 应用防御加成
            if (hasAdditiveDefenseBonus)
            {
                Player.statDefense += additiveDefenseBonus;
            }
            
            // 应用防御乘数
            if (hasDefenseMultiplier && defenseMultiplier != 1.0f)
            {
                Player.statDefense *= defenseMultiplier;
            }
            
            // 确保防御不会变为负数（通过隐式转换为int来检查）
            if ((int)Player.statDefense < 0)
            {
                Player.statDefense *= 0;
            }
        }
    }

    public static class DefenseModifier
    {
        /// <summary>
        /// 给玩家添加固定数值的防御力（加法）
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="defenseBonus">要添加的防御力值</param>
        public static void AddDefense(Player player, int defenseBonus)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasAdditiveDefenseBonus = true;
            defensePlayer.additiveDefenseBonus += defenseBonus;
        }

        /// <summary>
        /// 设置玩家防御力增加值（替换之前的加法加成）
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="defenseBonus">防御力增加值</param>
        public static void SetAdditiveDefense(Player player, int defenseBonus)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasAdditiveDefenseBonus = true;
            defensePlayer.additiveDefenseBonus = defenseBonus;
        }

        /// <summary>
        /// 给玩家防御力应用乘数
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="multiplier">防御力乘数</param>
        public static void MultiplyDefense(Player player, float multiplier)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasDefenseMultiplier = true;
            defensePlayer.defenseMultiplier *= multiplier;
        }

        /// <summary>
        /// 设置玩家防御力乘数（替换之前的乘数）
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="multiplier">防御力乘数</param>
        public static void SetDefenseMultiplier(Player player, float multiplier)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasDefenseMultiplier = true;
            defensePlayer.defenseMultiplier = multiplier;
        }
    }
}