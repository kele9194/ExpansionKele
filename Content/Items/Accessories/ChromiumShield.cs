using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class ChromiumShield : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";

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
            // 添加14%防御前减伤
            ExpansionKeleTool.MultiplyPreDefenseDamageReduction(player, 0.88f);
            ECShieldSystem ecshield = player.GetModPlayer<ECShieldSystem>();
            ecshield.ShieldActive = true;
            ecshield.MaxShieldModifier.Base+=50f;
            ecshield.ShieldRegenModifier.Base+=5f;
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