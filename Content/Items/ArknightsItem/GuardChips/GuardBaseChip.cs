using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 基础近卫芯片 - 用于合成各类近卫子芯片的基础材料。
    /// </summary>
    public class GuardBaseChip : ModItem
    {
        public override string LocalizationCategory => "Items.Chips";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.White;
            Item.value = Item.buyPrice(silver: 2);
        }
    }
}