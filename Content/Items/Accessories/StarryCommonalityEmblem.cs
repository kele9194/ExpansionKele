using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryCommonalityEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        public const float DamageBonus = 0.15f; // 20% 乘算增伤
        public const int CriticalBonus = 8; // 10% 暴击率
        public const float AttackSpeedBonus = 0.15f; // 15% 攻击速度
        public const int DefenseBonus = 20; // 10 防御力
        public const float DamageReduction = 0.08f; // 10% 自定义减伤

        public override void SetDefaults()
        {
            //Item.SetNameOverride("星元徽章");
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


            // 应用所有增益效果，由玩家修饰类判断是否应该应用
            player.GetModPlayer<StarryEmblemPlayer>().ApplyCommonalityEffects();
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonCommonalityEmblemHolding", "[c/00FF00:装备时:]"},
                    {"StarryCommonalityEmblemDamage", $"[c/00FF00:+{DamageBonus * 100}%乘算增伤]"},
                    {"StarryCommonalityEmblemCrit", $"[c/00FF00:+{CriticalBonus}%暴击率]"},
                    {"StarryCommonalityEmblemSpeed", $"[c/00FF00:+{AttackSpeedBonus * 100}%攻击速度]"},
                    {"StarryCommonalityEmblemDefense", $"[c/00FF00:+{DefenseBonus}防御力]"},
                    {"StarryCommonalityEmblemReduction", $"[c/00FF00:+{DamageReduction * 100}%自定义伤害减免]"},
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
            recipe.AddIngredient(ModContent.ItemType<MoonCommonalityEmblem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }

    public class StarryEmblemPlayer : ModPlayer
    {
        // 跟踪是否装备了饰品
        public bool HasCommonalityEmblem = false;

        // 效果数值
        public float CommonalityDamageBonus = 0f;
        public int CommonalityCriticalBonus = 0;
        public float CommonalityAttackSpeedBonus = 1f;
        public int CommonalityDefenseBonus = 0;
        public float CommonalityDamageReduction = 0f;

        public override void ResetEffects()
        {
            HasCommonalityEmblem = false;

            CommonalityDamageBonus = 0f;
            CommonalityCriticalBonus = 0;
            CommonalityAttackSpeedBonus = 1f;
            CommonalityDefenseBonus = 0;
            CommonalityDamageReduction = 0f;
        }

        public void ApplyCommonalityEffects()
        {
            // 检查当前武器是否为非标准伤害类型

                HasCommonalityEmblem = true;

                // 累加效果数值
                CommonalityDamageBonus += StarryCommonalityEmblem.DamageBonus;
                CommonalityCriticalBonus += StarryCommonalityEmblem.CriticalBonus;
                CommonalityAttackSpeedBonus +=StarryCommonalityEmblem.AttackSpeedBonus;
                CommonalityDefenseBonus += StarryCommonalityEmblem.DefenseBonus;
                CommonalityDamageReduction += StarryCommonalityEmblem.DamageReduction;

                // 应用效果
                ApplyEffects();
            
        }

        private void ApplyEffects()
        {
            // 应用乘算伤害加成（来自星元徽章）
            if (HasCommonalityEmblem)
            {
                var damageMultiPlayer = Player.GetModPlayer<ExpansionKeleDamageMulti>();
                damageMultiPlayer.AddMultiplicativeDamageBonus(CommonalityDamageBonus);

                // 增加暴击率
                Player.GetCritChance<GenericDamageClass>() += CommonalityCriticalBonus;

                // 增加攻击速度
                Player.GetModPlayer<AttackSpeedBoosterOther>().AttackSpeedBoosterEquipped = true;
                Player.GetModPlayer<AttackSpeedBoosterOther>().attackSpeedBoosterMultiplier *= CommonalityAttackSpeedBonus;

                // 增加防御力
                Player.statDefense += CommonalityDefenseBonus;

                // 增加自定义减伤
                var reductionPlayer = Player.GetModPlayer<CustomDamageReductionPlayer>();
                reductionPlayer.AddCustomDamageReduction(CommonalityDamageReduction);
            }
        }

        
    }
}
