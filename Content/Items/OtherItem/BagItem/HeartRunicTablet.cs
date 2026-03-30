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
    public class HeartRunicTablet : ModItem
    {
        public const float MaxLifeBonus = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(MaxLifeBonus, true)
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
                player.GetModPlayer<HeartRunicTabletPlayer>().heartRuneEquipped = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.LifeCrystal, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class HeartRunicTabletPlayer : ModPlayer
    {
        public bool heartRuneEquipped = false;

        public override void ResetEffects()
        {
            heartRuneEquipped = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (heartRuneEquipped)
            {
                Player.statLifeMax2 = (int)Math.Round(Player.statLifeMax2 * (1 + HeartRunicTablet.MaxLifeBonus));
            }
        }
    }
}