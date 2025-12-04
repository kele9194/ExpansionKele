using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Consumables
{
    public class ChocolateBar : ModItem
    {
        public override string LocalizationCategory => "Items.Consumables";
        public static int timeInMiniute=5;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("巧克力棒");
            // Tooltip.SetDefault("提供中幅度食物增益5分钟的同时还可以获得可可原液5分钟");
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("巧克力");
            Item.width = 20;
            Item.height = 20;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.consumable = true;
            Item.UseSound = SoundID.Item2;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.maxStack = 30;
            Item.buffType = ModContent.BuffType<Buff.CocoaBuff>();
            Item.buffTime = timeInMiniute * 60 * 60; // 5分钟可可增益效果
        }

        public override bool? UseItem(Player player)
        {
            // 添加中等幅度的食物增益（Well Fed的更强版本）
            player.AddBuff(BuffID.WellFed2, Item.buffTime); // 5分钟食物增益
            player.AddBuff(Item.buffType, Item.buffTime); // 5分钟可可增益
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ChocolateBarDescription", $"提供很满意食物增益{timeInMiniute}分钟\n同时还可以获得可可增益{timeInMiniute}分钟"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<CocoaLiquor>(), 2)
                .AddTile(TileID.Solidifier) // 使用食物拼盘作为合成站，模拟固化机效果
                .Register();
        }
    }
}