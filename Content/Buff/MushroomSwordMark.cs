using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ExpansionKele.Content.Buff
{
    // 用于标记敌人的debuff
    public class MushroomSwordMark : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 标记期间NPC显示特殊颜色效果
            npc.color = Color.GreenYellow;
        }
    }
}