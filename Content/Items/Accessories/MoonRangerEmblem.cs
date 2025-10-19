using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonRangerEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float RangedDamageBonus = 0.15f; // +15%远程伤害
        private const int BaseArmorPenetration = 6;
        private const int BaseDamage = 4;
        private const float ArmorPenetrationPerDamage = 2f; // 每4%额外远程伤害提供1穿甲
        private const float DamagePerDamage = 4f; // 每8%额外远程伤害提供1点面板伤害

        public override void SetDefaults()
        {
            //Item.SetNameOverride("游侠满月徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 5, 0, 0);
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
            float additionalRangedDamage = player.GetDamage(DamageClass.Ranged).Additive - 1f;
            additionalRangedDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.GetModPlayer<DamageFlatBonusRanger>().DamageFlatBonus += BaseDamage;// +4伤害
            player.GetModPlayer<DamageFlatBonusRanger>().DamageFlatBonus += (int)(additionalRangedDamage / DamagePerDamage * 100);//每8%额外远程伤害加成提供1点面板伤害
            player.GetArmorPenetration(DamageClass.Ranged) += BaseArmorPenetration; // +6穿甲 
            player.GetArmorPenetration(DamageClass.Ranged) += additionalRangedDamage / ArmorPenetrationPerDamage * 100;// 每4%额外远程伤害提供1穿甲
            player.GetDamage(DamageClass.Ranged) += RangedDamageBonus; // +15%远程伤害
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonRangerEmblemDamage", $"[c/00FF00:+{(int)(RangedDamageBonus * 100)}%远程伤害]"},
                    {"MoonRangerEmblemPenetration", $"[c/00FF00:+{BaseArmorPenetration}远程穿甲]"},
                    {"MoonRangerEmblemDamageFlat", $"[c/00FF00:+{BaseDamage}伤害]"},
                    {"MoonRangerEmblemBonus1", $"[c/00FF00:每{ArmorPenetrationPerDamage}%额外远程伤害提供1远程穿甲]"},
                    {"MoonRangerEmblemBonus2", $"[c/00FF00:每{DamagePerDamage}%额外远程伤害提供1点远程武器面板伤害]"},
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
            recipe.AddIngredient(ModContent.ItemType<ArmorPiercingNecklace>(), 1);
            recipe.AddIngredient(ModContent.ItemType<FullMoonBar>(), 1);
            recipe.AddIngredient(ItemID.RangerEmblem, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}