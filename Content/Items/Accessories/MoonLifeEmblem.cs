using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonLifeEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        private const int BaseLifeMaxBonus = 60;
        private const float LifeMaxMultiplier = 1.1f;
        private const float LifeRegenThreshold = 0.5f;
        private const float HealThreshold = 0.2f;
        private const float LifeRegenProbability = 0.02f;
        private const int HealInterval = 30;

        private const int HealAmount = 1;
        
        public override void SetDefaults()
        {
            //Item.SetNameOverride("满月生命徽章");
            Item.width = 32;
            Item.height = 36;
            Item.value = Item.buyPrice(0, 50, 0, 0);
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
            // 应用生命值加成
            player.statLifeMax2 += BaseLifeMaxBonus; // +60最大生命值
            player.statLifeMax2 = (int)(player.statLifeMax2 * LifeMaxMultiplier); // +10%最大生命值

            // 获取玩家当前生命百分比
            float lifePercentage = (float)player.statLife / player.statLifeMax2;

            // 当生命值低于50%时，每减少1%的血量，生命回复时间有2%的概率增加1
            if (lifePercentage < LifeRegenThreshold)
            {
                // 计算低于50%的部分（以百分比为单位）
                float belowThreshold = LifeRegenThreshold - lifePercentage;
                // 转换为百分点（例如：低于50%的20%时，为0.2）
                float percentBelow = belowThreshold * 100f;
                
                // 每1%血量有2%概率增加1点生命回复时间
                // 我们使用一个简单的判定方法
                    if (Main.rand.NextFloat() < LifeRegenProbability * percentBelow) // 2%概率
                    {
                        player.lifeRegenTime++; // 增加生命回复时间
                    }
            }

            // 当生命值低于20%时，每过30帧回复1生命值
            if (lifePercentage < HealThreshold)
            {
                if (++player.GetModPlayer<FullMoonLifeEmblemPlayer>().lowHealthTimer >= HealInterval)
                {
                    player.GetModPlayer<FullMoonLifeEmblemPlayer>().lowHealthTimer = 0;

                        player.Heal(HealAmount);
                }
            }
        }

         public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LifeCrystal, 3);
            recipe.AddIngredient(ModContent.ItemType<FullMoonBar>(), 3);
            recipe.AddIngredient(ItemID.WarriorEmblem, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        
        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"LifeMaxBonus1", $"[c/00FF00:+{BaseLifeMaxBonus} 最大生命值]"},
                    {"LifeMaxBonus2", $"[c/00FF00:+{(LifeMaxMultiplier - 1f) * 100}% 最大生命值]"},
                    {"LifeRegenBonus", $"[c/00FF00:生命值低于{LifeRegenThreshold * 100}%时，每减少1%生命值，生命回复时间有{LifeRegenProbability * 100}%概率增加1]"},
                    {"LowHealthHeal", $"[c/00FF00:生命值低于{HealThreshold * 100}%时，每过{HealInterval/60f}s回复{HealAmount}生命值]"},
                    {"WARNING", "[c/800000:注意：多个满月徽章装备将只有第一个生效]"}
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }
// ... existing code ...
    }

    public class FullMoonLifeEmblemPlayer : ModPlayer
    {
        public int lowHealthTimer = 0;

        public override void ResetEffects()
        {
            lowHealthTimer = 0;
        }
    }
}