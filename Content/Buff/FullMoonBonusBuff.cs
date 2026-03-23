using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.Localization;

namespace ExpansionKele.Content.Buff
{
    public class FullMoonBonusBuff : ModBuff
    {
        public static float buffbonus=6;
        public override string LocalizationCategory=>"Buff";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Language.GetText("Mods.ExpansionKele.Buff.FullMoonBonusBuff.Description").Format(buffbonus);
    }

        public override void Update(Player player, ref int buffIndex)
        {
            ExpansionKeleTool.MultiplyDamageBonus(player, 1f+buffbonus/100f); // 增加6%伤害
        }
    }
}