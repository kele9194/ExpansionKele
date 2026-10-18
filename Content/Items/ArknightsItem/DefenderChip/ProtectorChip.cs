using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.DefenderChip
{
    /// <summary>
    /// 铁卫芯片 - 右键刻印近战武器为【铁卫】子职业。
    /// 铁卫特性：挡4、自身防御有效性×1.2
    /// </summary>
    public class ProtectorChip : DefenderChipBase
    {
        protected override DefenderSubclass ChipSubclass => DefenderSubclass.Protector;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<DefenderBaseChip>(), 1)
            .AddIngredient(ItemID.IronBar, 1) // 铁卫 → 铁锭
            .AddTile(TileID.Anvils)
            .Register();

            // 若世界生成铅矿，则可用铅锭替代
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<DefenderBaseChip>(), 1)
            .AddIngredient(ItemID.LeadBar, 1)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}