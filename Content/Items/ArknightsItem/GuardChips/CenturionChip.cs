using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 强攻手芯片 - 右键刻印近战武器为【强攻手】子职业。
    /// 强攻手特性：挡2、对被阻挡敌人1.12x增伤、半血以上20%减伤、溅射传导20%
    /// </summary>
    public class CenturionChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Centurion;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.CopperBar, 5)  // 铁条，契合"强攻"主题
            .AddTile(TileID.Anvils)
            .Register();

            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.TinBar, 5)  // 铁条，契合"强攻"主题
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}