using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 解放者芯片 - 右键刻印近战武器为【解放者】子职业。
    /// 解放者特性：不使用物品时逐渐充能武器伤害倍率（最高+2.4倍），
    /// 使用武器时进入解放状态（维持倍率10s、挡2），非解放不阻挡。
    /// </summary>
    public class LiberatorChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Liberator;

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.WoodenHammer, 1)   // 光明之魂 — 解放者长期积蓄力量
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}