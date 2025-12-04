using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class Wrench : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetDefaults()
        {
            //Item.SetNameOverride("扳手");
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1; // 最大堆叠数为1
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("ExpansionKele:AnyIronBars", 5)// 需要12个木头
                .AddTile(TileID.WorkBenches) // 在工作台制作
                .Register();
        }

        // 静态方法用于检查并修改手持物品的 autoUse 状态
        public override void UpdateInventory(Player player)
    {
        CheckAndFixAutoUse(player);
    }

    public static void CheckAndFixAutoUse(Player player)
    {
        Item heldItem = player.inventory[player.selectedItem];

        if (heldItem != null && !heldItem.IsAir && !heldItem.autoReuse )
        {
            heldItem.autoReuse = true;
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "WoodDemonInstrumentInfo", 
                "允许放入背包时使手持物品自动挥舞") 
            {
                OverrideColor = new Color(75, 75, 255)
            });
        }

    
    }
}