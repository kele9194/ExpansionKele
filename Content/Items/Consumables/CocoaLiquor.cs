using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Materials;

namespace ExpansionKele.Content.Items.Consumables
{
    public class CocoaLiquor : ModItem
    {
        public override string LocalizationCategory => "Items.Consumables";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("可可原液");
            // Tooltip.SetDefault("饮用时回复60点生命值，并且获得30s药水病，同时会获得可可增益：提升自身移速20%，增加4点防御力，提升5%暴击率，持续30s");
        }

        public override void SetDefaults()
        {
            Item.DefaultToHealingPotion(16, 24, 100);
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.buffType = ModContent.BuffType<CocoaBuff>();
            Item.buffTime = 60 * 60; // 30秒增益效果
        }


        public override bool? UseItem(Player player)
        {
            player.AddBuff(Item.buffType, Item.buffTime); // 添加可可增益
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            
           
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<CocoaBeans>(), 1)
                .AddIngredient(ItemID.BottledWater, 2)
                .AddTile(TileID.Bottles)
                .Register();

            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<CocoaBeans>(), 1)
                .AddIngredient(ItemID.BottledWater, 2)
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }
}