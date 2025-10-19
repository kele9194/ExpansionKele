using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MysteryCrystal : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float ManaBonus = 40f; // +40魔力上限
        private const float ManaRegenBonus = 2f; // +2魔力恢复速度
        private const float MagicCritBonus = 0.06f;
        private const float ManaCostReduction = 0.10f; // -10%蓝耗
        private const float PotionEffectIncrease = 0.30f; // 喝药效果增加30%
        private const float DamageToManaRatio = 1.8f; // 每1%额外魔法伤害增加1.8最大蓝量
        private const float DamageToCostReduction = 0.001f; // 每1%额外魔法伤害减少0.1%蓝耗
        private const float MaxCostReduction = 0.30f; // 蓝耗最多减少30%
        private const float MaxLowManaDamageBonus = 0.20f; // 低蓝量时最高增加10%魔法伤害

        public override void SetDefaults()
        {
            //Item.SetNameOverride("奥秘魔晶");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetCritChance(DamageClass.Magic) += (int)(MagicCritBonus * 100);
            // +40魔力上限
            player.statManaMax2 += (int)ManaBonus;
            
            // +2魔力恢复速度
            player.manaRegen += (int)ManaRegenBonus;
            
            // -10%蓝耗
            player.manaCost -= ManaCostReduction;
            if (player.manaCost < 0) player.manaCost = 0;
            
            // 允许自动喝药
            player.manaFlower = true;
            
            // 喝药效果增加30%
            
            // 每1%额外魔法伤害增加1.8最大蓝量和减少0.1%蓝耗
            float additionalMagicDamage = player.GetDamage(DamageClass.Magic).Additive - 1f;
            additionalMagicDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.statManaMax2 += (int)(additionalMagicDamage * 100 * DamageToManaRatio);
            
            // 蓝耗减少最多累加到30%
            float costReduction = additionalMagicDamage * 100 * DamageToCostReduction;
            if (costReduction > MaxCostReduction){
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
                    {"MysteryCrystalMana", $"[c/00FF00:+{ManaBonus}魔力上限]"},
                    {"MysteryCrystalRegen", $"[c/00FF00:+{ManaRegenBonus}魔力恢复速度]"},
                    {"MysteryCrystalCost", $"[c/00FF00:-{ManaCostReduction * 100}%蓝耗]"},
                    {"MysteryCrystalLowMana", $"[c/00FF00:蓝量越低魔法伤害越高，最多+{MaxLowManaDamageBonus * 100}%]"},
                    {"MysteryCrystalAuto", "[c/00FF00:允许自动喝药]"},
                    {"MysteryCrystalBonus", $"[c/00FF00:每1%额外魔法伤害增加{DamageToManaRatio}最大蓝量和减少0.1%蓝耗(蓝耗减少累加到{MaxCostReduction * 100}%)]"}
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
            recipe.AddIngredient(ItemID.ManaFlower, 1);
            recipe.AddIngredient(ItemID.ManaCrystal, 2);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 1);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
    }
}