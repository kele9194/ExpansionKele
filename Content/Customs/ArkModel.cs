using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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

    public enum DefenderSubclass : int
    {
        Unassigned = 0,
        Base = (int)(ArknightsClass.Defender) * 16 + 0x00,   // = 48
        Protector = Base + 1,   // 铁卫
        // Fortress = Base + 2,       // 要塞
        // Juggernaut = Base + 3,     // 决战者
        // ArtsProtector = Base + 4,  // 哨戒铁卫
        // Guardian = Base + 5,       // 不屈者
        // PrimalProtector = Base + 6,// 本源铁卫
        // Sustainer = Base + 7,      // 自强体
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
            else if (num >= (int)DefenderSubclass.Base && num < (int)DefenderSubclass.Base + 15)
                homingSupported = CanDefenderSubclassHoming((DefenderSubclass)num);

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

            // 尝试重装子职业 (48~62)
            if (num >= (int)DefenderSubclass.Base && num < (int)DefenderSubclass.Base + 15)
            {
                var subclass = (DefenderSubclass)num;
                string key = $"Mods.ExpansionKele.Items.Chips.TalentDisplayName.{subclass}";
                string value = Language.GetTextValue(key);
                if (value != key) return value;
                return "";
            }

            return "";
        }

        // ======================== 子类查询 ========================

        /// <summary>
        /// 读取物品的狙击子职业。空气 / 无效物品返回 <see cref="SniperSubclass.Unassigned"/>。
        /// </summary>
        public static SniperSubclass GetSniperSubclass(Item item)
        {
            int num = GetArknightsNum(item);
            if (num >= (int)SniperSubclass.Base && num < (int)SniperSubclass.Base + 8)
                return (SniperSubclass)num;
            return SniperSubclass.Unassigned;
        }

        /// <summary>
        /// 读取物品的近卫子职业。空气 / 无效物品返回 <see cref="GuardSubclass.Unassigned"/>。
        /// </summary>
        public static GuardSubclass GetGuardSubclass(Item item)
        {
            int num = GetArknightsNum(item);
            if (num >= (int)GuardSubclass.Base && num < (int)GuardSubclass.Base + 15)
                return (GuardSubclass)num;
            return GuardSubclass.Unassigned;
        }

        /// <summary>
        /// 读取物品的重装子职业。空气 / 无效物品返回 <see cref="DefenderSubclass.Unassigned"/>。
        /// </summary>
        public static DefenderSubclass GetDefenderSubclass(Item item)
        {
            int num = GetArknightsNum(item);
            if (num >= (int)DefenderSubclass.Base && num < (int)DefenderSubclass.Base + 15)
                return (DefenderSubclass)num;
            return DefenderSubclass.Unassigned;
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

        public static bool CanDefenderSubclassHoming(DefenderSubclass subclass)
        {
            return false;
        }

        // ======================== 刻印读写 ========================

        /// <summary>
        /// 安全地获取物品上的 <see cref="ArknightsGlobalItem"/> 附加数据实例。
        /// <para/>空气物品（<c>type == 0</c>）不存在附加数据实例，此时直接调用
        /// <see cref="Item.GetGlobalItem{T}()"/> 会抛 <see cref="KeyNotFoundException"/>：
        /// TML 的 <c>GlobalType.TryGetGlobal</c> 只在 <c>entityType &gt; 0</c> 时才读实例数组，
        /// 否则回落到 <c>AppliesToType</c> 分支（其实现为 <c>type &gt; 0 &amp;&amp; ...</c>）并判否。
        /// 因此一切按 Item 读取刻印数据的入口都应先走这里。
        /// </summary>
        /// <param name="item">待查询的物品，允许为 <see langword="null"/>。</param>
        /// <param name="arkItem">成功时输出附加数据实例；失败时为 <see langword="null"/>。</param>
        /// <returns>取到附加数据实例时返回 <see langword="true"/>。</returns>
        public static bool TryGetArknightsGlobalItem(Item item, out ArknightsGlobalItem arkItem)
        {
            arkItem = null;

            // type <= ItemID.None（空气 / 空手 / 未初始化物品）没有附加数据实例，查询会抛异常
            if (item == null || item.type <= ItemID.None)
                return false;

            return item.TryGetGlobalItem(out arkItem);
        }

        /// <summary>
        /// 写入刻印编号。真·方舟物品与空气 / 无效物品会写入失败。
        /// </summary>
        /// <param name="item">目标物品。</param>
        /// <param name="num">要写入的刻印编号，0 表示清除刻印。</param>
        /// <returns>写入成功返回 <see langword="true"/>。</returns>
        public static bool TrySetArknightsNum(Item item, int num)
        {
            if (!TryGetArknightsGlobalItem(item, out var arkItem))
                return false;

            if (arkItem.IsRealArknightsItem)
                return false;

            arkItem.ArknightsNum = num;
            return true;
        }

        /// <summary>
        /// 读取物品上的刻印编号（0 表示未刻印）。空气 / 无效物品一律返回 0。
        /// </summary>
        /// <param name="item">待查询的物品。</param>
        /// <returns>刻印编号；无法读取时返回 0。</returns>
        public static int GetArknightsNum(Item item)
        {
            return TryGetArknightsGlobalItem(item, out var arkItem) ? arkItem.ArknightsNum : 0;
        }

        /// <summary>
        /// 该物品是否为“真·方舟物品”（自带天赋，不可被刻印）。空气 / 无效物品返回 <see langword="false"/>。
        /// </summary>
        /// <param name="item">待查询的物品。</param>
        /// <returns>是真·方舟物品时返回 <see langword="true"/>。</returns>
        public static bool IsRealArknights(Item item)
        {
            return TryGetArknightsGlobalItem(item, out var arkItem) && arkItem.IsRealArknightsItem;
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