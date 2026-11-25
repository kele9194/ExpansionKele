using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.ArknightsItem.Materials;

namespace ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard
{
    /// <summary>
    /// 解放者芯片（近卫系）：不攻击时蓄积力量，一旦出手就把蓄积全部释放成一段解放窗口。
    /// <list type="bullet">
    /// <item><b>装备</b>：不挥动武器时每秒蓄积 0.16 解放值，上限 1.12（7 秒蓄满）；
    /// 蓄积期间随时可以用近战武器攻击，立刻进入 10 秒【解放】；
    /// 解放期间近战伤害 ×(1 + 解放值)，满蓄为 ×2.12；解放结束后解放值清零，2 秒后才能再次解放。</item>
    /// <item><b>未装备时放于背包并收藏</b>：充能改为「随时间自动」，每秒 0.04，上限 0.24（6 秒蓄满），
    /// 而且<b>不管在不在攻击都会照常蓄积</b>；满值自动进入 3 秒【解放】，满蓄 ×1.24。</item>
    /// </list>
    /// <para/>
    /// 这是把方舟解放者「平时不出手，技能期爆发」转成 TR 的手段：
    /// 蓄积条件刻意用 <c>itemAnimation</c> 而不是「命中」，因为原特性限制的就是「不出手」而不是「打不到人」。
    /// 蓄积期完全没有输出，所以倍率上限必须远高于回本点才不亏 ——
    /// 这是这套数值存在的理由，降低 <see cref="MaxCharge"/> 会直接让装备档变成负收益。
    /// <para/>
    /// 收藏档不采用「挥动充能」或「不攻击加速」，因为两种都会把玩家引向亏损玩法：
    /// 收藏档倍率只有 ×1.60、窗口只有 3 秒，停手凑满得到的收益远不及停手损失的输出。
    /// </summary>
    public class LiberatorChip : ModItem, IChargeableAccessories
    {
        /// <summary>装备档：解放值上限（满蓄时近战伤害 ×(1 + 本值) = ×2.12）。</summary>
        public const float MaxCharge = 1.12f;

        /// <summary>装备档：每秒蓄积的解放值（1.12 / 0.16 = 7 秒蓄满）。</summary>
        public const float ChargePerSecond = 0.16f;

        /// <summary>装备档：解放窗口持续帧数（600 帧 = 10 秒）。</summary>
        public const int LiberationDuration = 600;

        /// <summary>解放结束到能再次进入解放的冷却帧数（120 帧 = 2 秒）。</summary>
        public const int LiberationInterval = 120;

        /// <summary>收藏档：解放值上限（满蓄 ×1.24）。</summary>
        public const float FavoritedMaxCharge = 0.24f;

        /// <summary>收藏档：每秒自动蓄积的解放值（0.24 / 0.04 = 6 秒蓄满）。</summary>
        public const float FavoritedChargePerSecond = 0.04f;

        /// <summary>收藏档：解放窗口持续帧数（180 帧 = 3 秒）。</summary>
        public const int FavoritedLiberationDuration = 180;

        public override string LocalizationCategory => "Items.ArknightsItem.Accessories";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<LiberatorChipPlayer>().chipEquipped = true;

            if (player.whoAmI == Main.myPlayer)
                player.GetModPlayer<ChargeableAccessoriesPlayer>().RegisterAccessory(this);
        }

        public override void UpdateInventory(Player player)
        {
            if (!Item.favorited)
                return;

            player.GetModPlayer<LiberatorChipPlayer>().chipFavorited = true;

            // 收藏档同样需要解放值条：没有条玩家就不知道什么时候能解放
            if (player.whoAmI == Main.myPlayer)
                player.GetModPlayer<ChargeableAccessoriesPlayer>().RegisterAccessory(this);
        }

