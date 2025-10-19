using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;

namespace ExpansionKele.Content.Items.Consumables
{
    public class RapidBashPotion : ModItem
    {
        public override string LocalizationCategory => "Items.Consumables";
        public static int time=4;        
        public override void SetStaticDefaults()
        {
            
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("狂击药水");
            Item.width = 20;
            Item.height = 28;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.consumable = true;
            Item.UseSound = SoundID.Item3;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
            Item.buffType = ModContent.BuffType<RapidBash>();
            Item.buffTime = time *60* 60; // 4 minutes in ticks
            Item.maxStack=9999;
        }

        public override bool? UseItem(Player player)
        {
            player.AddBuff(Item.buffType, Item.buffTime);
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Introduction", $"增加{RapidBash.buffBonus*100}%近战攻击速度，持续{time}分钟"));
            tooltips.Add(new TooltipLine(Mod, "Introduction", $"研究所一次偶然的实验发现暴怒药水和怒气药水的混合可以显著提升测试对象的攻击欲望..."));
        }
        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ItemID.RagePotion, 1)
                .AddIngredient(ItemID.WrathPotion, 1)
                .AddTile(TileID.Bottles)
                .Register();
        }
    }
}