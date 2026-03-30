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
    public class AvengerRunicTablet : ModItem
    {
        public const float DamageMultiplier = 1.04f; // 4% 乘算增伤
        
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(DamageMultiplier)
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
                player.GetModPlayer<AvengerRunicTabletPlayer>().avengerRuneEquipped = true;
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

    public class AvengerRunicTabletPlayer : ModPlayer
    {
        public bool avengerRuneEquipped = false;

        public override void ResetEffects()
        {
            avengerRuneEquipped = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (avengerRuneEquipped)
            {
                ExpansionKeleTool.MultiplyDamageBonus(Player, AvengerRunicTablet.DamageMultiplier);
            }
        }
    }
}