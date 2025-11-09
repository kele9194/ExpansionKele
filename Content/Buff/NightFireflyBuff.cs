using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    public class NightFireflyBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 萤火状态逻辑将在玩家更新中处理
        }
    }
}