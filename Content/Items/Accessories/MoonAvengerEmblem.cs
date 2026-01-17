using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class MoonAvengerEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 9% 乘算增伤
        public const float MultiplicativeDamageBonus = 0.09f;
        
        // 6% 伤害加成
        public const float AdditiveDamageBonus = 0.06f;

        public override void SetDefaults()
        {
            //Item.SetNameOverride("满月复仇徽章");
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
            // 应用满月复仇者徽章效果
            player.GetModPlayer<MoonEmblemPlayer>().ApplyCommonalityEffects();

            var avengerPlayer = player.GetModPlayer<AvengerPlayer>();
            avengerPlayer.HasAvengerEmblem = true;
            avengerPlayer.ApplyDamageBonus();
            var starryEmblemPlayer = player.GetModPlayer<StarryEmblemPlayer>();
            if (!starryEmblemPlayer.HasCommonalityEmblem)
            {
                ExpansionKeleTool.AddDamageBonus(player, avengerPlayer.StarryTimer / 60f * avengerPlayer.TimerDamageBonus);
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
                    {"Note1", "[c/00FF00:继承满月徽章效果但不与满月徽章叠加]"},
                    {"Note2", "[c/00FF00:下列为复仇效果：]"},
                    {"SpecialEffect1", $"[c/00FF00:装备起内置计时器,每过1s增加{ModContent.GetInstance<AvengerPlayer>().TimerDamageBonus * 60}%乘算增伤]"},
                    {"SpecialEffect2", $"[c/00FF00:计时器最多{AvengerPlayer.MaxStarryTimer/60}s，受伤后重置计时器]"},
                    {"SpecialEffect3", $"[c/00FF00:受伤后获得星元焰火减益持续{AvengerPlayer.FireworkDebuffDuration/60}s]"},
                    {"SpecialEffect4", $"[c/00FF00:星元焰火减益减少玩家{AvengerPlayer.FireworkDefenseReduction*100}%的防御"+
                    $"并使玩家额外遭受{AvengerPlayer.FireworkCustomDefenseReduction*100}%的伤害]"},
                    {"WARNING", "[c/800000:注意：多个满月徽章装备于饰品栏将只有第一个生效]"}
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
            recipe.AddIngredient(ModContent.ItemType<SigwutBar>(), 2);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}