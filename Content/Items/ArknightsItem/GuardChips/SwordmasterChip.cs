using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 剑豪芯片 - 右键刻印近战武器为【剑豪】子职业。
    /// 剑豪特性：挡2、攻速加快(动画时间×0.7)、
    /// 武器伤害×0.65、攻击附带15点破甲+10%减防、最终伤害×1.1(不受距离影响)
    /// </summary>
    public class SwordmasterChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Swordmaster;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.Silk, 2)      // 丝绸 — 剑豪的轻快与敏捷
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}