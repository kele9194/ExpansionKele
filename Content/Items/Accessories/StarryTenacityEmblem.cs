using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryTenacityEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const int BaseDefenseBonus = 20;
        private const float DefensePercentBonus = 0.15f; // 15%
        private const float DamageReduction = 0.10f; // 10%
        private const float BonusPerTenDefense = 0.01f; // 1%
        private const float MaxBonus = 0.20f; // 20% 最大加成
        private const float DamageReductionAfterHit = 0.20f; // 受伤后20%减伤
        private const float DefenseAfterHitPercent = 0.15f; // 受伤后15%防御
        private const int EffectDuration = 600; // 600帧效果持续时间

        public override void SetDefaults()
        {
            //Item.SetNameOverride("坚韧星元徽章");
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


            // 基础防御加成
            player.statDefense += BaseDefenseBonus;
            
            // 百分比防御加成
            player.statDefense += (int)(player.statDefense * DefensePercentBonus);
            
            // 自定义伤害减免
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            reductionPlayer.AddCustomDamageReduction(DamageReduction);
            
            // 根据防御力提供额外加成
            int defense = player.statDefense;
            float defenseBonusTiers = defense / 10f;
            float bonus = defenseBonusTiers * BonusPerTenDefense;
            
            // 限制最大加成
            if (bonus > MaxBonus)
                bonus = MaxBonus;
                
            // 应用伤害加成
            var damageMultiPlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            damageMultiPlayer.AddMultiplicativeDamageBonus(bonus);
            
            // 应用额外减伤
            reductionPlayer.AddCustomDamageReduction(bonus);
            
            // 免疫击退效果
            player.noKnockback = true;
            
            // 获取坚韧徽章玩家数据
            var tenacityPlayer = player.GetModPlayer<StarryTenacityEmblemPlayer>();
            
            // 应用受伤后的减伤和防御加成（如果效果还在持续）
            if (tenacityPlayer.effectTimer > 0)
            {
                // 线性递减效果
                float effectMultiplier = (float)tenacityPlayer.effectTimer / EffectDuration;
                
                // 应用减伤
                reductionPlayer.AddCustomDamageReduction(DamageReductionAfterHit * effectMultiplier);
                
                // 应用防御加成
                player.statDefense += (int)(tenacityPlayer.contactDamageDefense * effectMultiplier);
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
                    {"StarryTenacityEmblemDefenseBase", $"[c/00FF00:+{BaseDefenseBonus}+{DefensePercentBonus * 100}%防御力]"},
                    {"StarryTenacityEmblemReduction", $"[c/00FF00:+{DamageReduction * 100}%自定义伤害减免]"},
                    {"StarryTenacityEmblemScaling", $"[c/00FF00:每10点防御力增加{BonusPerTenDefense * 100}%伤害和减伤]"},
                    {"StarryTenacityEmblemMax", $"[c/00FF00:最多增加{MaxBonus * 100}%伤害和减伤]"},
                    {"StarryTenacityEmblemKnockback", "[c/00FF00:免疫击退]"},
                    {"StarryTenacityEmblemHitEffect", $"[c/00FF00:受伤后获得{EffectDuration/60}秒逐渐减少的{DamageReductionAfterHit * 100}%减伤]"},
                    {"StarryTenacityEmblemDefenseBoost", $"[c/00FF00:和接触伤害{DefenseAfterHitPercent * 100}%的防御值]"},
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
            recipe.AddIngredient(ModContent.ItemType<MoonTenacityEmblem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }

    public class StarryTenacityEmblemPlayer : ModPlayer
    {
        public int effectTimer = 0;
        public int contactDamageDefense = 0;
        
        private const int EffectDuration = 600; // 600帧效果持续时间
        private const float DefenseAfterHitPercent = 0.15f; // 受伤后15%防御

        public override void ResetEffects()
        {
            // 每帧减少效果计时器
            if (effectTimer > 0)
            {
                effectTimer--;
            }
        }

        // ... existing code ...

        public override void OnHurt(Player.HurtInfo info)
        {
            // 受到伤害时重置效果计时器并设置防御值
            effectTimer = EffectDuration;
            contactDamageDefense = (int)(info.Damage * DefenseAfterHitPercent);
        }
// ... existing code ...
    }
}