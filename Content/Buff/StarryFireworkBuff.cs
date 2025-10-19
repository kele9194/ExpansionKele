using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Accessories;
using Terraria.Localization;

namespace ExpansionKele.Content.Buff
{ 

public class StarryFireworkBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 标记为减益
            Main.buffNoSave[Type] = true; // 不保存
            Main.buffNoTimeDisplay[Type] = false; // 显示时间
        }
        
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            float defenseReduction = (1 - AvengerPlayer.FireworkDefenseReduction) * 100;
            float damageIncrease = AvengerPlayer.FireworkCustomDefenseReduction * 100;
            tip = Language.GetText("Mods.ExpansionKele.Buff.StarryFireworkBuff.Description").Format(defenseReduction, damageIncrease);
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            // 减少50%防御
            player.statDefense *= AvengerPlayer.FireworkDefenseReduction;
            // 使玩家额外遭受50%伤害
            player.GetModPlayer<CustomDamageReductionPlayer>().AddCustomDamageReduction(-AvengerPlayer.FireworkCustomDefenseReduction);
        }
    }
    }