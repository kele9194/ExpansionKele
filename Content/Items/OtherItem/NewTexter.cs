using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    /// <summary>
    /// NewTexter - 调试标记工具
    /// 收藏此物品后，所有使用DebugMarker.Mark()的代码位置都会在聊天框中显示调试信息
    /// </summary>
    public class NewTexter : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("NewTexter");
            // Tooltip.SetDefault("收藏此物品以启用调试标记输出\n显示代码位置和调试信息");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateInventory(Player player)
        {
            // 当物品被收藏时，重置调试状态缓存
            if (Item.favorited)
            {
                DebugMarker.ResetDebugCache();
            }
        }


        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DebugInfo", 
                "[c/FFFF00:收藏此物品以启用调试标记]")
            {
                OverrideColor = new Color(255, 255, 0)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DebugDesc", 
                "允许在聊天框中显示代码调试信息")
            {
                OverrideColor = new Color(200, 200, 200)
            });
        }
    }
}