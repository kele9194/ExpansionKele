using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Threading;
using System.IO;

namespace ExpansionKele.Content.Armor.StarArmorA
{
    // 抽象基类，用于星元盔甲头盔
    [AutoloadEquip(EquipType.Head)]
    public abstract class StarHelmetAbs : ModItem
    {
        public override string LocalizationCategory => "Armor.StarArmorA";
        
        // 抽象属性，每个具体的头盔实现需要提供自己的索引
        public abstract int Index { get; }
        
        // 从ArmorData获取各项数值
        protected int HelmetDefense => ArmorData.HelmetDefense[Index];
        protected float GenericDamageBonus => ArmorData.GenericDamageBonus[Index] / 100f;
        protected int MaxTurrets => ArmorData.MaxTurrets[Index];
        protected float RogueStealthMax => ArmorData.StealthMax[Index] / 100f;
        protected int RogueCritChance => ArmorData.RogueCritChance[Index];
        protected float A => ArmorData.CalculateA(0.34f + 0.3f * ArmorData.GenericDamageBonus[Index] / 100f);

        public static LocalizedText SetBonusText { get; private set; }
        public static LocalizedText SetBonusTextWithCalamity { get; private set; }

        public override void SetStaticDefaults()
        {
            SetBonusText = this.GetLocalization("SetBonus");
            SetBonusTextWithCalamity = this.GetLocalization("SetBonusWithCalamity").WithFormatArgs(RogueCritChance, RogueStealthMax * 100);
        }

        public override void SetDefaults()
        {
            Item.width = 18; // Width of the item
            Item.height = 18; // Height of the item
            Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
            Item.rare = ItemRarityID.Green; // The rarity of the item
            Item.defense = HelmetDefense; // The amount of defense the item will give when equipped
        }

        // IsArmorSet determines what armor pieces are needed for the setbonus to take effect
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType && // 使用当前实例的类型而不是硬编码
                   body.type == GetBodyType() &&
                   legs.type == GetLegsType();
        }

        // UpdateArmorSet allows you to give set bonuses to the armor.
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            if (ExpansionKele.calamity != null)
            {
                player.setBonus = SetBonusTextWithCalamity.Value;
                player.GetCritChance<ThrowingDamageClass>() += RogueCritChance;
                ReflectionHelper.ApplyRogueStealth(player, RogueStealthMax);
            }
            float lifePercentage = player.statLife / (float)player.statLifeMax2;
            if (lifePercentage > 1)
            {
                lifePercentage = 1;
            }
            float damageBoost = (1 / (lifePercentage + A)) - (1 / (1 + A));
            player.GetDamage<GenericDamageClass>() += damageBoost;
        }

        public override void UpdateEquip(Player player)
        {
            player.maxTurrets += MaxTurrets;
            player.GetDamage(DamageClass.Generic) += GenericDamageBonus;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModContent.GetInstance<ExpansionKeleConfig>().EnableDetailedTooltips)
            {
                tooltips.Add(new TooltipLine(Mod, "DetailedInfo", "[c/00FF00:详细信息:]"));
                var tooltipData = new Dictionary<string, string>
                {
                    {"GenericDamageBonus", $"[c/00FF00:伤害 +{GenericDamageBonus*100}%]"},
                    {"maxTurrets", $"[c/00FF00:最大哨兵数量 +{MaxTurrets}]"},
                };

                foreach (var kvp in tooltipData)
                {
                    tooltips.Add(new TooltipLine(Mod, kvp.Key, kvp.Value));
                }
            }
        }

        // ... existing code ...
        // 抽象方法，让子类指定对应的胸甲和腿甲类型
        protected abstract int GetBodyType();
        protected abstract int GetLegsType();

        // 获取当前实例的类型（用于IsArmorSet）
        protected virtual int ItemType => ModContent.ItemType<StarHelmetAbs>();
    }
}
// ... existing code ...