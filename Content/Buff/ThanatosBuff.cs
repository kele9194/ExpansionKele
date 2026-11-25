using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 一阶段「塔纳托斯」的契约图标。
    /// 剩余时间就是替身剩余时间，所以刻意开启时间显示 —— 它本身就是契约倒计时。
    /// </summary>
    public class ThanatosBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
        }
    }
}
