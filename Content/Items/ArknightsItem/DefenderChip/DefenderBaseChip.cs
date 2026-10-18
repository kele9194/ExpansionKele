using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.DefenderChip
{
    /// <summary>
    /// 基础重装芯片 - 用于合成各类重装子芯片的基础材料。
    /// </summary>
    public class DefenderBaseChip : ModItem
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