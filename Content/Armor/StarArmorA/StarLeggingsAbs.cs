using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Armor.StarArmorA
{
    // 抽象基类，用于星元护胫系列护具
    [AutoloadEquip(EquipType.Legs)]
    public abstract class StarLeggingsAbs : ModItem
    {
        public override string LocalizationCategory => "Armor.StarArmorA";

        // 定义需要在派生类中实现的抽象属性

        
        // 基于Index获取的属性
        public abstract int LeggingsDefense { get; }
        public abstract float MoveSpeedBonus { get; }
        public abstract float SummonDamage { get; }
        public abstract int MeleeCritChance { get; }
        public abstract int RangedCritChance { get; }
        public abstract float MeleeSpeed { get; }
        public abstract int MaxMana { get; }
        public abstract float ManaCostReduction { get; }
        public abstract float AmmoCostReduction { get; }

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBonus);

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = 1000; // How many coins the item is worth
            Item.rare = ItemRarityID.White; // The rarity of the item
            Item.defense = LeggingsDefense; // The amount of defense the item will give when equipped
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += MoveSpeedBonus; // Increase the movement speed of the player
            player.GetCritChance(DamageClass.Melee) += MeleeCritChance;
            player.GetCritChance(DamageClass.Ranged) += RangedCritChance;
            player.GetAttackSpeed(DamageClass.Melee) += MeleeSpeed;
            player.statManaMax2 += MaxMana;
            player.manaCost -= ManaCostReduction;
            player.ammoCost75 = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"MoveSpeedBonus", $"[c/00FF00:移速 +{MoveSpeedBonus * 100}%]"}, // 移速通常是百分比形式
                    {"MeleeCritChance", $"[c/00FF00:近战暴击 +{MeleeCritChance}%]"}, // 近战暴击率
                    {"RangedCritChance", $"[c/00FF00:远程暴击 +{RangedCritChance}%]"}, // 远程暴击率
                    {"MeleeSpeed", $"[c/00FF00:近战攻击速度 +{MeleeSpeed * 100}%]"}, // 近战攻击速度通常是百分比形式
                    {"MaxMana", $"[c/00FF00:法术上限 +{MaxMana}]"},
                    {"ManaCostReduction", $"[c/00FF00:法术消耗减少 -{ManaCostReduction * 100}%]"}, // 法术消耗减少通常是百分比形式
                    {"AmmoCost75", "[c/00FF00:弹药消耗减少 -25%]"},
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }

        public abstract override void AddRecipes();

    }
}