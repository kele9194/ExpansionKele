using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Global;
using ExpansionKele.Content.Items.Placeables;
using System.Text;

namespace ExpansionKele.Content.Items.OtherItem.BagItem
{
    public class UnityRunicTablet : ModItem
    {
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
                player.GetModPlayer<UnityRunicTabletPlayer>().unityRuneEquipped = true;
                
                player.GetModPlayer<RuneStoneTabletPlayer>().runeStoneEquipped = true;
                player.GetModPlayer<MarksmanRuneStonePlayer>().marksmanRuneStoneEquipped = true;
                player.GetModPlayer<SorcererRuneStoneTabletPlayer>().runeStoneEquipped = true;
                player.GetModPlayer<SummonerRunicTabletPlayer>().summonerRuneEquipped = true;
                
                PlayerUtils.ReduceManaSicknessDuration(player);
                player.manaFlower = true;
                Wrench.CheckAndFixAutoUse(player);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<WarriorRunicTablet>(), 1)
                .AddIngredient(ModContent.ItemType<RangerRunicTablet>(), 1)
                .AddIngredient(ModContent.ItemType<SorcererRunicTablet>(), 1)
                .AddIngredient(ModContent.ItemType<SummonerRunicTablet>(), 1)
                .AddIngredient(ItemID.HellstoneBar, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var warriorTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.WarriorRunicTablet.Tooltip");
            var rangerTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.RangerRunicTablet.Tooltip");
            var sorcererTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.SorcererRunicTablet.Tooltip");
            var summonerTooltip = Language.GetTextValue("Mods.ExpansionKele.Items.OtherItem.SummonerRunicTablet.Tooltip");
        
        }
    }

    public class UnityRunicTabletPlayer : ModPlayer
    {
        public bool unityRuneEquipped = false;

        public override void ResetEffects()
        {
            unityRuneEquipped = false;
        }
    }
}