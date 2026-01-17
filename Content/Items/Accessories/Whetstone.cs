using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.Accessories
{
    public class Whetstone : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float AttackSpeedBonus = 0.1f; // 10% 攻击速度加成
        private const float DamageToSpeedRatio = 0.3f; // 每1%额外近战伤害增加的攻击速度百分比

        public override void SetDefaults()
        {
            //Item.SetNameOverride("磨刀石");
            Item.width = 24;
            Item.height = 24;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +10%攻击速度
            player.GetAttackSpeed(DamageClass.Melee) += AttackSpeedBonus;

            // 每1%额外近战伤害增加0.3%攻速
            float additionalMeleeDamage = player.GetDamage(DamageClass.Melee).Additive - 1f;
            additionalMeleeDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            player.GetAttackSpeed(DamageClass.Melee) += additionalMeleeDamage * DamageToSpeedRatio;

            // 允许自动挥舞
            player.autoReuseGlove = true;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"WhetstoneSpeed", $"[c/00FF00:+{AttackSpeedBonus * 100}%近战攻速]"},
                    {"WhetstoneBonus", $"[c/00FF00:每1%额外近战伤害增加{DamageToSpeedRatio}%攻速]"},
                    {"WhetstoneAuto", "[c/00FF00:允许自动挥舞]"}
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
            recipe.AddIngredient(ItemID.FeralClaws, 1);
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}