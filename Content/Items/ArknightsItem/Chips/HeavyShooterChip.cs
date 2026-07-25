using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.ID;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 重射手刻印芯片：右键使用，将手持武器刻印为重射手（狙击 · 重射手）
    /// 特性：伤害×1.3，攻速×0.75，近身增伤，角度判定增伤，切武器攻速爆发
    /// </summary>
    public class HeavyShooterChip : SniperChipBase
    {
        protected override SniperSubclass ChipSubclass => SniperSubclass.HeavyShooter;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.CopperBar, 5)   // 铅锭 → 沉重/重型的象征
            .AddTile(TileID.Anvils)
            .Register();

            // 也支持铁锭配方
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<SniperBaseChip>(), 1)
            .AddIngredient(ItemID.TinBar, 5)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}