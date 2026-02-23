using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    public class MentalOmegaDamageClass : DamageClass
    {
        public override string LocalizationCategory => "Others";
        
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            
            
            // 接受所有伤害加成和暴击率加成
            // 这意味着该伤害类型会从所有其他伤害类型获取加成
            return new StatInheritanceData(
                damageInheritance: 1f,
                critChanceInheritance: 1f,
                attackSpeedInheritance: 1f,
                armorPenInheritance: 1f,
                knockbackInheritance: 1f
            );
        }

        public override bool GetEffectInheritance(DamageClass damageClass)
        {
            // 继承所有伤害类型的效果
            return true;
        }

        public override void SetDefaultStats(Player player)
        {
            // 可以在这里设置默认统计信息
        }
    }
}