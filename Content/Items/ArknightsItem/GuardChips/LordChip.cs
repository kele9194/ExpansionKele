using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 领主芯片 - 右键刻印近战武器为【领主】子职业。
    /// 领主特性：挡2、近战攻击额外发射领主剑气、
    /// 武器自带射弹获得加成、群战攻速提升
    /// </summary>
    public class LordChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Lord;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.WoodenArrow, 50)   
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}