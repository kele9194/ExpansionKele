using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonWarriorEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 定义常量
        private const float MeleeDamageBonus = 0.12f; // 12% 近战伤害加成
        private const float AttackSpeedBonus = 0.08f; // 8% 攻击速度加成
        private const float DamageToSpeedRatio = 0.26f; // 每1%额外近战伤害增加的攻击速度百分比
        private const float SpeedModeMultiplier = 1.0f; // 攻速模式下额外攻速收益倍率
        private const float NonSpeedModeMultiplier = 0.4f; // 非攻速模式下额外攻速收益倍率
        private const float NonSpeedModeConversionRate = 0.5f; // 非攻速模式下转换比例

        public override void SetDefaults()
        {
            //Item.SetNameOverride("战士满月徽章");
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                ExpansionKelePlayer modPlayer = player.GetModPlayer<ExpansionKelePlayer>();
                modPlayer.moonWarriorEmblemSpeedMode = !modPlayer.moonWarriorEmblemSpeedMode;
                
                if (modPlayer.moonWarriorEmblemSpeedMode)
                {
                    Main.NewText("已切换至攻速模式", Color.Green);
                }
                else
                {
                    Main.NewText("已切换至非攻速模式", Color.Orange);
                }
            }
            return true;
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
            // +12%近战伤害
            player.GetDamage(DamageClass.Melee) += MeleeDamageBonus;
            
            // +8%攻击速度
            player.GetAttackSpeed(DamageClass.Melee) += AttackSpeedBonus;
            
            // 每1%额外近战伤害增加0.26%攻速
            float additionalMeleeDamage = player.GetDamage(DamageClass.Melee).Additive - 1f;
            additionalMeleeDamage+=player.GetDamage(DamageClass.Generic).Additive-1;
            
            if (modPlayer.moonWarriorEmblemSpeedMode)
            {
                // 攻速模式
                player.GetAttackSpeed(DamageClass.Melee) += additionalMeleeDamage * DamageToSpeedRatio * SpeedModeMultiplier;
            }
            else
            {
                // 非攻速模式
                // 减少额外攻速收益到原来的0.4倍
                player.GetAttackSpeed(DamageClass.Melee) += additionalMeleeDamage * DamageToSpeedRatio * NonSpeedModeMultiplier;
                
                // 将剩余的0.6倍中的一半转化为额外近战伤害加成
                float convertedDamageBonus = additionalMeleeDamage * DamageToSpeedRatio * (1 - NonSpeedModeMultiplier) * NonSpeedModeConversionRate;
                player.GetDamage(DamageClass.Melee) += convertedDamageBonus;
            }
            
            // 允许自动挥舞
            player.autoReuseGlove = true;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                ExpansionKelePlayer modPlayer = Main.LocalPlayer.GetModPlayer<ExpansionKelePlayer>();
                string modeText = modPlayer.moonWarriorEmblemSpeedMode ? "攻速模式" : "非攻速模式";
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoonWarriorEmblemDamage", $"[c/00FF00:+{MeleeDamageBonus * 100}%近战伤害]"},
                    {"MoonWarriorEmblemSpeed", $"[c/00FF00:+{AttackSpeedBonus * 100}%攻击速度]"},
                    {"MoonWarriorEmblemBonus", $"[c/00FF00:每1%额外近战伤害增加{DamageToSpeedRatio}%攻击速度]"},
                    {"MoonWarriorEmblemMode", $"[c/00FF00:当前模式: {modeText} (手持使用以切换)]"},
                    {"MoonWarriorEmblemAuto", "[c/00FF00:允许自动挥舞]"},
                    {"WARNING", "[c/800000:注意：多个满月徽章装备将只有第一个生效]"}
                };
                
                // 如果是非攻速模式，添加额外说明
                if (!modPlayer.moonWarriorEmblemSpeedMode)
                {
                    tooltipData.Add("MoonWarriorEmblemNonSpeedDetail1", "[c/00FF00:非攻速模式下额外攻击造成的近战攻速收益减少为原来的40%]");
                    tooltipData.Add("MoonWarriorEmblemNonSpeedDetail2", "[c/00FF00:并将剩余60%攻速收益的一半转化为额外近战伤害加成]");
                }

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
            recipe.AddIngredient(ModContent.ItemType<Whetstone>(), 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}