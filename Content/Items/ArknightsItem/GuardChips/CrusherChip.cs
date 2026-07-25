using ExpansionKele.Content.Customs;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    public class CrusherChip : GuardChipBase
    {
        protected override GuardSubclass ChipSubclass => GuardSubclass.Crusher;

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.CopperHammer, 1)
            .AddTile(TileID.Anvils)
            .Register();
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
            .AddIngredient(ItemID.TinHammer, 1)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}