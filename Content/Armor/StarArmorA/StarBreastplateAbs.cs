using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Armor.StarArmorA
{
    // The AutoloadEquip attribute automatically attaches an equip texture to this item.
    // Providing the EquipType.Body value here will result in TML expecting X_Arms.png, X_Body.png and X_FemaleBody.png sprite-sheet files to be placed next to the item's main texture.
    [AutoloadEquip(EquipType.Body)]
    public abstract class StarBreastplateAbs : ModItem
    {
        public override string LocalizationCategory => "Armor.StarArmorA";
        
        // 防御值、暴击率加成和最大召唤物数量加成将在具体实现中定义
        public abstract int PlateDefense { get; }
        public abstract int CritChance { get; }
        public abstract int MaxMinions { get; }
        
        // 使用本地化系统处理物品名称和工具提示
        // public override LocalizedText DisplayName => base.DisplayName.WithFormatArgs();
        // public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CritChance, MaxMinions);

        // 设置物品默认属性
        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = 1000;
            Item.rare = ItemRarityID.White;
            Item.defense = PlateDefense; // The amount of defense the item will give when equipped
        }

        // 装备时的效果
        public override void UpdateEquip(Player player)
        {
            player.buffImmune[BuffID.OnFire] = true; // Make the player immune to Fire
            player.maxMinions += MaxMinions; // Increase how many minions the player can have by one
            player.noKnockback = true; // Increase knockback resistance
            player.GetCritChance(DamageClass.Generic) += CritChance;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"critChance", $"[c/00FF00:暴击率 +{CritChance}%]"},
                    { "MaxMinions", $"[c/00FF00:最大召唤物数量 +{MaxMinions}]"},
                    { "FireImmunity", $"[c/00FF00:免疫火焰伤害,免疫击退]"},
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }

        // 合成配方，具体实现由派生类完成
        public abstract override void AddRecipes();
    }
}