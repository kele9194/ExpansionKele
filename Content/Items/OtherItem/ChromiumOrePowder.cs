using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class ChromiumOrePowder : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("铬矿粉");
            // Tooltip.SetDefault("一种闪闪发光的金属粉末，可以用于冶炼");
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.White;
        }
    }
}