using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Buff;

namespace ExpansionKele.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)] // Load the spritesheet you create as a shield for the player when it is equipped.
    public class HallowHeartShield : ModItem
    {
        private const float unit=0.04f;
        private const float MaxSpeedBonus=0.3f;
        
        private const float SpeedBonus=-0.01f;
        private const float EnduranceBonus=0.003f;

        private const int DefenseBonus=1;
        private const string setNameOverride="神圣心盾";
        private const int LifeMaxBonus=60;
        private int counter;
        private static int MaxCounter=1200;
        private static float ImmuneCounter=150;
        public override string LocalizationCategory => "Items.Accessories";


        
        public override void SetDefaults()
        {
            //Item.SetNameOverride(setNameOverride);
            Item.width = 24;
            Item.height = 28;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;

            
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 基础移速加成30%
            player.moveSpeed += MaxSpeedBonus;
            player.statLifeMax2 +=LifeMaxBonus;

            // 计算最大生命值减少的百分比
            float maxLifeReducedPercentage = 1f - (player.statLife / (float)player.statLifeMax2);
            if(maxLifeReducedPercentage <0){
                maxLifeReducedPercentage = 0;
            }


            // 每减少3%最大生命值，增加1点防御和0.4%减伤，减少1%移速加成
            int lifeReducedIn3PercentSteps = (int)(maxLifeReducedPercentage / unit);
            //Main.NewText($"{lifeReducedIn3PercentSteps}");
            player.statDefense += DefenseBonus*lifeReducedIn3PercentSteps;
            ExpansionKeleTool.AddDamageReduction(player,EnduranceBonus*lifeReducedIn3PercentSteps);
            player.moveSpeed += SpeedBonus * lifeReducedIn3PercentSteps;
            //Main.NewText($"Defense:{player.statDefense},endurance:{player.endurance},SpeedBonus:{player.moveSpeed}");

            player.GetModPlayer<HallowHeartShieldPlayer>().hasHallowHeartShield = true;
            
        }
       // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MaxSpeedBonus", $"[c/00FF00:基础移速增加 {MaxSpeedBonus*100}%]"},
                    {"LifeMaxBonus", $"[c/00FF00:最大生命值增加 {LifeMaxBonus}点]"},
                    {"IronCurtain",$"[c/00FF00:每{MaxCounter/60}秒获得{ImmuneCounter/60}秒无敌]"},
                    {"tooltips", $"[c/00FF00:每减少{unit*100}%生命值，增加{DefenseBonus}点防御、{EnduranceBonus*100}%减伤，并减少{SpeedBonus*(-100)}%移速加成]"},
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
            
            Recipe recipe = Recipe.Create(ModContent.ItemType<HallowHeartShield>()); // 替换为 GaSniperD 的类型  
	        recipe.AddIngredient(ItemID.HallowedBar, 5);
	        recipe.AddIngredient(ItemID.LifeCrystal, 3);
            recipe.AddIngredient(ItemID.MeteoriteBar,3);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

        
        
    }

    // ... existing code ...
public class HallowHeartShieldPlayer : ModPlayer
{
    private int shieldCounter = 0;
    private const int MAX_COUNTER = 1200;
    private const int IMMUNE_DURATION = 180;
    public bool hasHallowHeartShield = false;

    public override void ResetEffects()
    {
        hasHallowHeartShield = false;
    }

    public override void PreUpdate()
    {
        if (hasHallowHeartShield)
        {
            // 更新计数器
            if (shieldCounter < MAX_COUNTER)
            {
                shieldCounter++;
            }
            else
            {
                shieldCounter = 0;
            }

            // 管理无敌状态和buff
            if (shieldCounter >= (MAX_COUNTER - IMMUNE_DURATION))
            {
                // 激活无敌期间
                Player.immune = true;
                Player.immuneTime = 2;
                
                // 移除冷却buff（因为正在无敌）
                if (Player.HasBuff(ModContent.BuffType<HallowHeartShieldCooldown>()))
                {
                    Player.ClearBuff(ModContent.BuffType<HallowHeartShieldCooldown>());
                }
            }
            else
            {
                // 冷却期间，应用冷却buff
                int remainingCooldown = MAX_COUNTER - IMMUNE_DURATION - shieldCounter;
                if (remainingCooldown > 0 && !Player.HasBuff(ModContent.BuffType<HallowHeartShieldCooldown>()))
                {
                    Player.AddBuff(ModContent.BuffType<HallowHeartShieldCooldown>(), remainingCooldown);
                }
            }
        }
        else
        {
            // 没有装备盾牌时清理状态
            shieldCounter = 0;
            if (Player.HasBuff(ModContent.BuffType<HallowHeartShieldCooldown>()))
            {
                Player.ClearBuff(ModContent.BuffType<HallowHeartShieldCooldown>());
            }
        }
    }
}
// ... existing code ...
}