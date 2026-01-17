using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonSummonerEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float SummonDamageBonus = 0.14f; // +14%召唤伤害
        private const float CritChanceBonus = 0.02f; // +5%暴击率
        private const int MinionSlotBonus = 2; // +2召唤栏位
        private const float WhipRangeBonus = 0.12f; // +12%鞭子范围
        private const float WhipSpeedBonus = 0.08f; // +8%鞭子攻速
        private const float DamageToWhipRangeRatio = 0.5f; // 每1%额外召唤伤害增加的鞭子范围百分比
        private const float DamageToWhipSpeedRatio = 0.4f; // 每1%额外召唤伤害增加的鞭子攻速百分比

        public override void SetDefaults()
        {
            //Item.SetNameOverride("召唤师满月徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
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
            // +14%召唤伤害
            player.GetDamage(DamageClass.Summon) += SummonDamageBonus;
            
            // +5%暴击率
            player.GetCritChance(DamageClass.Generic) += CritChanceBonus * 100;
            
            // +2召唤栏位
            player.maxMinions += MinionSlotBonus;
            
            // +12%鞭子范围
            player.whipRangeMultiplier += WhipRangeBonus;
            
            // +8%鞭子攻速
            player.GetAttackSpeed(DamageClass.Summon) += WhipSpeedBonus;
            
            // 允许将暴击率按1:1增加到召唤伤害
            player.GetDamage(DamageClass.Summon) += player.GetCritChance(DamageClass.Generic) / 100f;
            
            // 每1%额外召唤伤害增加0.5%鞭子范围和0.4%鞭子攻速
            float additionalSummonDamage = player.GetDamage(DamageClass.Summon).Additive - 1f;
            additionalSummonDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.whipRangeMultiplier += additionalSummonDamage * DamageToWhipRangeRatio;
            player.GetAttackSpeed(DamageClass.Summon) += additionalSummonDamage * DamageToWhipSpeedRatio;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonSummonerEmblemDamage", $"[c/00FF00:+{SummonDamageBonus * 100}%召唤伤害]"},
                    {"MoonSummonerEmblemCrit", $"[c/00FF00:+{CritChanceBonus * 100}%暴击率]"},
                    {"MoonSummonerEmblemMinion", $"[c/00FF00:+{MinionSlotBonus}召唤栏位]"},
                    {"MoonSummonerEmblemWhipRange", $"[c/00FF00:+{WhipRangeBonus * 100}%鞭子范围]"},
                    {"MoonSummonerEmblemWhipSpeed", $"[c/00FF00:+{WhipSpeedBonus * 100}%鞭子攻速]"},
                    {"MoonSummonerEmblemCritToDamage", "[c/00FF00:允许将暴击率按1:1增加到召唤伤害]"},
                    {"MoonSummonerEmblemBonus", $"[c/00FF00:每1%额外召唤伤害增加{DamageToWhipRangeRatio}%鞭子范围和{DamageToWhipSpeedRatio}%鞭子攻速]"},
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
            recipe.AddIngredient(ModContent.ItemType<LoyalScroll>(), 1);
            recipe.AddIngredient(ModContent.ItemType<FullMoonBar>(), 1);
            recipe.AddIngredient(ItemID.SummonerEmblem, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}