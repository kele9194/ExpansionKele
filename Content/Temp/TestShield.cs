using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;


namespace ExpansionKele.Content.Temp
{
    
    [AutoloadEquip(EquipType.Shield)] // Load the spritesheet you create as a shield for the player when it is equipped.
    public class TestShield : ModItem
    {
        public override string LocalizationCategory=>"Temp";
        private const string setNameOverride="测试盾";
        
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
            ExpansionKeleTool.AddDamageBonus(player,0.5f);
            ExpansionKeleTool.AddDamageReduction(player,0.2f);
            
            // 使用DamageTrackingHelper获取连续伤害值并应用增伤效果
            long consecutiveDamage = DamageTrackingHelper.GetConsecutiveDamage(player,120);
            if(consecutiveDamage>10000)
            {
                consecutiveDamage=10000;
            }
            //Main.NewText("连续伤害:"+consecutiveDamage);
            
            // 应用增伤效果
            
            if (consecutiveDamage >= 1500)
            {
                ExpansionKeleTool.MultiplyDamageBonus(player, 2.0f); // 增加150%乘算增伤
            }
            if (consecutiveDamage >= 3000)
            {
                ExpansionKeleTool.AddDamageBonus(player, 3.0f); // 增加300%乘算增伤
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var tooltipData = new Dictionary<string, string>
            {
                //{ "Defense", $"防御力 +{Item.defense}" },
				{"MaxSpeedBonus", $"测试用"},
                
            };

            foreach (var kvp in tooltipData)
            {
                tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            }
        }
        public override void AddRecipes()
        {
            
            
        }

        
        
    }
}