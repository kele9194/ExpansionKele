using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 重剑手芯片（近卫系）：以血换血 —— 防御归零、血量翻倍、受伤即回血，同时把原防御折算成近战伤害。
    /// <list type="bullet">
    /// <item><b>装备</b>：+20% 近战伤害，并额外获得「原防御 × 0.3%」的近战伤害；近战攻速 −15%；
    /// 生命上限 ×2；防御归零；每次受伤后回复该次伤害的 15%（专家 20%、大师 25%）。</item>
    /// <item><b>未装备时放于背包并收藏</b>：+5% 近战伤害；每次受伤后回复该次伤害的 5%；
    /// 不碰防御、生命上限与攻速。</item>
    /// </list>
    /// <para/>
    /// 「防御 × 0.3% 转伤害」是为了补偿自带防御的装备：防御被归零后不再是纯亏损，
    /// 而是换成同等来源的输出，从而让重甲与轻甲构筑各有取舍而非单纯吃亏。
    /// <para/>
    /// 防御归零写两份：<c>statDefense *= 0f</c> 让面板显示 0（玩家看得见代价），
    /// <c>DefenseEffectiveness *= 0f</c> 兜底 —— 后者的每帧重置点独立于 <c>statDefense</c>，
    /// 因此不会被其它 <see cref="ModPlayer"/> 在 <c>PostUpdateEquips</c> 里加回防御而失效。
    /// </summary>
    public class CrusherChip : ModItem
    {
        /// <summary>装备档：常驻近战伤害加成（加算池）。</summary>
        public const float EquippedMeleeBonus = 0.20f;

        /// <summary>装备档：每 1 点原防御转化的近战伤害加成。</summary>
        public const float EquippedDefenseToDamage = 0.003f;

        /// <summary>装备档：近战攻速惩罚。</summary>
        public const float EquippedAttackSpeedPenalty = 0.15f;

        /// <summary>装备档：生命上限倍率。</summary>
        public const int EquippedLifeMaxMultiplier = 2;

        /// <summary>装备档：受伤后回复该次伤害的比例（普通 / 专家 / 大师）。</summary>
        public const float EquippedHealRatioNormal = 0.15f;
        public const float EquippedHealRatioExpert = 0.20f;
        public const float EquippedHealRatioMaster = 0.25f;

        /// <summary>收藏档：常驻近战伤害加成（加算池）。</summary>
        public const float FavoritedMeleeBonus = 0.05f;

        /// <summary>收藏档：受伤后回复该次伤害的比例。</summary>
        public const float FavoritedHealRatio = 0.05f;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CrusherChipPlayer>().chipEquipped = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
                player.GetModPlayer<CrusherChipPlayer>().chipFavorited = true;
        }

        public override void AddRecipes()
        {
            // 叶绿锭：机械三王全破 + 丛林叶绿矿，卡在教官（秘银 / 山铜）与收割者（神圣锭）之间。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.ChlorophyteBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            CrusherChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }
    }

    /// <summary>重剑手芯片的状态：一档装备 / 一档收藏。</summary>
    public class CrusherChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了重剑手芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有重剑手芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了重剑手芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有重剑手芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>归零前的防御值。归零之后读不到，所以每帧在 <see cref="PostUpdateEquips"/> 里先存一份。</summary>
        public int CachedDefense { get; private set; }

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
            CachedDefense = 0;
        }

        public override void PostUpdateEquips()
        {
            int level = Level;
            if (level <= 0)
                return;

            if (level >= 3)
            {
                // 防御必须在归零前读出来，否则「防御转伤害」永远是 0。
                int rawDefense = (int)Player.statDefense;
                CachedDefense = rawDefense;

                Player.GetDamage(DamageClass.Melee) += CrusherChip.EquippedMeleeBonus
                    + rawDefense * CrusherChip.EquippedDefenseToDamage;
                Player.GetAttackSpeed(DamageClass.Melee) -= CrusherChip.EquippedAttackSpeedPenalty;
                Player.statLifeMax2 *= CrusherChip.EquippedLifeMaxMultiplier;

                // statDefense 是 DefenseStat 结构体（只有 implicit operator int），不能直接赋值，只能 *= 0f。
                Player.statDefense *= 0f;

                // 兜底：即使别的 ModPlayer 在本钩子之后把 statDefense 加回来，减免依然为 0。
                Player.DefenseEffectiveness *= 0f;
                return;
            }

            Player.GetDamage(DamageClass.Melee) += CrusherChip.FavoritedMeleeBonus;
        }

        public override void PostHurt(Player.HurtInfo info)
        {
            int level = Level;
            if (level <= 0 || info.Damage <= 0)
                return;

            float ratio = level >= 3
                ? Main.masterMode
                    ? CrusherChip.EquippedHealRatioMaster
                    : Main.expertMode
                        ? CrusherChip.EquippedHealRatioExpert
                        : CrusherChip.EquippedHealRatioNormal
                : CrusherChip.FavoritedHealRatio;

            // 收藏档的 5% 很容易被 (int) 截成 0，用概率取整保住小数部分。
            int heal = ValueUtils.ProbabilisticRound(info.Damage * ratio);
            if (heal > 0)
                Player.Heal(heal);
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.CrusherChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<CrusherChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "CrusherChipDynamic", body));
        }
    }
}
