using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Customs
{
    public enum ArknightsClass : int
    {
        None,
        Vanguard,      // 先锋
        Guard,         // 近卫
        Defender,      // 重装
        Sniper,        // 狙击
        Caster,        // 术师
        Medic,         // 医疗
        Supporter,     // 辅助
        Specialist,    // 特种
    }

    public enum SniperSubclass : int
    {
        Unassigned = 0,
        Base = (int)(ArknightsClass.Sniper) * 16 + 0x00,
        RapidFire,      // 速射狙
        Marksman,       // 神射手
        HeavyShooter,   // 重射手
        Artillerist,    // 炮手
        Thrower,        // 投掷手
        Siege,          // 攻城手
        Spread          // 散射手
    }

    public enum GuardSubclass : int
    {
        Unassigned = 0,
        Base = (int)(ArknightsClass.Guard) * 16 + 0x00,
        Centurion = Base + 1,      // 强攻手
        Dreadnought = Base + 2,    // 无畏者
        Fighter = Base + 3,        // 斗士
        ArtsFighter = Base + 4,    // 术战者
        Lord = Base + 5,           // 领主
        Swordmaster = Base + 6,    // 剑豪
        Musha = Base + 7,          // 武者
        Reaper = Base + 8,         // 收割者
        Instructor = Base + 9,     // 教官
        Liberator = Base + 10,     // 解放者
        Crusher = Base + 11,       // 重剑手
        Earthshaker = Base + 12,   // 撼地者
        PrimalGuard = Base + 13,   // 本源近卫
        Mercenary = Base + 14,     // 佣兵
    }

    public interface IArknightsItem
    {
        int ArknightsNum { get; }
    }

    public class ArknightsGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public bool IsRealArknightsItem { get; internal set; }
        public int ArknightsNum { get; internal set; }
        public bool isHoming { get; internal set; }

        // ======================== SetDefaults ========================

        public override void SetDefaults(Item item)
        {
            if (item.ModItem is IArknightsItem arknightsItem)
            {
                IsRealArknightsItem = true;
                ArknightsNum = arknightsItem.ArknightsNum;
            }
            else
            {
                IsRealArknightsItem = false;
                ArknightsNum = 0;
            }
        }

        // ======================== Tooltips ========================

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            int num = GetArknightsNum(item);

            // 1. 真正的方舟物品 → 紫色标签
            if (IsRealArknights(item))
            {
                string talentName = GetGenericTalentName(num);
                if (!string.IsNullOrEmpty(talentName))
                {
                    string tag = Language.GetTextValue("Mods.ExpansionKele.Items.Chips.ArknightsTag");
                    tooltips.Add(new TooltipLine(Mod, "ArknightsTag", $"{tag}{talentName}")
                    {
                        OverrideColor = Microsoft.Xna.Framework.Color.Purple
                    });
                }
                return;
            }

            // 2. 未刻印 → 不显示
            if (num == 0) return;

            // 3. 已刻印 → 橙色天赋名称
            string talentDisplay = GetGenericTalentName(num);
            if (string.IsNullOrEmpty(talentDisplay)) return;

            tooltips.Add(new TooltipLine(Mod, "ArknightsTalent", talentDisplay)
            {
                OverrideColor = Microsoft.Xna.Framework.Color.Orange
            });

            // 4. 制导状态（和 GetGenericTalentName 一样，范围派发）
            bool homingSupported = false;

            if (num >= (int)SniperSubclass.Base && num < (int)SniperSubclass.Base + 8)
                homingSupported = CanSubclassHoming((SniperSubclass)num);
            else if (num >= (int)GuardSubclass.Base && num < (int)GuardSubclass.Base + 15)
                homingSupported = CanGuardSubclassHoming((GuardSubclass)num);

            if (homingSupported)
            {
                if (isHoming)
                {
                    tooltips.Add(new TooltipLine(Mod, "ArknightsHomingStatusOn",
                        Language.GetTextValue("Mods.ExpansionKele.Items.Chips.HomingStatusOn"))
                    {
                        OverrideColor = Microsoft.Xna.Framework.Color.Cyan
                    });
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "ArknightsHomingStatusOff",
                        Language.GetTextValue("Mods.ExpansionKele.Items.Chips.HomingStatusOff"))
                    {
                        OverrideColor = Microsoft.Xna.Framework.Color.Gray
                    });
                }
            }
        }

        // ======================== 统一天赋名查询（范围派发） ========================

        /// <summary>
        /// 按范围依次尝试各职业枚举，用枚举名拼本地化键值。
        /// 键值格式: TalentDisplayName.{枚举名}
        /// 例如: TalentDisplayName.RapidFire、TalentDisplayName.Centurion
        /// </summary>
        private static string GetGenericTalentName(int num)
        {
            if (num <= 0) return "";

            // 尝试狙击子职业 (96~103)
            if (num >= (int)SniperSubclass.Base && num < (int)SniperSubclass.Base + 8)
            {
                var subclass = (SniperSubclass)num;
                string key = $"Mods.ExpansionKele.Items.Chips.TalentDisplayName.{subclass}";
                string value = Language.GetTextValue(key);
                if (value != key) return value;
                return "";
            }

            // 尝试近卫子职业 (32~46)
            if (num >= (int)GuardSubclass.Base && num < (int)GuardSubclass.Base + 15)
            {
                var subclass = (GuardSubclass)num;
                string key = $"Mods.ExpansionKele.Items.Chips.TalentDisplayName.{subclass}";
                string value = Language.GetTextValue(key);
                if (value != key) return value;
                return "";
            }

            return "";
        }

        // ======================== 子类查询 ========================

        public static SniperSubclass GetSniperSubclass(Item item)
        {
            int num = GetArknightsNum(item);
            if (num >= (int)SniperSubclass.Base && num < (int)SniperSubclass.Base + 8)
                return (SniperSubclass)num;
            return SniperSubclass.Unassigned;
        }

        public static GuardSubclass GetGuardSubclass(Item item)
        {
            int num = GetArknightsNum(item);
            if (num >= (int)GuardSubclass.Base && num < (int)GuardSubclass.Base + 15)
                return (GuardSubclass)num;
            return GuardSubclass.Unassigned;
        }

        // ======================== 制导白名单（每个职业独立方法） ========================

        public static bool CanSubclassHoming(SniperSubclass subclass)
        {
            return subclass switch
            {
                SniperSubclass.RapidFire => true,
                SniperSubclass.Marksman => true,
                SniperSubclass.HeavyShooter => false,
                SniperSubclass.Siege => true,
                _ => false
            };
        }

        public static bool CanGuardSubclassHoming(GuardSubclass subclass)
        {
            return false;
        }

        // ======================== 刻印读写 ========================

        public static bool TrySetArknightsNum(Item item, int num)
        {
            var arkItem = item.GetGlobalItem<ArknightsGlobalItem>();
            if (arkItem.IsRealArknightsItem)
                return false;
            arkItem.ArknightsNum = num;
            return true;
        }

        public static int GetArknightsNum(Item item)
        {
            return item.GetGlobalItem<ArknightsGlobalItem>().ArknightsNum;
        }

        public static bool IsRealArknights(Item item)
        {
            return item.GetGlobalItem<ArknightsGlobalItem>().IsRealArknightsItem;
        }

        // ======================== 存档 ========================

        public override void SaveData(Item item, TagCompound tag)
        {
            tag["IsRealArknightsItem"] = IsRealArknightsItem;
            tag["ArknightsNum"] = ArknightsNum;
            tag["isHoming"] = isHoming;
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            IsRealArknightsItem = tag.GetBool("IsRealArknightsItem");
            ArknightsNum = tag.GetInt("ArknightsNum");
            isHoming = tag.GetBool("isHoming");
        }
    }
}