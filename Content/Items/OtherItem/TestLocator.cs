using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Terraria.ID;
using ExpansionKele.Content.Items.Accessories;
using System.Formats.Asn1;
using Stubble.Core.Classes;
using ExpansionKele.Content.Items.Tiles;

namespace ExpansionKele.Content.Items.OtherItem
{
    public class TestLocator : ModItem
    {
        public override string LocalizationCategory => "Items.OtherItem";
        public override void SetDefaults()
        {
            //Item.SetNameOverride("自定义属性检测仪");
            Item.width = 24;
            Item.height = 24;
            Item.value = 0;
            Item.rare = ItemRarityID.Red;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
        }

        // ... existing code ...
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            
            // 获取玩家的自定义乘算增伤数值
            var damageMultiPlayer = player.GetModPlayer<ExpansionKeleDamageMulti>();
            float multiplicativeDamage = damageMultiPlayer.MultiplicativeDamageBonus;
            
            // 获取玩家的自定义减伤数值
            var reductionPlayer = player.GetModPlayer<CustomDamageReductionPlayer>();
            float customDamageReduction = reductionPlayer.customDamageReduction;

            var starryLifeEmblemPlayer = player.GetModPlayer<StarryLifeEmblemPlayer>();
            int CooldownTimer= starryLifeEmblemPlayer.fatalCooldownTimer;
            
            // 添加乘算增伤提示信息
            tooltips.Add(new TooltipLine(Mod, "MultiplicativeDamage", $"乘算增伤: {(multiplicativeDamage - 1f) * 100:F2}%"));
            
            // 添加自定义减伤提示信息
            tooltips.Add(new TooltipLine(Mod, "CustomDamageReduction", $"自定义减伤: {customDamageReduction * 100:F2}%"));
            tooltips.Add(new TooltipLine(Mod, "CooldownTimer", $"冷却时间: {starryLifeEmblemPlayer.fatalCooldownTimer:F2}"));
            
            // 显示满月矿石生成状态
            tooltips.Add(new TooltipLine(Mod, "FullMoonOreGenerated", $"满月矿石已生成: {FullMoonOreSystem.oreGenerated}"));
        }
// ... existing code ...
    }
}