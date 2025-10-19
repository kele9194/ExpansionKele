using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonSorcererEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float MagicDamageBonus = 0.15f; // +15%魔法伤害
        private const float MagicCritBonus = 0.05f; // +15%魔法伤害
        private const float ManaBonus = 60f; // +60魔力上限
        private const float ManaRegenBonus = 2f; // +2魔力恢复速度
        private const float ManaCostReduction = 0.08f; // -8%蓝耗
        private const float PotionEffectIncrease = 0.25f; // 喝药效果增加25%
        private const float DamageToManaRatio = 1.5f; // 每1%额外魔法伤害增加1.5最大蓝量
        private const float DamageToCostReduction = 0.001f; // 每1%额外魔法伤害减少0.1%蓝耗
        private const float MaxCostReduction = 0.30f; // 蓝耗最多减少30%
         private const float MaxLowManaDamageBonus = 0.20f; // 低蓝量时最高增加10%魔法伤害

        public override void SetDefaults()
        {
            //Item.SetNameOverride("巫师满月徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.rare = ItemRarityID.Cyan;
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
            // +15%魔法伤害
            player.GetDamage(DamageClass.Magic) += MagicDamageBonus;
            player.GetCritChance(DamageClass.Magic) += MagicCritBonus*100;
            
            // +60魔力上限
            player.statManaMax2 += (int)ManaBonus;
            
            // +2魔力恢复速度
            player.manaRegen += (int)ManaRegenBonus;
            
            // -8%蓝耗
            player.manaCost -= ManaCostReduction;
            if (player.manaCost < 0) player.manaCost = 0;
            
            // 允许自动喝药
            player.manaFlower = true;
            
            // 喝药效果增加25%
            
            // 每1%额外魔法伤害增加1.5最大蓝量和减少0.1%蓝耗
            float additionalMagicDamage = player.GetDamage(DamageClass.Magic).Additive - 1f;
            additionalMagicDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.statManaMax2 += (int)(additionalMagicDamage * 100 * DamageToManaRatio);
            
            // 蓝耗减少最多累加到30%
            float costReduction = additionalMagicDamage * 100 * DamageToCostReduction;
            if (costReduction > MaxCostReduction) {
                costReduction = MaxCostReduction;
            }
            player.manaCost -= costReduction;
            if (player.manaCost < 0) player.manaCost = 0;

            if (player.statManaMax2 > 0)
            {
                float manaRatio = (float)player.statMana / player.statManaMax2;
                float damageBonus = (1 - manaRatio) * MaxLowManaDamageBonus;
                player.GetDamage(DamageClass.Magic) += damageBonus;
            }
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonSorcererEmblemDamage", $"[c/00FF00:+{MagicDamageBonus * 100}%魔法伤害]"},
                    {"MoonSorcererEmblemMana", $"[c/00FF00:+{ManaBonus}魔力上限]"},
                    {"MoonSorcererEmblemRegen", $"[c/00FF00:+{ManaRegenBonus}魔力恢复速度]"},
                    {"MoonSorcererEmblemCost", $"[c/00FF00:-{ManaCostReduction * 100}%蓝耗]"},
                    {"MoonSorcererEmblemLowMana", $"[c/00FF00:蓝量越低魔法伤害越高，最多+{MaxLowManaDamageBonus * 100}%]"},
                    {"MoonSorcererEmblemAuto", "[c/00FF00:允许自动喝药]"},
                    {"MoonSorcererEmblemBonus", $"[c/00FF00:每1%额外魔法伤害增加{DamageToManaRatio}最大蓝量和减少0.1%蓝耗(蓝耗减少最多累加到{MaxCostReduction * 100}%)]"},
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
            recipe.AddIngredient(ModContent.ItemType<MysteryCrystal>(), 1);
            recipe.AddIngredient(ModContent.ItemType<FullMoonBar>(), 1);
            recipe.AddIngredient(ItemID.SorcererEmblem, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}