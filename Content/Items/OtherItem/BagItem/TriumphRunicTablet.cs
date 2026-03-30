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
    public class TriumphRunicTablet : ModItem
    {
        public const float MaxLifeBonus = HeartRunicTablet.MaxLifeBonus;
        public const int DefenseBonus = TenacityRunicTablet.DefenseBonus;
        public const float DamageReduction = TenacityRunicTablet.DamageReduction;
        public const float DamageMultiplier = AvengerRunicTablet.DamageMultiplier;
        
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(MaxLifeBonus, true),
            ValueUtils.FormatValue(DefenseBonus),
            ValueUtils.FormatValue(DamageReduction),
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
                player.GetModPlayer<TriumphRunicTabletPlayer>().triumphRuneEquipped = true;
                
                player.GetModPlayer<HeartRunicTabletPlayer>().heartRuneEquipped = true;
                player.GetModPlayer<TenacityRunicTabletPlayer>().tenacityRuneEquipped = true;
                player.GetModPlayer<AvengerRunicTabletPlayer>().avengerRuneEquipped = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HeartRunicTablet>(), 1)
                .AddIngredient(ModContent.ItemType<TenacityRunicTablet>(), 1)
                .AddIngredient(ModContent.ItemType<AvengerRunicTablet>(), 1)
                .AddIngredient(ItemID.HellstoneBar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var heartTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.HeartRunicTablet.Tooltip");
            var tenacityTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.TenacityRunicTablet.Tooltip");
            var avengerTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.AvengerRunicTablet.Tooltip");
            
        }
    }

    public class TriumphRunicTabletPlayer : ModPlayer
    {
        public bool triumphRuneEquipped = false;

        public override void ResetEffects()
        {
            triumphRuneEquipped = false;
        }
    }
}