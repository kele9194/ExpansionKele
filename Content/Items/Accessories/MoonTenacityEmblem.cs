using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonTenacityEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const int BaseDefenseBonus = 10;
        private const float DefensePercentBonus = 0.07f; // 7%
        private const float DamageReduction = 0.05f; // 5%
        private const float BonusPerTenDefense = 0.005f; // 0.5%
        private const float MaxBonus = 0.10f; // 10% 最大加成

        public override void SetDefaults()
        {
            //Item.SetNameOverride("满月坚韧徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
            
            // 检查当前玩家是否已经有星元徽章生效，且不是自己
            if (modPlayer.activeMoonEmblemType != -1 && 
                modPlayer.activeMoonEmblemType != Item.type)
                return; // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeMoonEmblemType = Item.type;
            // 基础防御加成
            player.statDefense += BaseDefenseBonus;
            
            // 百分比防御加成
            player.statDefense += (int)(player.statDefense * DefensePercentBonus);
            
            // 自定义伤害减免
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.AddCustomDamageReduction(DamageReduction);
            
            // 根据防御力提供额外加成
            int defense = player.statDefense;
            float defenseBonusTiers = defense / 10;
            float bonus = defenseBonusTiers * BonusPerTenDefense;
            
            // 限制最大加成
            if (bonus > MaxBonus)
                bonus = MaxBonus;
                
            // 应用伤害加成
            var damageMultiPlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damageMultiPlayer.AddMultiplicativeDamageBonus(bonus);
            
            // 应用额外减伤
            reductionPlayer.AddCustomDamageReduction(bonus);
            player.noKnockback = true;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonTenacityEmblemDefenseBase", $"[c/00FF00:+{BaseDefenseBonus}防御力]"},
                    {"MoonTenacityEmblemDefensePercent", $"[c/00FF00:+{DefensePercentBonus * 100}%防御力]"},
                    {"MoonTenacityEmblemReduction", $"[c/00FF00:+{DamageReduction * 100}%自定义伤害减免]"},
                    {"MoonTenacityEmblemScaling", $"[c/00FF00:每10点防御力增加{BonusPerTenDefense * 100}%伤害和减伤]"},
                    {"MoonTenacityEmblemMax", $"[c/00FF00:最多增加{MaxBonus * 100}%伤害和减伤]"},
                    {"MoonTenacityEmblemKnockback", "[c/00FF00:免疫击退]"},
                    {"WARNING", "[c/800000:注意：多个满月徽章装备将只有第一个生效]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<FullMoonBar>(), 3);
            recipe.AddIngredient(ItemID.WarriorEmblem, 1);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 3);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
            
        }
    }
}