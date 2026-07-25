using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 武者复活冷却 Debuff — 由武者致命伤害触发复活后施加。
    /// 持续10分钟（36000帧），期间再次死亡将真正死亡。
    /// </summary>
    public class MushaRevivalCooldown : ModBuff
    {
        public override string LocalizationCategory => "Buff";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }
}