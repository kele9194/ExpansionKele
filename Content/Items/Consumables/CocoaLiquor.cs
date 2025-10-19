using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;

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
            //Item.SetNameOverride("可可原液");
            Item.width = 16;
            Item.height = 24;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.consumable = true;
            Item.UseSound = SoundID.Item3;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 20);
            Item.maxStack = 30;
            Item.healLife = 60; // 回复60点生命值
            Item.buffType = ModContent.BuffType<CocoaBuff>();
            Item.buffTime = 30 * 60; // 30秒增益效果
        }

        public override bool? UseItem(Player player)
        {
            player.AddBuff(BuffID.PotionSickness, 30 * 60); // 添加30秒药水病
            player.AddBuff(Item.buffType, Item.buffTime); // 添加可可增益
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "CocoaLiquorDescription", "饮用时回复60点生命值，并且获得30s药水病\n同时会获得可可增益：提升自身移速20%，增加4点防御力，提升5%暴击率和1生命再生，持续30s"));
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<OtherItem.CocoaBeans>(), 1)
                .AddIngredient(ItemID.BottledWater, 2)
                .AddTile(TileID.Bottles)
                .Register();

            CreateRecipe(2)
                .AddIngredient(ModContent.ItemType<OtherItem.CocoaBeans>(), 1)
                .AddIngredient(ItemID.BottledWater, 2)
                .AddTile(TileID.AlchemyTable)
                .Register();
        }
    }
}