using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class FullMoonAmuletSegment1Cooldown : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月护符第一段冷却");
            // Description.SetDefault("望月护符的第一段血量保护正在冷却中");
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
         

   
    }
    public class FullMoonAmuletSegment2Cooldown : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月护符第二段冷却");
            // Description.SetDefault("望月护符的第二段血量保护正在冷却中");
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
     public class FullMoonAmuletSegment3Cooldown : ModBuff
    {
        public override string LocalizationCategory => "Buff";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月护符第三段冷却");
            // Description.SetDefault("望月护符的第三段血量保护正在冷却中");
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}