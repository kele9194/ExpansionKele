using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Buff
{
    public class RedemptionAttackBuff : ModBuff
    {
        public override string LocalizationCategory=> "Buff";
        public static int maxStacks = 10;
        
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Buff存在时应用效果
            var modPlayer = player.GetModPlayer<RedemptionAttackPlayer>();
            
            // 应用乘算增伤和防御前减伤 (每层1%)
            var damagePlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            
            damagePlayer.AddMultiplicativeDamageBonus(modPlayer.redemptionAttackStacks * 0.01f);
            reductionPlayer.AddPreDefenseDamageReduction(modPlayer.redemptionAttackStacks * 0.01f);
        }
    }
    
     public class RedemptionAttackPlayer : ModPlayer
    {
        public int redemptionAttackStacks = 0;
        
        public override void ResetEffects()
        {
            // 检查玩家是否还有RedemptionAttackBuff
            bool hasBuff = Player.HasBuff(ModContent.BuffType<RedemptionAttackBuff>());
            
            // 如果没有buff，重置层数
            if (!hasBuff)
            {
                redemptionAttackStacks = 0;
            }
        }
    }
}