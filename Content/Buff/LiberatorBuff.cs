using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 解放状态 Buff：持续 10 秒。
    /// 期间维持充能的伤害倍率不变，阻挡数提升至 2。
    /// 实际逻辑由 <see cref="LiberatorPlayer"/> 驱动。
    /// </summary>
    public class LiberatorBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
        }
    }
}