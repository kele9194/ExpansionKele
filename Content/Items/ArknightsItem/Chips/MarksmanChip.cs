using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 神射手刻印芯片：右键使用，将手持武器刻印为神射手（狙击 · 神射手）
    /// 特性：对防御最低的敌人增伤、轻微追踪、未击杀补偿、距离增伤
    /// </summary>
    public class MarksmanChip : SniperChipBase
    {
        protected override SniperSubclass ChipSubclass => SniperSubclass.Marksman;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.Lens, 1)    // 透镜 → 精准/瞄准的象征
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}