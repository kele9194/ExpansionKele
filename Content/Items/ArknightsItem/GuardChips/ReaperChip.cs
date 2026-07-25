using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 收割者芯片 - 右键刻印近战武器为【收割者】子职业。
    /// 收割者特性：挡2、抑制自然生命再生、命中回复已损生命2%
    /// </summary>
    public class ReaperChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Reaper;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.LesserHealingPotion, 2)   // 暗影之魂 — 收割者的黑暗与掠夺
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}