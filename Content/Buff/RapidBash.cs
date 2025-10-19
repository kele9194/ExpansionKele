using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Localization; // 添加这行以引入 Microsoft.Xna.Framework.Color

namespace ExpansionKele.Content.Buff
{
    public class RapidBash : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public static float buffBonus=0.10f;
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Rapid Bash");
            //Description.SetDefault("Increases melee attack speed by 10%");
            Main.buffNoTimeDisplay[Type] = false; // 是否显示时间
            Main.debuff[Type] = false; // 是否为负面效果
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            tip = Language.GetText("Mods.ExpansionKele.Buff.RapidBash.Description").Format(buffBonus * 100);
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 对 NPC 的 buff 更新，这里不需要处理
        }
        

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed<MeleeDamageClass>() += buffBonus; // 增加 10% 的近战攻速
        }
    }
}