        public override void AddRecipes()
        {
            // 蘑菇矿锭：花后阶段，蓄力期完全无输出，需要足够高的武器基础伤害才撑得起倍率。
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GuardBaseChip>(), 1)
                .AddIngredient(ItemID.ShroomiteBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            LiberatorChipPlayer.AddDynamicTooltips(tooltips, Mod);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 返回的是「蓄满所需秒数」，不是解放值本身：解放值 0~1.12 对玩家没概念，
        /// 换成「还要 3.5 秒」才有用。
        /// </remarks>
        public float GetCurrentCharge()
        {
            Player player = Main.LocalPlayer;
            if (player is null || !player.active)
                return 0f;

            LiberatorChipPlayer modPlayer = player.GetModPlayer<LiberatorChipPlayer>();
            if (modPlayer.Level <= 0)
                return 0f;

            float perSecond = GetChargePerSecond(modPlayer);
            return perSecond <= 0f ? 0f : modPlayer.ChargeValue / perSecond;
        }

        /// <inheritdoc/>
        /// <remarks>同上，返回该档位蓄满所需的总秒数（装备档 7 秒 / 收藏档 6 秒）。</remarks>
        public float GetMaxCharge()
        {
            Player player = Main.LocalPlayer;
            if (player is null || !player.active)
                return 0f;

            LiberatorChipPlayer modPlayer = player.GetModPlayer<LiberatorChipPlayer>();
            if (modPlayer.Level <= 0)
                return 0f;

            float perSecond = GetChargePerSecond(modPlayer);
            return perSecond <= 0f ? 0f : modPlayer.ChargeCap / perSecond;
        }

        /// <inheritdoc/>
        public string GetAccessoryName()
        {
            return Language.GetTextValue(
                "Mods.ExpansionKele.Items.ArknightsItem.Accessories.LiberatorChip.DisplayName");
        }

        /// <summary>当前档位每秒蓄积的解放值，用于把解放值折算成秒。</summary>
        private static float GetChargePerSecond(LiberatorChipPlayer modPlayer)
        {
            return modPlayer.IsEquipped
                ? LiberatorChip.ChargePerSecond
                : LiberatorChip.FavoritedChargePerSecond;
        }
    }

    /// <summary>解放者芯片的状态：一档装备 / 一档收藏。</summary>
    public class LiberatorChipPlayer : ModPlayer
    {
        /// <summary>本帧是否装备了解放者芯片。</summary>
        public bool chipEquipped;

        /// <summary>本帧是否有解放者芯片被收藏在背包里。</summary>
        public bool chipFavorited;

        /// <summary>本帧是否装备了解放者芯片模组（预留）。</summary>
        public bool moduleEquipped;

        /// <summary>本帧是否有解放者芯片模组被收藏（预留）。</summary>
        public bool moduleFavorited;

        /// <summary>当前解放值，0 ~ <see cref="ChargeCap"/>。</summary>
        public float ChargeValue { get; private set; }

        /// <summary>上一帧是否在挥动武器，用于取「开始使用」的上升沿。</summary>
        private bool _wasItemActive;

        /// <summary>上一帧是否处于解放，用于检测解放到期。</summary>
        private bool _wasLiberationActive;

        /// <summary>距离能再次进入解放还剩的帧数。</summary>
        private int _cooldown;

        /// <summary>
        /// 生效档位：0 = 无，1 = 收藏芯片，2 = 收藏模组，3 = 装备芯片，4 = 装备模组。
        /// </summary>
        public int Level =>
            moduleEquipped ? 4 : chipEquipped ? 3 : moduleFavorited ? 2 : chipFavorited ? 1 : 0;

        /// <summary>是否处于装备档（3 / 4）。</summary>
        public bool IsEquipped => Level >= 3;

        /// <summary>当前档位的解放值上限。</summary>
        public float ChargeCap => IsEquipped ? LiberatorChip.MaxCharge : LiberatorChip.FavoritedMaxCharge;

        /// <summary>当前是否处于解放窗口内。</summary>
        public bool IsLiberationActive => Player.HasBuff(ModContent.BuffType<LiberatorBuff>());

        public override void ResetEffects()
        {
            chipEquipped = false;
            chipFavorited = false;
            moduleEquipped = false;
            moduleFavorited = false;
        }

