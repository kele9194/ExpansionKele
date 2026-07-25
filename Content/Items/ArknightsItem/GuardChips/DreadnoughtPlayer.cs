using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 无畏者玩家状态管理器。
    /// 
    /// 职责（每个芯片子职业的 Player 各自管理自己的逻辑）：
    /// 1. PostUpdateEquips() → 检查手持武器是否为无畏者刻印，设置 BlockingCap 和子职业
    /// 2. ModifyHitNPC() → 对阻挡目标施加无畏者专属 1.15x 增伤
    /// 3. PreKill() → 受到致命伤害时触发无畏重生
    /// 4. PostUpdateRun() → 检测重生Buff到期，施加6分钟冷却Debuff
    /// </summary>
    public class DreadnoughtPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持无畏者刻印武器</summary>
        public bool HasDreadnoughtWeaponEquipped { get; private set; } = false;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasDreadnoughtWeaponEquipped = false;
        }

        /// <summary>
        /// 装备更新后执行：检查手持武器是否为无畏者刻印，
        /// 并将阻挡状态同步到 BlockingPlayer。
        /// </summary>
        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasDreadnoughtWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasDreadnoughtWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Dreadnought)
            {
                HasDreadnoughtWeaponEquipped = false;
                return;
            }

            HasDreadnoughtWeaponEquipped = true;

            // ===== 同步到 BlockingPlayer =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Dreadnought); // = 1
            blocking.CurrentGuardSubclass = GuardSubclass.Dreadnought;

        }

        // ======================== 专属 1.15x 阻挡增伤 ========================

        /// <summary>
        /// 无畏者对阻挡名单内的敌人造成 15% 额外伤害（1.15x）。
        /// 覆盖 BlockingSystem 默认的 20%（挡1通用值）。
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasDreadnoughtWeaponEquipped) return;

           var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (!blocking.IsInTeamBlockingSet(target)) return;  // ← 改为团队版
            
            float dreadnoughtBonus = 0.15f; // 无畏者专属 15%
            modifiers.FinalDamage *= 1f + dreadnoughtBonus;//不受距离影响

        }

        // ======================== 无畏重生机制 ========================

        /// <summary>
        /// 玩家即将死亡时触发重生逻辑。
        /// </summary>
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (!HasDreadnoughtWeaponEquipped) return true;

            // 重生冷却中 → 正常死亡
            if (Player.HasBuff<DreadnoughtRebirthBuff>())
                return true;

            // 已经处于重生状态 → 真正死亡（不干预）
            if (!Player.HasBuff<DreadnoughtRebirthBuff>()){
                Player.AddBuff(ModContent.BuffType<DreadnoughtRebirthBuff>(),21600);
                Player.statLife = Player.statLifeMax2;
                // 3. 音效 + 粒子
                Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item29, Player.position);
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDust(Player.position, Player.width, Player.height,
                        Terraria.ID.DustID.GoldFlame, 0f, -2f, 100, default, 1.5f);
                }
                return false;
            }
            return true;
        }

    }
}