using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;

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
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 11;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 5;
            player.ammoCost80 = true;
            player.GetDamage(DamageClass.Summon) += 0.08f;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.15f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                {"CritBonus", "暴击率增加5%"},
                {"AmmoSave", "弹药消耗减少20%"},
                {"SummonDamage", "召唤伤害增加8%"},
                {"WhipSpeed", "鞭子攻速增加15%"}
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
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