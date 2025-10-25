using ExpansionKele.Commons;
using ExpansionKele.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Global 
{  
    public class ExpansionKeleGlobalItem : GlobalItem
    {
        public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
        {
            // extractType: 0 = 泥沙/雪泥, 3347 = 沙漠化石
            switch (extractType)
            {
                // 泥沙和雪泥
                case 0:
                    // 4%概率产出望月矿
                    if (Main.rand.NextFloat() < 0.04f)
                    {
                        // Main.NewText($"{extractType}");
                        resultType = ModContent.ItemType<SigwutBar>();
                        resultStack = Main.rand.Next(1, 17); // 1-16个
                    }
                    break;
                    
                // 沙漠化石
                case 3347:
                    // 4%概率产出星光矿
                    if (Main.rand.NextFloat() < 0.04f)
                    {
                        // Main.NewText($"{extractType}");
                        resultType = ModContent.ItemType<SigwutBar>();
                        resultStack = Main.rand.Next(1, 17); // 1-16个
                    }
                    break;
                default:{
                    Main.NewText($"{extractType}");
                    break;
                }
                
                    
            }
        }

        public override void UseItemFrame(Item item, Player player)
        {
            // 应用改进的物品定位逻辑到所有近战挥舞类武器
            if (item.useStyle == ItemUseStyleID.Swing)
            {
                ExpansionKeleUtils.ConductBetterItemLocation(player);
            }
        }

    }
}

