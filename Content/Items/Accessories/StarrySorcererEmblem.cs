using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarrySorcererEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float MagicDamageBonus = 0.15f; // +15%魔法伤害
        private const float MagicCritBonus = 0.06f;
        private const float ManaBonus = 150f; // +150法力上限
        private const float ManaCostReduction = 0.25f; // -25%蓝耗
        private const float PotionEffectIncrease = 0.30f; // 喝药效果增加30%
        private const float DamageToManaRatio = 1.8f; // 每1%额外魔法伤害增加1.8最大蓝量
        private const float DamageToCostReduction = 0.001f; // 每1%额外魔法伤害减少0.1%蓝耗
        private const float MaxCostReduction = 0.30f; // 蓝耗最多减少30%
        private const float ManaSicknessReduction = 0.5f; // 魔力病刷新时间加快为原来的2倍（减少50%）
         private const float MaxLowManaDamageBonus = 0.20f; // 低蓝量时最高增加10%魔法伤害

        public override void SetDefaults()
        {
            //Item.SetNameOverride("智慧星元徽章");
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
            if (modPlayer.activeStarryEmblemType != -1 && 
                modPlayer.activeStarryEmblemType != Item.type)
                return; // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeStarryEmblemType = Item.type;


            // +15%魔法伤害
            player.GetDamage(DamageClass.Magic) += MagicDamageBonus;

            
            player.GetCritChance(DamageClass.Magic) += MagicCritBonus*100;
            
            // +150法力上限
            player.statManaMax2 += (int)ManaBonus;
            
            // -25%蓝耗
            player.manaCost -= ManaCostReduction;
            if (player.manaCost < 0) player.manaCost = 0;
            
            // 允许自动喝蓝
            player.manaFlower = true;
            
            // 喝药效果增加30%
            
            // 魔力病刷新时间加快为原来的2倍
            player.manaSickReduction *= (1 - ManaSicknessReduction);
            
            // 每1%额外魔法伤害增加1.8最大蓝量和减少0.1%蓝耗
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
            PlayerUtils.ReduceManaSicknessDuration(player);
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"StarrySorcererEmblemDamage", $"[c/00FF00:+{MagicDamageBonus * 100}%魔法伤害]"},
                    {"StarrySorcererEmblemCrit", $"[c/00FF00:+{MagicCritBonus * 100}%魔法暴击]"},
                    {"StarrySorcererEmblemMana", $"[c/00FF00:+{ManaBonus}法力上限]"},
                    {"StarrySorcererEmblemCost", $"[c/00FF00:-{ManaCostReduction * 100}%蓝耗]"},
                    {"StarrySorcererEmblemAuto", "[c/00FF00:允许自动喝蓝]"},
                    {"StarrySorcererEmblemLowMana", $"[c/00FF00:蓝量越低魔法伤害越高，最多+{MaxLowManaDamageBonus * 100}%]"},
                    {"StarrySorcererEmblemSickness", "[c/00FF00:魔力病刷新时间加快,效果减弱]"},
                    {"StarrySorcererEmblemBonus", $"[c/00FF00:每1%额外魔法伤害增加{DamageToManaRatio}最大蓝量和减少0.1%蓝耗(蓝耗减少最多累到{MaxCostReduction * 100}%)]"},
                    {"WARNING", "[c/800000:注意：多个星元徽章装备将只有第一个生效]"}
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
            recipe.AddIngredient(ModContent.ItemType<MoonSorcererEmblem>(), 1);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}