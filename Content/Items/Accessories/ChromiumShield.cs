using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;
using Terraria.Localization;
using System;

namespace ExpansionKele.Content.Items.Accessories
{
   [AutoloadEquip(EquipType.Shield)]
    public class ChromiumShield : ModItem
    {
        public static float PreDefenseReduction = 0.88f; // 12% 防御前减伤
        public static float MaxShield = 50f;
        public static float ShieldRegen = 5f;
        
        public override string LocalizationCategory => "Items.Accessories";
        
       public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(
            ValueUtils.FormatValue((1 - PreDefenseReduction) * 100),
            ValueUtils.FormatValue(MaxShield),
            ValueUtils.FormatValue(ShieldRegen)
        );
        

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
            Item.defense = 2;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 添加 14% 防御前减伤
            ExpansionKeleTool.MultiplyPreDefenseDamageReduction(player, PreDefenseReduction);
            ECShieldSystem ecshield = player.GetModPlayer<ECShieldSystem>();
            ecshield.ShieldActive = true;
            ecshield.MaxShieldModifier.Base+=MaxShield;
            ecshield.ShieldRegenModifier.Base+=ShieldRegen;
            player.noKnockback=true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ChromiumBar>(12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}