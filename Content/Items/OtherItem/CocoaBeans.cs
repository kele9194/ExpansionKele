using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class CocoaBeans : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("可可豆");
            // Tooltip.SetDefault("一种可以用来制作巧克力的食材");
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("可可豆");
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips) {  
			// tooltips.Add(new TooltipLine(Mod, "obtain", "可以通过砍伐树木获得"));
            // tooltips.Add(new TooltipLine(Mod, "pity", "作者碎碎念：找不到摇树还找不到红木树判断方法"));
        }  
    }
    

    public class TreeShakeDrop : GlobalTile
    {
        public override void Drop(int i, int j, int type)
        {
            // 检查是否是树木方块
            if (type == TileID.Trees)
            {

                    // 5%概率掉落可可豆
                    if (Main.rand.NextFloat() < 0.05f)
                    {
                        // 掉落1-3个可可豆
                        int itemDrop = Main.rand.Next(1, 4);
                        int itemType = ModContent.ItemType<CocoaBeans>();
                        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType, itemDrop);
                    }
                }
            }
        }
}
