using ExpansionKele.Commons;
using ExpansionKele.Content.Items.OtherItem;
using ExpansionKele.Content.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Global 
{  
    public class ExpansionKeleGlobalItem : GlobalItem
    {
        public override void SetStaticDefaults()
        {
            SetStaticDefaults_ShimmerRecipes();
        }
        // ... existing code ...
        public override void ExtractinatorUse(int extractType, int extractinatorBlockType, ref int resultType, ref int resultStack)
        {
            // extractType: 0 = 泥沙/雪泥, 3347 = 沙漠化石
            switch (extractType)
            {
                // 泥沙和雪泥
                case 0:
                    // 4%概率产出望月矿
                    if (Main.rand.NextFloat() < 0.04f && NPC.downedBoss3)
                    {
                        // Main.NewText($"{extractType}");
                        resultType = ModContent.ItemType<ChromiumOrePowder>();
                        resultStack = Main.rand.Next(1, 13); // 1-16个
                    }
                    break;
                    
                // 沙漠化石
                case 3347:
                    // 4%概率产出星光矿
                    if (Main.rand.NextFloat() < 0.04f && NPC.downedBoss3)
                    {
                        // Main.NewText($"{extractType}");
                        resultType = ModContent.ItemType<ChromiumOrePowder>();
                        resultStack = Main.rand.Next(1, 13); // 1-16个
                    }
                    break;
                default:{
                    break;
                }
                
                    
            }
        }
// ... existing code ...

        public override void UseItemFrame(Item item, Player player)
        {
            // 应用改进的物品定位逻辑到所有近战挥舞类武器
            if (item.useStyle == ItemUseStyleID.Swing)//&&item.noUseGraphic==false
            {
                ExpansionKeleUtils.ConductBetterItemLocation(player);
            }
        }
        private void SetStaticDefaults_ShimmerRecipes()
        {
            var shimmerTransmute = ItemID.Sets.ShimmerTransformToItem;
            shimmerTransmute[ModContent.ItemType<ChromiumOre>()] =ItemID.Hellstone;
        }
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            // 对ExpansionKele模组和ExpansionKeleCal模组的武器应用特定武器伤害倍率配置
            if (item.ModItem != null && 
                (item.ModItem.Mod.Name == "ExpansionKele" || 
                 item.ModItem.Mod.Name == "ExpansionKeleCal"))
            {
                damage *= ExpansionKeleConfig.Instance.SpecificWeaponDamageMultiplier;
            }

            // 应用运行时物品属性修改
            int originalDamage = ContentSamples.ItemsByType[item.netID].damage;
            if (originalDamage > 0)
            {
                int modifiedDamage = RuntimeItemModificationSystem.ApplyDamageModifications(item, player, originalDamage);
                if (modifiedDamage != originalDamage)
                {
                    float multiplier = (float)modifiedDamage / originalDamage;
                    damage *= multiplier;
                }
            }
        }

        // ... existing code ...
        // ... existing code ...
        public override void ModifyTooltips(Item item, System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // 只对有伤害值的武器显示基础伤害
            if (item.damage > 0)
            {
                // 获取物品的基础伤害（不包含修饰语）
                int baseDamage = ContentSamples.ItemsByType[item.netID].damage;
                
                // 计算修改后的伤害
                Player localPlayer = Main.LocalPlayer;
                int modifiedDamage = RuntimeItemModificationSystem.ApplyDamageModifications(item, localPlayer, baseDamage);
                
                // 查找玩家特定的修改信息用于显示
                string damageDisplay = "";
                var playerSpecificModifications = RuntimeItemModificationSystem.GetPlayerSpecificModifications(localPlayer, item.netID);
                
                if (playerSpecificModifications.Count > 0 && modifiedDamage != baseDamage)
                {
                    // 获取最后一个修改（因为我们的修改规则是最新的覆盖旧的）
                    var lastModification = playerSpecificModifications[playerSpecificModifications.Count - 1];
                    switch (lastModification.Type)
                    {
                        case ItemPropertyModification.ModificationType.Add:
                            damageDisplay = $"基础伤害：{modifiedDamage}---{baseDamage} (+{lastModification.Value})";
                            break;
                        case ItemPropertyModification.ModificationType.Multiply:
                            damageDisplay = $"基础伤害：{modifiedDamage}---{baseDamage} (×{lastModification.Value:F2})";
                            break;
                        case ItemPropertyModification.ModificationType.SetValue:
                            damageDisplay = $"基础伤害：{modifiedDamage}---{baseDamage} ({lastModification.Value})";
                            break;
                    }
                }
                else
                {
                    // 没有修改或者修改后数值相同
                    damageDisplay = $"基础伤害：{baseDamage}";
                }
                
                tooltips.Add(new TooltipLine(Mod, "BaseDamage", $"[c/00FFFF:{damageDisplay}]"));
            }

            // 显示自定义修改信息
            var globalModifications = RuntimeItemModificationSystem.GetGlobalModifications(item.netID);
            var player = Main.LocalPlayer;
            var playerModifications = RuntimeItemModificationSystem.GetPlayerSpecificModifications(player, item.netID);
            
            if (globalModifications.Count > 0 || playerModifications.Count > 0)
            {
                string modificationText = "[c/FF69B4:已被开发者修改：修改属性如下：]";
                tooltips.Add(new TooltipLine(Mod, "DevModifiedHeader", modificationText));
                
                // 显示全局修改
                foreach (var modification in globalModifications)
                {
                    string operation = "";
                    switch (modification.Type)
                    {
                        case ItemPropertyModification.ModificationType.Multiply:
                            operation = $"乘以 {modification.Value} ({modification.Description})";
                            break;
                        case ItemPropertyModification.ModificationType.Add:
                            operation = $"增加 {modification.Value} ({modification.Description})";
                            break;
                        case ItemPropertyModification.ModificationType.SetValue:
                            operation = $"设为 {modification.Value} ({modification.Description})";
                            break;
                    }
                    tooltips.Add(new TooltipLine(Mod, "DevModifiedGlobalDetail", $"[c/FF69B4:全局 - {operation}]"));
                }
                
                // 显示玩家特定修改
                foreach (var modification in playerModifications)
                {
                    string operation = "";
                    switch (modification.Type)
                    {
                        case ItemPropertyModification.ModificationType.Multiply:
                            operation = $"乘以 {modification.Value} ({modification.Description})";
                            break;
                        case ItemPropertyModification.ModificationType.Add:
                            operation = $"增加 {modification.Value} ({modification.Description})";
                            break;
                        case ItemPropertyModification.ModificationType.SetValue:
                            operation = $"设为 {modification.Value} ({modification.Description})";
                            break;
                    }
                    tooltips.Add(new TooltipLine(Mod, "DevModifiedPlayerDetail", $"[c/FF69B4:个人 - {operation}]"));
                }
            }
        }
// ... existing code ...

    }
}