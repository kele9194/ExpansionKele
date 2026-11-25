using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 收割者芯片的回血额度授予器。
    /// <para/>
    /// <see cref="UseAnimation"/> 由 <c>Player.ItemCheck_StartActualUse</c> → <c>ApplyItemAnimation</c>
    /// 调用（<c>Player.cs:42436</c>），<b>只在实际开始挥动的那一帧执行一次</b>
    /// —— 这是「一次 useAnimation」最可靠的信号，按住攻击也不会漏，
    /// 也不依赖 <c>itemAnimation</c> 的逐帧递减。
    /// <para/>
    /// 与 <see cref="LordChipGlobalItem"/> / <see cref="MushaChipGlobalItem"/> 同一套判定
    /// （排除工具、只处理本地玩家）。
    /// </summary>
    public class ReaperChipGlobalItem : GlobalItem
    {
        public override void UseAnimation(Item item, Player player)
        {
            // 只让每个客户端处理自己的那次挥动，避免多人环境重复授予。
            if (Main.dedServ || player.whoAmI != Main.myPlayer)
                return;

            if (item.IsAir || !item.CountsAsClass(DamageClass.Melee))
                return;

            // 镐 / 斧 / 锤也是 DamageClass.Melee，挖矿时不该回血。
            if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
                return;

            player.GetModPlayer<ReaperChipPlayer>().OnMeleeSwingStart();
        }
    }
}
