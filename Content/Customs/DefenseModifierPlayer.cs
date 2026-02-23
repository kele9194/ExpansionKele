using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 防御修改器玩家类
    /// 提供灵活的防御力修改系统，支持加法和乘法两种修改方式
    /// </summary>
    public class DefenseModifierPlayer : ModPlayer
    {
        /// <summary>
        /// 基础防御加成（加法）
        /// 在原有防御基础上直接增加或减少固定数值
        /// </summary>
        public int additiveDefenseBonus = 0;
        
        /// <summary>
        /// 防御乘数（乘法）
        /// 对最终防御力进行倍率调整，默认值为1.0f（无变化）
        /// </summary>
        public float defenseMultiplier = 1.0f;
        
        /// <summary>
        /// 标记是否启用了加法防御加成
        /// 用于优化性能，避免不必要的计算
        /// </summary>
        public bool hasAdditiveDefenseBonus = false;
        
        /// <summary>
        /// 标记是否启用了防御乘数
        /// 用于优化性能，避免不必要的计算
        /// </summary>
        public bool hasDefenseMultiplier = false;

        /// <summary>
        /// 重置每帧效果
        /// 在每一游戏帧开始时调用，将所有防御修改重置为默认值
        /// 这确保了防御修改是临时性的，只在装备或效果激活时生效
        /// </summary>
        public override void ResetEffects()
        {
            // 每帧重置防御修改为默认值
            additiveDefenseBonus = 0;
            defenseMultiplier = 1.0f;
            hasAdditiveDefenseBonus = false;
            hasDefenseMultiplier = false;
        }

        /// <summary>
        /// 装备更新后处理防御修改
        /// 在装备效果处理完成后调用，应用所有累积的防御修改
        /// 修改顺序：先应用加法加成，再应用乘法倍率
        /// </summary>
        public override void PostUpdateEquips()
        {
            // 应用防御加成（加法）
            if (hasAdditiveDefenseBonus)
            {
                Player.statDefense += additiveDefenseBonus;
            }
            
            // 应用防御乘数（乘法）
            if (hasDefenseMultiplier && defenseMultiplier != 1.0f)
            {
                Player.statDefense *= defenseMultiplier;
            }
            
            // 确保防御不会变为负数（通过隐式转换为int来检查）
            // 当防御值小于0时，将其设置为0
            if ((int)Player.statDefense < 0)
            {
                Player.statDefense *= 0;
            }
        }
    }

    /// <summary>
    /// 防御修改器静态工具类
    /// 提供便捷的方法来修改玩家防御力
    /// 支持加法和乘法两种修改模式
    /// </summary>
    public static class DefenseModifier
    {
        /// <summary>
        /// 给玩家添加固定数值的防御力（加法叠加）
        /// 可以多次调用此方法，每次的数值会累加到总加成中
        /// </summary>
        /// <param name="player">目标玩家实例</param>
        /// <param name="defenseBonus">要添加的防御力值（可正可负）</param>
        /// <example>
        /// // 增加10点防御力
        /// DefenseModifier.AddDefense(player, 10);
        /// // 减少5点防御力
        /// DefenseModifier.AddDefense(player, -5);
        /// </example>
        public static void AddDefense(Player player, int defenseBonus)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasAdditiveDefenseBonus = true;
            defensePlayer.additiveDefenseBonus += defenseBonus;
        }

        /// <summary>
        /// 设置玩家防御力增加值（完全替换模式）
        /// 此方法会覆盖之前的所有加法加成，设置一个全新的固定值
        /// </summary>
        /// <param name="player">目标玩家实例</param>
        /// <param name="defenseBonus">新的防御力增加值</param>
        /// <example>
        /// // 直接设置防御力增加20点（覆盖之前的任何加成）
        /// DefenseModifier.SetAdditiveDefense(player, 20);
        /// </example>
        public static void SetAdditiveDefense(Player player, int defenseBonus)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasAdditiveDefenseBonus = true;
            defensePlayer.additiveDefenseBonus = defenseBonus;
        }

        /// <summary>
        /// 给玩家防御力应用乘数（倍率叠加）
        /// 可以多次调用此方法，每次的倍率会与现有倍率相乘
        /// </summary>
        /// <param name="player">目标玩家实例</param>
        /// <param name="multiplier">防御力乘数（大于1增加防御，小于1降低防御）</param>
        /// <example>
        /// // 增加50%防御力
        /// DefenseModifier.MultiplyDefense(player, 1.5f);
        /// // 降低25%防御力
        /// DefenseModifier.MultiplyDefense(player, 0.75f);
        /// </example>
        public static void MultiplyDefense(Player player, float multiplier)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasDefenseMultiplier = true;
            defensePlayer.defenseMultiplier *= multiplier;
        }

        /// <summary>
        /// 设置玩家防御力乘数（完全替换模式）
        /// 此方法会覆盖之前的所有乘数效果，设置一个全新的倍率值
        /// </summary>
        /// <param name="player">目标玩家实例</param>
        /// <param name="multiplier">新的防御力乘数</param>
        /// <example>
        /// // 直接设置防御力为原来的1.2倍（覆盖之前的任何乘数）
        /// DefenseModifier.SetDefenseMultiplier(player, 1.2f);
        /// </example>
        public static void SetDefenseMultiplier(Player player, float multiplier)
        {
            var defensePlayer = player.GetModPlayer<DefenseModifierPlayer>();
            defensePlayer.hasDefenseMultiplier = true;
            defensePlayer.defenseMultiplier = multiplier;
        }
    }
}