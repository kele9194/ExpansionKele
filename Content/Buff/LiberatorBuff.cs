using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Buff
{
    /// <summary>
    /// 解放者芯片的「解放」状态标记。
    /// <para/>
    /// 这个 buff 本身不提供任何数值：它只是一块「解放窗口还剩多久」的计时牌，
    /// 让玩家能从 buff 栏看到当前处于解放期。真正的伤害倍率由
    /// <see cref="Items.ArknightsItem.Accessories.Guard.LiberatorChipPlayer"/> 按解放值实时计算，
    /// 所以即使 buff 被玩家手动取消，也只是提前结束窗口，不会残留倍率。
    /// </summary>
    public class LiberatorBuff : ModBuff
    {
        public override string LocalizationCategory => "Buff";

        public override void SetStaticDefaults()
        {
            // 退出世界不保留：解放是战斗内的临时窗口，跨图带回来会很怪
            Main.buffNoSave[Type] = true;

            // 需要显示剩余时间，玩家要据此判断还能打几刀
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
