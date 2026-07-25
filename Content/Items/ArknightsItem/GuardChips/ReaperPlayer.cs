using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using System;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 收割者玩家状态管理器。
    /// 
    /// 收割者特性：
    /// 1. 挡2
    /// 2. 抑制自然生命再生（lifeRegen = 0, lifeRegenTime = 0）
    /// 3. 命中NPC时回复已损生命值（非线性 + 衰减平滑恢复）
    /// 4. 300px内每多一个敌人+6%伤害，最多+12%
    /// </summary>
    public class ReaperPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持收割者刻印武器</summary>
        public bool HasReaperWeaponEquipped { get; private set; } = false;

        /// <summary>命中衰减系数（1.0=无衰减，随连续命中降低，随帧恢复至1.0）</summary>
        private float streakDecay = 1f;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasReaperWeaponEquipped = false;
        }

        /// <summary>
        /// 每帧向 1.0 恢复衰减系数，约 1.5 秒完全恢复（假设 60fps）。
        /// </summary>
        public override void PostUpdate()
        {
            if (streakDecay < 1f)
                streakDecay = Math.Min(1f, streakDecay + 0.02f);
        }

        /// <summary>
        /// 装备更新后执行：检查手持武器是否为收割者刻印，
        /// 设置 BlockingCap = 2。
        /// </summary>
        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasReaperWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasReaperWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Reaper)
            {
                HasReaperWeaponEquipped = false;
                return;
            }

            HasReaperWeaponEquipped = true;

            // ===== 同步阻挡（挡2） =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Reaper); // 挡2
            blocking.CurrentGuardSubclass = GuardSubclass.Reaper;
        }

        // ======================== 抑制生命再生 ========================

        /// <summary>
        /// 手持收割者武器时，抑制自然生命再生：
        /// - 将 lifeRegen 设为 0（防止正面再生）
        /// - 将 lifeRegenTime 设为 0（防止再生时间累积）
        /// </summary>
        public override void UpdateBadLifeRegen()
        {
            if (!HasReaperWeaponEquipped) return;

            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;

            Player.lifeRegenTime = 0;
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            if (!HasReaperWeaponEquipped) return;
            // 将自然再生系数设为零，完全禁止自然恢复
            regen *= 0f;
        }

        // ======================== 命中回血（双重非线性 + 平滑衰减恢复） ========================

        /// <summary>
        /// 近战命中NPC时，恢复已损生命值，采用双重非线性约束：
        /// ① 已损比例 ^0.8 — 残血时收益边际递减
        /// ② 命中衰减系数 — 连续快攻惩罚重，未命中时每帧自然恢复
        /// </summary>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasReaperWeaponEquipped) return;

            int missingHealth = Player.statLifeMax2 - Player.statLife;
            if (missingHealth <= 0) return;

            // ─── ① 已损生命非线性（^0.8） ───
            float missingRatio = 1f - (float)Player.statLife / Player.statLifeMax2;
            float nonLinearFactor = MathF.Pow(missingRatio, 0.8f);

            // ─── ③ 距离衰减（128px内满效，128~320px衰减至33%，320px外仅33%） ───
            float distance = Player.Center.Distance(target.Center);
            float distanceMult;
            if (distance <= 128f)
                distanceMult = 1f;
            else if (distance >= 320f)
                distanceMult = 0.33f;
            else
                distanceMult = 1f - (distance - 128f) / 192f * 0.67f;

            // ─── ④ 最终治疗量 ───
            float healFloat = nonLinearFactor * Player.statLifeMax2 * 0.03f * streakDecay * distanceMult;
            int healAmount = ValueUtils.ProbabilisticRound(healFloat);

            if (healAmount > 0)
            {
                Player.Heal(healAmount);
            }

            // 命中后衰减 ×0.8，保底 0.2
            streakDecay = Math.Max(0.2f, streakDecay * 0.8f);
        }

        // ======================== 群战增伤（300px内每多一个敌人+6%，最多12%） ========================

        /// <summary>
        /// 300px内每多一个敌人，伤害+6%，最多+12%。
        /// 鼓励冲入敌阵战斗。
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasReaperWeaponEquipped) return;

            int nearbyCount = 0;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                //if (npc.whoAmI == target.whoAmI) continue;   // 排除当前目标
                if (!npc.CanBeChasedBy()) continue;           // 可被追踪
                if (npc.friendly) continue;                    // 排除友方
                if (npc.Distance(Player.Center) > 300f) continue; // 超出300px
                nearbyCount++;
            }

            // 每多一个+6%，最多+12%（即最多计算2个额外敌人）
            int bonusPercent = Math.Min(nearbyCount, 2) * 6;
            if (bonusPercent > 0)
            {
                modifiers.FinalDamage *= 1f + bonusPercent / 100f;
            }
        }
    }
}