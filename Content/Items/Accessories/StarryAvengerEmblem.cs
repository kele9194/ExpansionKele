using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using System;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    public class StarryAvengerEmblem : ModItem
    {
        public override string LocalizationCategory => "Items.Accessories";
        // 18% 伤害加成
        public const float AdditiveDamageBonus = 0.18f;
        
        // 12% 乘算增伤
        public const float MultiplicativeDamageBonus = 0.12f;
        
        // 40 全属性穿甲
        public const int ArmorPenetrationBonus = 40;
        
        // 25 面板伤害
        public const int FlatDamageBonus = 25;
        public const float TimerDamageBonusPlus=0.004f;
        float totalDamageBonus = (TimerDamageBonusPlus + ModContent.GetInstance<AvengerPlayer>().TimerDamageBonus) * 60;
        int tooltipBonus=(int)(TimerDamageBonusPlus / ModContent.GetInstance<AvengerPlayer>().TimerDamageBonus *100);

        public override void SetDefaults()
        {
            //Item.SetNameOverride("力量星元徽章");
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
                modPlayer.activeStarryEmblemType != Item.type){
                    //添加提示
                    return; 
                }
                // 如果已经有其他类型的徽章生效了，则直接返回，不应用效果
                
            // 标记当前玩家已经有星元魔法师徽章生效
            modPlayer.activeStarryEmblemType = Item.type;


            player.GetModPlayer<StarryEmblemPlayer>().ApplyCommonalityEffects();
            
            // 应用星元徽章特殊效果
            var avengerPlayer = player.GetModPlayer<AvengerPlayer>();
            avengerPlayer.TimerDamageBonus =(TimerDamageBonusPlus + ModContent.GetInstance<AvengerPlayer>().TimerDamageBonus);
            avengerPlayer.HasAvengerEmblem = true;
            ExpansionKeleTool.AddDamageBonus(player, avengerPlayer.StarryTimer / 60f * avengerPlayer.TimerDamageBonus);

        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"Note1", "[c/00FF00:继承星元徽章效果但不与星元徽章叠加]"},
                    {"Note2", $"[c/00FF00:继承满月复仇徽章的复仇效果,但提升乘算增伤效果增加{tooltipBonus}%]"},
                    {"Note3", "[c/00FF00:两者复仇效果不叠加]"},
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
            recipe.AddIngredient(ModContent.ItemType<MoonAvengerEmblem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<StarryBar>(), 3);
            recipe.AddIngredient(ItemID.LunarBar, 3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
    
    public class AvengerPlayer : ModPlayer
    {
        public bool HasAvengerEmblem = false;
        public int StarryTimer = 0;
        public const int MaxStarryTimer = 3000; // 最大3000帧计时器
        public const int FireworkDebuffDuration = 180; // 星元焰火减益持续180帧

        public const float FireworkDefenseReduction = 0.5f;
        public const float FireworkCustomDefenseReduction = 0.5f;
        public float TimerDamageBonus = 0.006f;

        public override void ResetEffects()
        {
            HasAvengerEmblem = false;
        }
        
        public override void PostUpdate()
        {
            if (HasAvengerEmblem)
            {
                // 每10帧增加计时器
                if (StarryTimer < MaxStarryTimer)
                {
                    StarryTimer++;
                }
            }
            else
            {
                // 没有装备时重置计时器
                StarryTimer = 0;
            }
        }
        
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasAvengerEmblem)
            {
                // 受伤后重置计时器
                StarryTimer = 0;
                
                // 添加星元焰火减益
                Player.AddBuff(ModContent.BuffType<StarryFireworkBuff>(), FireworkDebuffDuration);
            }
        }
        
        public void ApplyDamageBonus()
        {
            if (HasAvengerEmblem)
            {
                
            }
        }
    }
    
    
}