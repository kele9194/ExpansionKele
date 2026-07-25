using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 速射狙刻印芯片：右键使用，将手持武器刻印为速射狙（狙击 · 速射狙）
    /// 特性：对空特化，射速快，射弹速度快，对空中目标轻微追踪
    /// </summary>
    public class RapidFireChip : SniperChipBase
    {
        protected override SniperSubclass ChipSubclass => SniperSubclass.RapidFire;
        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.Feather,1)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}