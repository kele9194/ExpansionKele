using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 武者芯片 - 右键刻印近战武器为【武者】子职业。
    /// 武者特性：挡1、抑制自然回血、命中回已损5%（冷却=武器useTime）、
    /// 半血以下20%减伤、已损血量线性攻速加成（最高50%）、死亡复活（10分钟冷却）
    /// </summary>
    public class MushaChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Musha;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.CopperShortsword, 1) // 生命水晶 → 契合武者的生存主题
            .AddTile(TileID.Anvils)
            .Register();
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.TinShortsword, 1) // 生命水晶 → 契合武者的生存主题
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}