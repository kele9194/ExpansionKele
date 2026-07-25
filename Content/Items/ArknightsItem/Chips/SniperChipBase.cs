using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 狙击芯片抽象基类，封装了右键刻印的通用逻辑。
    /// 子类只需提供 <see cref="ChipSubclass"/>，其余文本全由本地化驱动。
    /// </summary>
    public abstract class SniperChipBase : ModItem
    {
        public override string LocalizationCategory => "Items.Chips";
        private bool rightClickSuccessful = false;

        /// <summary>子类提供对应的狙击子类枚举值</summary>
        protected abstract SniperSubclass ChipSubclass { get; }

        /// <summary>当前类的 localization 前缀，由类名自动推导</summary>
        private string Prefix => $"Mods.ExpansionKele.Items.Chips.{GetType().Name}";

        // ---- Tooltip 颜色方案（子类可重写） ----
        protected virtual Color[] TooltipColors => new[]
        {
            Color.LightYellow,
            Color.LightGray,
            Color.LightCyan,
        };

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 62;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(silver: 50);

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.consumable = true;
            Item.noMelee = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            rightClickSuccessful = false;

            Item held = player.HeldItem;
            if (held == null || held.IsAir)
            {
                Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.HoldWeapon"), Color.Red);
                return;
            }

            if (held.accessory || held.vanity)
            {
                Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.WeaponOnly"), Color.Red);
                return;
            }

            if (!held.DamageType.CountsAsClass(DamageClass.Ranged))
            {
                Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.RangedOnly"), Color.Red);
                return;
            }

            if (ArknightsGlobalItem.IsRealArknights(held))
            {
                Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.RealArknights"), Color.Red);
                return;
            }

            int currentNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (currentNum != 0)
            {
                Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.AlreadyEngraved"), Color.Red);
                return;
            }

            bool success = ArknightsGlobalItem.TrySetArknightsNum(held, (int)ChipSubclass);
            if (!success)
            {
                Main.NewText(Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.EngraveFailed"), Color.Red);
                return;
            }

            string engraveName = Language.GetTextValue($"{Prefix}.ChipEngraveName");
            Main.NewText(
                Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ChipsLoad.EngraveSuccess", held.Name, engraveName),
                Color.Green);
            rightClickSuccessful = true;
        }

        public override bool ConsumeItem(Player player) => rightClickSuccessful;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string fullTip = Language.GetTextValue($"{Prefix}.ChipTooltip");
            string[] lines = fullTip.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.Length == 0) continue;

                string key = i == 0 ? "ChipInfo" : $"ChipInfo{i + 1}";
                Color color = i < TooltipColors.Length ? TooltipColors[i] : Color.White;

                tooltips.Add(new TooltipLine(Mod, key, trimmed)
                {
                    OverrideColor = color
                });
            }
        }
    }

    
}