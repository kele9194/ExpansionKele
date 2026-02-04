using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Armors
{
    [AutoloadEquip(EquipType.Legs)]
    public class MoonLeggings : ModItem
    {
        public override string LocalizationCategory => "Items.Armors";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Moon Leggings");
            // Tooltip.SetDefault("5% increased critical strike chance\n20% chance to not consume ammo\n8% increased summon damage");
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月护腿");
            Item.width = 18;
            Item.height = 18;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.defense = 11;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 6;
            player.ammoCost80 = true;
            player.GetDamage(DamageClass.Summon) += 0.06f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<FullMoonBar>(12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}