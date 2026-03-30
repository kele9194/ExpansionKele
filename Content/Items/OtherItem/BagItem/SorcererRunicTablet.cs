using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class SorcererRunicTablet : ModItem
    {
        public const int ManaBonus = 20;
        public const float MagicCritBonus = 0.02f;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue(ManaBonus),
            ValueUtils.FormatValue(MagicCritBonus, true)
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
                player.GetModPlayer<SorcererRuneStoneTabletPlayer>().runeStoneEquipped = true;
                PlayerUtils.ReduceManaSicknessDuration(player);
                player.manaFlower = true;
            }
            

        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WoodDemonInstrument>(), 1)
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddIngredient(ItemID.ManaCrystal, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class SorcererRuneStoneTabletPlayer : ModPlayer
    {
        public bool runeStoneEquipped = false;

        public override void ResetEffects()
        {
            runeStoneEquipped = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (runeStoneEquipped)
            {
                Player.statManaMax2 += SorcererRunicTablet.ManaBonus;
                Player.GetCritChance(DamageClass.Magic) += SorcererRunicTablet.MagicCritBonus;
            }
        }
    }
}