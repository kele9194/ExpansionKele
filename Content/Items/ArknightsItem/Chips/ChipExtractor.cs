using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using Terraria.Localization;
using ExpansionKele.Content.Items.ArknightsItem.GuardChips;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 芯片提取器：右键使用，从当前手持的已刻印武器中提取刻印芯片并返还，
    /// 同时消耗50铜币。本物品不会被消耗。
    /// </summary>
    public class ChipExtractor : ModItem
    {
        public override string LocalizationCategory => "Items.Chips";
        private string Prefix => $"Mods.ExpansionKele.Items.Chips.{GetType().Name}";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 10);

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.consumable = false;
            Item.noMelee = true;
        }

        public override bool CanRightClick() => true;

        public override void RightClick(Player player)
        {
            Item held = player.HeldItem;
            if (held == null || held.IsAir)
            {
                Main.NewText(Language.GetTextValue($"{Prefix}.NoWeapon"), Color.Red);
                return;
            }

            if (ArknightsGlobalItem.IsRealArknights(held))
            {
                Main.NewText(Language.GetTextValue($"{Prefix}.RealArknights"), Color.Red);
                return;
            }

            int currentNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (currentNum == 0)
            {
                Main.NewText(Language.GetTextValue($"{Prefix}.NoEngraving"), Color.Red);
                return;
            }

            // ===== 先检查狙击子职业 =====
            SniperSubclass sniperSubclass = ArknightsGlobalItem.GetSniperSubclass(held);
            if (sniperSubclass != SniperSubclass.Unassigned)
            {
                ExtractSniperChip(player, held, sniperSubclass);
                return;
            }

            // ===== 再检查近卫子职业 =====
            GuardSubclass guardSubclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (guardSubclass != GuardSubclass.Unassigned)
            {
                ExtractGuardChip(player, held, guardSubclass);
                return;
            }

            // 未知类型
            Main.NewText(Language.GetTextValue($"{Prefix}.UnknownType"), Color.Red);
        }

        public override bool ConsumeItem(Player player) => false;

        // ======================== 提取逻辑拆分 ========================

        private void ExtractSniperChip(Player player, Item held, SniperSubclass subclass)
        {
            if (!DeductCost(player)) return;

            ArknightsGlobalItem.TrySetArknightsNum(held, 0);

            int chipType = GetChipTypeFromSniperSubclass(subclass);
            if (chipType > 0)
            {
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), chipType);
            }
            else
            {
                Main.NewText(Language.GetTextValue($"{Prefix}.MissingChip", subclass), Color.Yellow);
            }

            Main.NewText(Language.GetTextValue($"{Prefix}.Success", held.Name), Color.Green);
        }

        private void ExtractGuardChip(Player player, Item held, GuardSubclass subclass)
        {
            if (!DeductCost(player)) return;

            ArknightsGlobalItem.TrySetArknightsNum(held, 0);

            int chipType = GetChipTypeFromGuardSubclass(subclass);
            if (chipType > 0)
            {
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), chipType);
            }
            else
            {
                Main.NewText(Language.GetTextValue($"{Prefix}.MissingChip", subclass), Color.Yellow);
            }

            Main.NewText(Language.GetTextValue($"{Prefix}.Success", held.Name), Color.Green);
        }

        /// <summary>扣除50铜币，钱不够返回false</summary>
        private bool DeductCost(Player player)
        {
            const int cost = 50;
            if (!player.CanAfford(cost))
            {
                Main.NewText(Language.GetTextValue($"{Prefix}.NoMoney"), Color.Red);
                return false;
            }
            player.BuyItem(cost);
            return true;
        }

        // ======================== 芯片类型映射 ========================

        private static int GetChipTypeFromSniperSubclass(SniperSubclass subclass)
        {
            return subclass switch
            {
                SniperSubclass.Artillerist => ModContent.ItemType<ArtilleristChip>(),
                SniperSubclass.RapidFire   => ModContent.ItemType<RapidFireChip>(),
                SniperSubclass.Marksman     => ModContent.ItemType<MarksmanChip>(),
                SniperSubclass.HeavyShooter => ModContent.ItemType<HeavyShooterChip>(),
                SniperSubclass.Thrower      => ModContent.ItemType<ThrowerChip>(),
                SniperSubclass.Siege        => ModContent.ItemType<SiegeChip>(),
                SniperSubclass.Spread       => ModContent.ItemType<SpreadChip>(),
                _ => 0
            };
        }

        private static int GetChipTypeFromGuardSubclass(GuardSubclass subclass)
        {
            return subclass switch
            {
                GuardSubclass.Dreadnought => ModContent.ItemType<DreadnoughtChip>(),
                GuardSubclass.Centurion    => ModContent.ItemType<CenturionChip>(),
                GuardSubclass.ArtsFighter  => ModContent.ItemType<ArtsFighterChip>(),
                GuardSubclass.Fighter      => ModContent.ItemType<FighterChip>(),
                GuardSubclass.Swordmaster  => ModContent.ItemType<SwordmasterChip>(),
                GuardSubclass.Reaper       => ModContent.ItemType<ReaperChip>(),
                GuardSubclass.Crusher      => ModContent.ItemType<CrusherChip>(),
                GuardSubclass.Musha        => ModContent.ItemType<MushaChip>(),
                GuardSubclass.Lord         => ModContent.ItemType<LordChip>(),
                _ => 0
            };
        }

        // ======================== Tooltip ========================

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string fullTip = Language.GetTextValue($"{Prefix}.ChipTooltip");
            string[] lines = fullTip.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.Length == 0) continue;

                string key = i == 0 ? "ChipExtractorInfo" : $"ChipExtractorInfo{i + 1}";
                Color color = i switch
                {
                    0 => Color.LightYellow,
                    _ => Color.LightGray
                };

                tooltips.Add(new TooltipLine(Mod, key, trimmed)
                {
                    OverrideColor = color
                });
            }
        }
    }
}