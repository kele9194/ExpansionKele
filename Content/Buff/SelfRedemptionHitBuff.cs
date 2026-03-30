using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Buff
{
    public class SelfRedemptionHitBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {

                var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
                reductionPlayer.MultiPreDefenseDamageReduction(0.8f);
                
                if (Main.rand.NextDouble() <= 0.5)
                {
                    player.lifeRegenTime += 2;
                }
        }
    }
}