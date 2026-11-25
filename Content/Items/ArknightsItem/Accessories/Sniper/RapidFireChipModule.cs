using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper
{
    /// <summary>
    /// 速射手芯片模组（二级芯片）：包含一级 <see cref="RapidFireChip"/> 的全部效果，
    /// 并追加「全队削防」——每击中目标一次 -1 点，装备 15 点上限 / 仅收藏 5 点上限。
    /// <para/>
    /// 每个效果维度只取最强的一个来源（装备优先于收藏、同档比等级），不同维度各自生效；
    /// 所以两件同时存在时不会重复结算同一个维度。
    /// </summary>
    public class RapidFireChipModule : ModItem
    {
        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            // 配方待定（暂无获取途径），固定价值 / 稀有度，避免 CalculateValueFromRecipes 走 fallback。
            Item.value = Item.buyPrice(silver: 30);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 具体数值由 RapidFireChipPlayer 按维度下发，这里只做「持有」标记。
            player.GetModPlayer<RapidFireChipPlayer>().moduleEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<RapidFireChipPlayer>().moduleFavorited = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            RapidFireChipPlayer.AddDynamicTooltips(tooltips, Mod,
                "Mods.ExpansionKele.Items.ArknightsItem.Accessories.RapidFireChipModule.");
        }
    }
}
