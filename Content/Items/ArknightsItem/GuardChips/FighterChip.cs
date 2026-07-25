using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 斗士芯片 - 右键刻印近战武器为【斗士】子职业。
    /// 斗士特性：挡1、基础攻速1.12x、15%基础闪避、
    /// 半血以上+10%近战攻速、命中叠层(每层+1%闪避+2%攻速,最多5层)
    /// </summary>
    public class FighterChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Fighter;

        public override void AddRecipes()
        {
            // 或替代材料
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.Feather, 1) // 5根羽毛替代
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}