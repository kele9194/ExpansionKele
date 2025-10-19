using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Buff
{
    public class FullMoonBonusBuff : ModBuff
    {
        public override string LocalizationCategory=>"Buff";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            ExpansionKeleTool.AddDamageBonus(player, 0.06f); // 增加6%伤害
        }
    }
}