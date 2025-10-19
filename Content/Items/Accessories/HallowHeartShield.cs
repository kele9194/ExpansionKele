using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

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
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
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

            if (counter < MaxCounter)
    {
        counter++;
    }
    else
    {
        counter = 0;
    }

    // 最后120ticks时设置玩家为无敌状态
    if (counter >= (MaxCounter-ImmuneCounter))
    {
        player.immune = true;
        player.immuneTime = 0; // 确保无敌效果持续
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
}