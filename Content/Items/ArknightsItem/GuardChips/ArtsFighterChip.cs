using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 术战者芯片 - 右键刻印近战武器为【术战者】子职业。
    /// 术战者特性：挡1、伤害/弹幕转法术、30%防御穿透、非阻挡目标1.12x增伤、阻挡目标10%易伤
    /// </summary>
    public class ArtsFighterChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.ArtsFighter;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.ManaCrystal, 1)  // 法力水晶，契合"法术"主题
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}