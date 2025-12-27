using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Customs.Commands;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class BalanceScale : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Balance Scale");
            // Tooltip.SetDefault("Collect to enable effects\nEffects currently empty");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(0, 0, 50, 0);
            Item.rare = ItemRarityID.White;
            Item.useStyle = ItemUseStyleID.None;
            Item.useTime = 10;
            Item.useAnimation = 15;
        }
        public override void UpdateInventory(Player player)
        {

        }
        // ... existing code ...
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // 获取全局伤害倍率
            float multiplier = BalancingSystem.GlobalDamageMultiplier;
            string formattedMultiplier = $"*{multiplier:F2}";
            string tooltipText = $"全局伤害倍率：{formattedMultiplier}";
            
            // 创建全局伤害倍率工具提示行
            TooltipLine newLine = new TooltipLine(Mod, "BalancingScaleMultiplier", tooltipText);
            
            // 根据收藏状态设置颜色
            if (ExpansionKeleConfig.Instance.EnableGlobalDamageMultiplierModification)
            {
                newLine.OverrideColor = Microsoft.Xna.Framework.Color.Red;
            }
            else
            {
                newLine.OverrideColor = Microsoft.Xna.Framework.Color.Gray;
            }
            
            // 添加到工具提示列表
            tooltips.Add(newLine);
            
            // 添加原版物品伤害倍率
            if (HandHeldSystem.VanillaDamageMultiplier != 1.0f)
            {
                string vanillaFormattedMultiplier = $"*{HandHeldSystem.VanillaDamageMultiplier:F2}";
                string vanillaTooltipText = $"原版物品伤害倍率：{vanillaFormattedMultiplier}";
                
                TooltipLine vanillaLine = new TooltipLine(Mod, "VanillaDamageMultiplier", vanillaTooltipText);
                // 根据全局设置决定颜色
                if (ExpansionKeleConfig.Instance.EnableGlobalDamageMultiplierModification)
                {
                    vanillaLine.OverrideColor = Microsoft.Xna.Framework.Color.Red;
                }
                else
                {
                    vanillaLine.OverrideColor = Microsoft.Xna.Framework.Color.Gray;
                }
                
                tooltips.Add(vanillaLine);
            }
            
            // 添加非1的mod伤害倍率
            foreach (var kvp in HandHeldSystem.ModDamageMultipliers)
            {
                if (kvp.Value != 1.0f)
                {
                    string modFormattedMultiplier = $"*{kvp.Value:F2}";
                    string modTooltipText = $"{kvp.Key}物品伤害倍率：{modFormattedMultiplier}";
                    
                    TooltipLine modLine = new TooltipLine(Mod, $"ModDamageMultiplier_{kvp.Key}", modTooltipText);
                    // 根据全局设置决定颜色
                    if (ExpansionKeleConfig.Instance.EnableGlobalDamageMultiplierModification)
                    {
                        modLine.OverrideColor = Microsoft.Xna.Framework.Color.Red;
                    }
                    else
                    {
                        modLine.OverrideColor = Microsoft.Xna.Framework.Color.Gray;
                    }
                    
                    tooltips.Add(modLine);
                }
            }
        }


    }
}
// ... existing code ...