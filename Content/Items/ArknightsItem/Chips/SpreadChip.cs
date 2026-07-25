using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 散射手刻印芯片：右键使用，将手持武器刻印为散射手（狙击 · 散射手）
    /// 特性：每次射击积累弹药伤害，3帧后释放扇形AOE
    /// </summary>
    public class SpreadChip : SniperChipBase
    {
        protected override SniperSubclass ChipSubclass => SniperSubclass.Spread;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.Cobweb, 10)  // 蛛网
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}