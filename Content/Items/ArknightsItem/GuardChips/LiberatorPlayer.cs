using System;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 解放者玩家状态管理器。
    ///
    /// 解放者特性（来自明日方舟设定）：
    /// 1. 充能：不使用物品时逐渐增加武器伤害倍率
    ///    - 每秒 +0.16 倍，上限 15s = +2.4 倍（总倍率 1 → 3.4）
    ///    - 切换到非解放者武器时保留充能值但不增加
    /// 2. 使用武器（挥动动画上升沿）时停止充能并进入【解放】状态 10s
    /// 3. 解放状态：维持当前伤害倍率不变，阻挡数 = 2（完全启用阻挡增伤/减伤）
    /// 4. 解放结束：伤害倍率归 1（充能清零），离开后 2s 内不能再次进入解放
    /// 5. 非解放状态不阻挡（阻挡数 0）
    /// </summary>
    public class LiberatorPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持解放者刻印武器</summary>
        public bool HasLiberatorWeaponEquipped { get; private set; } = false;

        /// <summary>当前是否处于解放状态（持有解放 buff）</summary>
        public bool IsLiberationActive => Player.HasBuff<LiberatorBuff>();

        /// <summary>当前充能值（0 ~ 2.4），伤害倍率 = 1 + 充能值</summary>
        public float ChargeValue { get; private set; } = 0f;



        // ======================== 常量 ========================

        /// <summary>充能值上限（倍率 +2.4）</summary>
        public const float MaxCharge = 2.4f;

        /// <summary>每秒充能值增量（倍率 +0.16/s）</summary>
        public const float ChargePerSecond = 0.16f;

        /// <summary>解放状态持续时长（帧）= 10s</summary>
        public const int LiberationDuration = 600;

        /// <summary>离开解放后的冷却（帧）= 2s</summary>
        public const int LiberationInterval = 120;

        // ======================== 私有状态 ========================

        /// <summary>上一帧是否在挥动武器（用于上升沿检测）</summary>
        private bool wasItemActive = false;

        /// <summary>上一帧是否处于解放状态（用于到期检测）</summary>
        private bool wasLiberationActive = false;

        /// <summary>离开解放后的冷却倒计时（帧）</summary>
        private int liberationCooldown = 0;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasLiberatorWeaponEquipped = false;
        }

        /// <summary>
        /// 装备更新后：检测手持武器是否为解放者刻印。
        /// 非解放状态不阻挡（BlockingCap=0），解放状态阻挡数=2。
        /// </summary>
        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasLiberatorWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasLiberatorWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Liberator)
            {
                HasLiberatorWeaponEquipped = false;
                return;
            }

            HasLiberatorWeaponEquipped = true;

            // ===== 同步阻挡：解放状态挡2，非解放不阻挡 =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.CurrentGuardSubclass = GuardSubclass.Liberator;
            blocking.BlockingCap = IsLiberationActive
                ? BlockingSystem.GetBlockingCap(GuardSubclass.Liberator) // 挡2
                : 0;
        }

        /// <summary>
        /// 每帧驱动解放者状态机：
        /// 1. 解放到期 → 倍率归1并进入2s冷却
        /// 2. 充能（持解放者武器且未使用任何物品）
        /// 3. 使用武器（挥动动画上升沿）→ 进入解放
        /// </summary>
        public override void PostUpdate()
        {
            // ---- 1. 解放状态到期检测 ----
            if (wasLiberationActive && !IsLiberationActive)
            {
                ChargeValue = 0f;                      // 倍率归 1
                liberationCooldown = LiberationInterval; // 记录 2s 冷却
            }
            if (liberationCooldown > 0)
                liberationCooldown--;

            // ---- 2. 充能 ----
            if (HasLiberatorWeaponEquipped
                && !IsLiberationActive
                && !Player.ItemAnimationActive)        // 不使用任何物品
            {
                ChargeValue = Math.Min(MaxCharge, ChargeValue + ChargePerSecond / 60f);
            }

            // ---- 3. 使用武器 → 进入解放 ----
            if (HasLiberatorWeaponEquipped
                && !IsLiberationActive
                && liberationCooldown <= 0
                && Player.ItemAnimationActive
                && Player.controlUseItem               // 按住使用键（排除右键药水等）
                && !wasItemActive)                     // 挥动动画上升沿
            {
                Player.AddBuff(ModContent.BuffType<LiberatorBuff>(), LiberationDuration);
            }

            // 更新状态记录
            wasItemActive = Player.ItemAnimationActive;
            wasLiberationActive = IsLiberationActive;
        }

        // ======================== 伤害倍率 ========================

        /// <summary>
        /// 仅对当前手持的近战武器应用伤害倍率（1 + 充能值）。
        /// </summary>
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (!HasLiberatorWeaponEquipped) return;
            if (item != Player.HeldItem) return;
            if (ArknightsGlobalItem.GetGuardSubclass(item) != GuardSubclass.Liberator) return;
            if (ChargeValue <= 0f) return;

            damage *= 1f + ChargeValue;
        }
    }
}