using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Accessories
{
    public class LoyalScroll : ModItem
    {
        // 定义常量
        private const float SummonDamageBonus = 0.06f; // +6%召唤伤害
        private const float CritChanceBonus = 0.06f; // +6%暴击率
        private const int MinionSlotBonus = 1; // +1召唤栏
        private const float WhipRangeBonus = 0.1f; // +10%鞭子范围
        private const float WhipSpeedBonus = 0.04f; // +4%鞭子攻速
        private const float DamageToWhipRangeRatio = 0.5f; // 每1%额外召唤伤害增加的鞭子范围百分比
        private const float DamageToWhipSpeedRatio = 0.4f; // 每1%额外召唤伤害增加的鞭子攻速百分比
        public override string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            //Item.SetNameOverride("忠诚卷轴");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +6%召唤伤害
            player.GetDamage(DamageClass.Summon) += SummonDamageBonus;
            
            // +6%暴击率
            player.GetCritChance(DamageClass.Generic) += CritChanceBonus * 100;
            
            // +1召唤栏
            player.maxMinions += MinionSlotBonus;
            
            // +10%鞭子范围
            player.whipRangeMultiplier += WhipRangeBonus;
            
            // +4%鞭子攻速
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
        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"LoyalScrollDamage", $"[c/00FF00:+{SummonDamageBonus * 100}%召唤伤害]"},
                    {"LoyalScrollCrit", $"[c/00FF00:+{CritChanceBonus * 100}%暴击率]"},
                    {"LoyalScrollMinion", $"[c/00FF00:+{MinionSlotBonus}召唤栏]"},
                    {"LoyalScrollWhipRange", $"[c/00FF00:+{WhipRangeBonus * 100}%鞭子范围]"},
                    {"LoyalScrollWhipSpeed", $"[c/00FF00:+{WhipSpeedBonus * 100}%鞭子攻速]"},
                    {"LoyalScrollCritToDamage", "[c/00FF00:允许将暴击率按1:1增加到召唤伤害]"},
                    {"LoyalScrollBonus", $"[c/00FF00:每1%额外召唤伤害增加{DamageToWhipRangeRatio * 100}%鞭子范围和{DamageToWhipSpeedRatio * 100}%鞭子攻速]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...
// ... existing code ...

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PygmyNecklace);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 1);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
    }
}