using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria.ID;
using ExpansionKele.Content.Items.Weapons.Ranged;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Buff
{
    public class FadeHitDebuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // 护士无法移除
        }

        public override void Update(Player player, ref int buffIndex)
        {

            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.MultiPreDefenseDamageReduction(1.2f);
            if (Main.rand.NextDouble() <= 0.5)
                {
                    player.lifeRegenTime -= 1;
                }

            
        }
    }
}