        public override void PostUpdate()
        {
            // 卸下芯片时把状态机整体归零，避免下次装回来带着旧蓄力
            if (Level <= 0)
            {
                ChargeValue = 0f;
                _wasItemActive = false;
                _wasLiberationActive = false;
                _cooldown = 0;
                return;
            }

            // ---- 1. 解放到期：解放值清零并进入冷却 ----
            if (_wasLiberationActive && !IsLiberationActive)
            {
                ChargeValue = 0f;
                _cooldown = LiberatorChip.LiberationInterval;
            }

            if (_cooldown > 0)
                _cooldown--;

            bool usingItem = Player.ItemAnimationActive;

            // ---- 2. 蓄积 ----
            // 装备档：只有「不挥动」才涨；收藏档：随时间恒定涨，打不打都一样。
            float chargeRate = IsEquipped
                ? (usingItem ? 0f : LiberatorChip.ChargePerSecond)
                : LiberatorChip.FavoritedChargePerSecond;

            if (!IsLiberationActive)
                ChargeValue = Math.Min(ChargeCap, ChargeValue + chargeRate / 60f);

            // ---- 3. 进入解放 ----
            if (!IsLiberationActive && _cooldown <= 0)
            {
                bool triggered;

                if (IsEquipped)
                {
                    // 工具（镐 / 斧 / 锤）在 TR 里也算 Melee 且会走 itemAnimation，
                    // 不排掉的话挖矿会白烧掉一个解放窗口。
                    Item held = Player.HeldItem;
                    bool heldIsTool = held.IsAir || held.pick > 0 || held.axe > 0 || held.hammer > 0;

                    triggered = usingItem
                        && !heldIsTool
                        && Player.controlUseItem
                        && !_wasItemActive
                        && ChargeValue > 0f;
                }
                else
                {
                    // 收藏档没有「出手」这一步，蓄满就自动解放
                    triggered = ChargeValue >= ChargeCap;
                }

                if (triggered)
                {
                    int frames = IsEquipped
                        ? LiberatorChip.LiberationDuration
                        : LiberatorChip.FavoritedLiberationDuration;

                    Player.AddBuff(ModContent.BuffType<LiberatorBuff>(), frames);
                }
            }

            _wasItemActive = usingItem;
            _wasLiberationActive = IsLiberationActive;
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (Level <= 0 || ChargeValue <= 0f)
                return;

            if (item != Player.HeldItem)
                return;

            if (!item.CountsAsClass(DamageClass.Melee))
                return;

            // 挖矿 / 砍树不吃倍率，否则解放窗口会被工具白白消耗掉
            if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
                return;

            // 收藏档：蓄积期不给倍率，只在「解放」窗口内生效。
            // 否则倍率会随蓄积全程线性爬升，那 3 秒解放窗口反而看不出区别。
            // 装备档的蓄积期本来就要求停手，出手那一刻就是解放起点，不必再等 buff。
            if (!IsEquipped && !IsLiberationActive)
                return;

            damage *= 1f + ChargeValue;
        }

        /// <summary>
        /// 默认只显示当前生效的那一套（未激活时预览装备档），
        /// 按住 Shift 时同时展开装备与收藏两套完整效果。
        /// </summary>
        public static void AddDynamicTooltips(List<TooltipLine> tooltips, Mod mod)
        {
            const string baseKey = "Mods.ExpansionKele.Items.ArknightsItem.Accessories.LiberatorChip.";

            bool expanded = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            int level = Main.LocalPlayer.GetModPlayer<LiberatorChipPlayer>().Level;

            string head = Language.GetTextValue(baseKey + "InactiveHint");
            string equipped = Language.GetTextValue(baseKey + "EquippedDetailed");
            string favorited = Language.GetTextValue(baseKey + "FavoritedDetailed");
            string noStack = Language.GetTextValue(baseKey + "NoStackHint");

            string body = expanded
                ? head + "\n" + equipped + "\n" + favorited + "\n" + noStack
                : head + "\n" + (level == 1 || level == 2 ? favorited : equipped) + "\n" + noStack + "\n"
                    + Language.GetTextValue(baseKey + "ShiftHint");

            tooltips.Add(new TooltipLine(mod, "LiberatorChipDynamic", body));
        }
    }
}
