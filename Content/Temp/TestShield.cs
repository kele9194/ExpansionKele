using Terraria;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Customs.ECShield;

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
            // ECShieldSystem ecShield = player.GetModPlayer<ECShieldSystem>();
            
            // // 激活EC护盾系统
            // ecShield.ShieldActive = true;
            
            // // 设置护盾最大值为100
            // ecShield.MaxShieldModifier *= 0.2f;
            
            // // 设置基础恢复率为0.2（每秒恢复护盾最大值的20%）
            // ecShield.ShieldRegenModifier *= 10f;
            
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // var tooltipData = new Dictionary<string, string>
            // {
            //     //{ "Defense", $"防御力 +{Item.defense}" },
			// 	{"MaxSpeedBonus", $"获得100点EC护盾\n护盾恢复速度+20%"},
                
            // };

            // foreach (var kvp in tooltipData)
            // {
            //     tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
            // }
        }
        public override void AddRecipes()
        {
            
            
        }

        
        
    }
}