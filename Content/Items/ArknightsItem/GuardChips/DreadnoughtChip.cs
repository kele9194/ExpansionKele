using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 无畏者芯片 - 右键刻印近战武器为【无畏者】子职业。
    /// 无畏者特性：挡1、阻挡增伤1.15x、致命伤害触发无畏重生（回满血、生命上限变40%、攻速×1.3）
    /// </summary>
    public class DreadnoughtChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Dreadnought;
        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(),1)
            .AddIngredient(ItemID.CopperBroadsword,1)
            .AddTile(TileID.Anvils)
            .Register();

            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(),1)
            .AddIngredient(ItemID.TinBroadsword,1)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}