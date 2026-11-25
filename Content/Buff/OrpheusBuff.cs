using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 二阶段「俄耳甫斯」的契约图标。
    /// 剩余时间就是替身剩余时间，所以刻意开启时间显示 —— 它本身就是契约倒计时。
    /// </summary>
    public class OrpheusBuff : ModBuff
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
