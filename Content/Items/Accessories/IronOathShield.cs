using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Placeables;

namespace ExpansionKele.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class IronOathShield : ModItem
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
        private static float ImmuneCounter=3*60;
        public override string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = ItemUtils.CalculateValueFromRecipes(this);
            Item.rare = ItemUtils.CalculateRarityFromRecipes(this); 
            Item.accessory = true;
            Item.defense = ExpansionKele.DEFTool(6,9);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.noKnockback=true;
            // 基础移速加成30%
            player.moveSpeed += MaxSpeedBonus;
            player.statLifeMax2 +=LifeMaxBonus;

            var ironWillShieldPlayer=player.GetModPlayer<IronWillShieldPlayer>();
            ironWillShieldPlayer.SetTimers(30*60,2*60,0.3f);
            ironWillShieldPlayer.UpdateIronWillShield();



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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            // {
            //     tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
            //     var tooltipData = new Dictionary<string, string>
            //     {
            //         {"MaxLifeBoost", "[c/00FF00:最大生命值增加 60]"},
            //         {"KnockbackImmunity", "[c/00FF00:免疫击退]"},
            //         {"DamageReduction", "[c/00FF00:受伤后获得减伤效果]"},
            //         {"PreDefenseReduction", "[c/00FF00:15%防御前伤害减免]"},
            //     };

            //     foreach (var kvp in tooltipData)
            //     {
            //         tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            //     }
            // }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HallowHeartShield>(), 1)
                .AddIngredient(ModContent.ItemType<IronWillShield>(), 1)
                .AddIngredient(ModContent.ItemType<StarryBar>(), 3)
                .AddIngredient(ItemID.LunarBar, 3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}