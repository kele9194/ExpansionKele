using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;
using System;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class TenacityRunicTablet : ModItem
    {
        public const int DefenseBonus = 2;
        public const float DamageReduction = 0.96f; // 96% 受伤 = 4% 减伤
        
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(DefenseBonus),
            ValueUtils.FormatValue(DamageReduction)
        );

        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
            {
                player.GetModPlayer<TenacityRunicTabletPlayer>().tenacityRuneEquipped = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.IronBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class TenacityRunicTabletPlayer : ModPlayer
    {
        public bool tenacityRuneEquipped = false;

        public override void ResetEffects()
        {
            tenacityRuneEquipped = false;
        }
        public override void UpdateEquips()
        {

            //由于未知原因，防御只有放在这里才能生效
            if (tenacityRuneEquipped)
            {
                Player.statDefense += TenacityRunicTablet.DefenseBonus;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if (tenacityRuneEquipped)
            {
                var reductionPlayer = Player.GetModPlayer<CustomDamageReductionPlayer>();
                reductionPlayer.MulticustomDamageReduction(TenacityRunicTablet.DamageReduction);
            }
        }

    }
}