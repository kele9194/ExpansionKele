using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 剑豪芯片的连击复位器。
    /// <para/>
    /// <see cref="UseAnimation"/> 由 <c>Player.ItemCheck_StartActualUse</c> → <c>ApplyItemAnimation</c>
    /// 调用（<c>Player.cs:42436</c>），<b>只在实际开始挥动的那一帧执行一次</b>。
    /// <para/>
    /// 不能用 <c>itemAnimation &lt;= 0</c> 判断挥动结束：TML 里 autoReuse 的物品
    /// 同一帧内从 0 重新开始，没有 <c>itemAnimation == 0</c> 的帧（<c>Player.cs:35628</c>），
    /// 会导致「本刀已命中」标记永远清不掉，连击卡死在第 1 层。
    /// </summary>
    public class SwordmasterChipGlobalItem : GlobalItem
    {
        public override void UseAnimation(Item item, Player player)
        {
            // 只让每个客户端处理自己的那次挥动，避免多人环境重复复位。
            if (Main.dedServ || player.whoAmI != Main.myPlayer)
                return;

            if (item.IsAir || !item.CountsAsClass(DamageClass.Melee))
                return;

            // 镐 / 斧 / 锤也是 DamageClass.Melee，挖矿时不该推进连击。
            if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
                return;

            player.GetModPlayer<SwordmasterChipPlayer>().OnMeleeSwingStart();
        }
    }
}
