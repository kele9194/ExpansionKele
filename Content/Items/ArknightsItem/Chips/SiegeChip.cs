using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 攻城手刻印芯片：右键使用，将手持武器刻印为攻城手（狙击 · 攻城手）
    /// 特性：体积判定锁定、近身惩罚、远程增伤、轻微追踪
    /// </summary>
    public class SiegeChip : SniperChipBase
    {
        protected override SniperSubclass ChipSubclass => SniperSubclass.Siege;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.CopperPickaxe, 1)   // 铁镐
            .AddTile(TileID.Anvils)
            .Register();

            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.TinPickaxe, 1)   // 铁镐
            .AddTile(TileID.Anvils)
            .Register();

        }
    }
}