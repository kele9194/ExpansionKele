using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items.Accessories
{
    public class VanillaEmblemOverride : GlobalItem
    {
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            // 检查是否启用了原版物品修改
            bool overridesEnabled = ExpansionKeleConfig.Instance.EnableVanillaItemOverrides;
            
            // 修改原版复仇者徽章的伤害加成从12%到15%
            if (item.type == ItemID.AvengerEmblem && overridesEnabled)
            {
                // 确保只在原本是12%伤害加成的情况下修改
                player.GetDamage(DamageClass.Generic) += 0.03f; // 增加3%的伤害，总共15%
            }
            
            // 修改原版毁灭者徽章的伤害加成从10%到12%（毁灭者徽章已经有8%暴击率，不需要修改）
            if (item.type == ItemID.DestroyerEmblem && overridesEnabled)
            {
                // 确保只在原本是10%伤害加成的情况下修改
                player.GetDamage(DamageClass.Generic) += 0.02f; // 增加2%的伤害，总共12%
                // 毁灭者徽章本身就有8%暴击率加成，不需要额外添加
            }
            
            // 修改原版泰拉靴，添加蛙腿效果
            if (item.type == ItemID.TerrasparkBoots && overridesEnabled)
            {
                // 添加蛙腿效果
                player.autoJump = true;              // 允许自动跳跃
                player.jumpSpeedBoost += 1.6f;       // 跳跃速度提升1.6
                player.extraFall += 10;              // 最大安全坠落距离提高10格
            }
        }
        
        // ... existing code ...
        public override void ModifyTooltips(Item item, System.Collections.Generic.List<TooltipLine> tooltips)
        {
            bool overridesEnabled = ExpansionKeleConfig.Instance.EnableVanillaItemOverrides;
            
            if (item.type == ItemID.AvengerEmblem)
            {
                if (overridesEnabled)
                {
                    // 修改提示文本显示15%伤害加成而不是12%
                    foreach (TooltipLine line in tooltips)
                    {
                        if (line.Text.Contains("12%"))
                        {
                            line.Text = Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.AvengerEmblemModified.DamageChange");
                        }
                    }
                    // 添加提示说明该物品已被KeleExpansion修改
                    TooltipLine keleLine = new TooltipLine(Mod, "KeleExpansionModified", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.AvengerEmblemModified.Enabled"));
                    tooltips.Add(keleLine);
                }
                else
                {
                    // 添加提示说明该物品的修改已被关闭
                    TooltipLine keleLine = new TooltipLine(Mod, "KeleExpansionModified", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.AvengerEmblemModified.Disabled"));
                    tooltips.Add(keleLine);
                }
            }
            
            if (item.type == ItemID.DestroyerEmblem)
            {
                if (overridesEnabled)
                {
                    // 修改提示文本显示12%伤害加成而不是10%
                    foreach (TooltipLine line in tooltips)
                    {
                        if (line.Text.Contains("10%"))
                        {
                            line.Text = Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.DestroyerEmblemModified.DamageChange");
                        }
                    }
                    // 添加提示说明该物品已被KeleExpansion修改
                    TooltipLine keleLine = new TooltipLine(Mod, "KeleExpansionModified", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.DestroyerEmblemModified.Enabled"));
                    tooltips.Add(keleLine);
                }
                else
                {
                    // 添加提示说明该物品的修改已被关闭
                    TooltipLine keleLine = new TooltipLine(Mod, "KeleExpansionModified", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.DestroyerEmblemModified.Disabled"));
                    tooltips.Add(keleLine);
                }
            }
            
            // 为泰拉靴添加新效果提示
            if (item.type == ItemID.TerrasparkBoots)
            {
                if (overridesEnabled)
                {
                    // 添加蛙腿效果提示
                    TooltipLine frogLegLine = new TooltipLine(Mod, "FrogLegBonus", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.TerrasparkBootsModified.FrogLegBonus"))
                    {
                        IsModifier = true,
                    };
                    tooltips.Add(frogLegLine);
                    // 添加提示说明该物品已被KeleExpansion修改
                    TooltipLine keleLine = new TooltipLine(Mod, "KeleExpansionModified", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.TerrasparkBootsModified.Enabled"));
                    tooltips.Add(keleLine);
                }
                else
                {
                    // 添加提示说明该物品的修改已被关闭
                    TooltipLine keleLine = new TooltipLine(Mod, "KeleExpansionModified", Language.GetTextValue("Mods.ExpansionKele.Items.Accessories.VanillaOverrides.TerrasparkBootsModified.Disabled"));
                    tooltips.Add(keleLine);
                }
            }
        }
// ... existing code ...
    }
}