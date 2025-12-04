using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class WoodDemonInstrument : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        // 物品基础设置
        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("木魔仪");
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1; // 最大堆叠数为1
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.consumable = false; // 无法放置
        }

        // 合成配方：12个木头在工作台合成
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 12) // 需要12个木头
                .AddTile(TileID.WorkBenches) // 在工作台制作
                .Register();
        }

        // 持续监控背包中的物品
        public override void UpdateInventory(Player player)
        {
            // 设置玩家的魔力花效果字段
            player.manaFlower = true;

            PlayerUtils.ReduceManaSicknessDuration(player);
            

        }

        // 自定义提示信息 - 使用紫色显示特殊功能
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "WoodDemonInstrumentInfo", 
                "允许将其放在背包中自动使用魔力药水") 
            {
                OverrideColor = new Color(175, 75, 255) // 紫色提示
            });

            tooltips.Add(new TooltipLine(Mod, "WoodDemonInstrumentInfo", 
                "测试：装备后魔力病持续时间减少33%") 
            {
                OverrideColor = new Color(175, 75, 255) // 紫色提示
            });
        }



        
    }
}