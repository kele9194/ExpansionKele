using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace ExpansionKele.Content.Buff
{ 

public class HallowHeartShieldCooldown : ModBuff
{
    public override string LocalizationCategory => "Buff";
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("神圣心盾冷却中");
        // Description.SetDefault("神圣心盾的周期性无敌正在冷却中");
        Main.buffNoTimeDisplay[Type] = false;
        Main.debuff[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}
}
