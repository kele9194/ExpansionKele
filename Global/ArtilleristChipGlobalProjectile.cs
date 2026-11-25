using Terraria.ModLoader;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 炮手芯片的「每发弹幕只炸一次」标记。
    /// <para/>
    /// 必须存在弹幕自己身上而不是玩家身上 —— 一次攻击可能发射多发弹幕，
    /// 存在玩家上会变成「整轮攻击只炸一次」。
    /// </summary>
    public class ArtilleristChipGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        /// <summary>该弹幕是否已经触发过溅射。</summary>
        public bool aoeTriggered;
    }
}
