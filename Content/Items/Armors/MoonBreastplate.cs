using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Armors
{
    [AutoloadEquip(EquipType.Body)]
    public class MoonBreastplate : ModItem
    {
        public override string LocalizationCategory => "Items.Armors";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Moon Breastplate");
            // Tooltip.SetDefault("8% increased damage\n12% increased melee speed\n80 increased maximum mana");
        }

        public override void SetDefaults()
        {
            //Item.SetNameOverride("望月胸甲");
            Item.width = 18;
            Item.height = 18;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);              // 卖出价格
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this);   
            Item.defense = 12;
        }

        public override void UpdateEquip(Player player)
        {
            ExpansionKeleTool.AddDamageBonus(player,0.08f);
            player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
            player.statManaMax2 += 80;
            player.maxMinions += 3;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                {"DamageBonus", "乘算增伤增加8%"},
                {"MeleeSpeed", "近战攻速增加12%"},
                {"ManaBonus", "法力上限增加80"},
                {"MinionBonus", "最大召唤栏增加3"}
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<FullMoonBar>(16);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}