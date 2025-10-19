using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    public static class damageMultiplier
    {
        public static float GetDamageMultiplier(float mul ,StatModifier statModifier)
        {
            return mul * (1+statModifier.ApplyTo(1));
        }

    //     public override void Update(Player player, ref int buffIndex)
    // {
    //     // 计算魔法伤害加成
    //     float baseDamageIncrease = MagicSBData.DamageBonus100/100f;
    //     float additionalDamageIncrease = MagicSBData.DamageBonus100/100f * player.GetTotalDamage(DamageClass.Magic).ApplyTo(1);
    //     float totalDamageIncrease = baseDamageIncrease + additionalDamageIncrease;

    //     // 应用魔法伤害加成
    //     player.GetDamage(DamageClass.Magic) += totalDamageIncrease;
    // }
    }
    public static class PlayerUtils
    {
        /// <summary>
        /// 减少玩家的魔力病持续时间
        /// </summary>
        /// <param name="player">要减少魔力病持续时间的玩家</param>
        public static void ReduceManaSicknessDuration(Player player)
        {
            int buffIndex = player.FindBuffIndex(BuffID.ManaSickness);
            if (buffIndex != -1)
            {
                // 每帧减少 1 帧（≈ 0.0167 秒）持续时间
                if (Main.rand.NextDouble() < 0.5f)
                {
                    player.buffTime[buffIndex] -= 1;
                }
                //Main.NewText( "Mana Sickness reduced by 3 frame.", new Color(255, 0, 0));

                // 防止负值
                if (player.buffTime[buffIndex] < 0)
                {
                    player.buffTime[buffIndex] = 0;
                }
            }
        }
        
    }
}