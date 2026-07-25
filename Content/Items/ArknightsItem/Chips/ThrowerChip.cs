using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 投掷手刻印芯片：右键使用，将手持武器刻印为投掷手（狙击 · 投掷手）
    /// 特性：主弹命中造成0.75倍伤害，随后0.5秒和1.0秒各产生一次小范围爆炸
    /// </summary>
    public class ThrowerChip : SniperChipBase
    {
        protected override SniperSubclass ChipSubclass => SniperSubclass.Thrower;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.Grenade, 1)    //手榴弹
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}