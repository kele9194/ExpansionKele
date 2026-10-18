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
    /// 3. 命中NPC时回复已损生命值2%（限定时间=武器useTime内最多回3次，保留33%距离下限）
    /// 4. 300px内每多一个敌人+6%伤害，最多+12%
    /// 5. 复仇印记：攻击收割者的敌人永久对所有玩家伤害-15%
    /// 6. 收割印记：收割者攻击过的敌人，其对该收割者的伤害永久-10%（防守）
    /// </summary>
    public class ReaperPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持收割者刻印武器</summary>
        public bool HasReaperWeaponEquipped { get; private set; } = false;

        /// <summary>限定时间窗口（帧），= 武器实际useTime（与武者回血冷却一致）</summary>
        private int healWindow = 0;

        /// <summary>当前窗口内已回血次数（最多3次，用完即锁）</summary>
        private int healsInWindow = 0;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasReaperWeaponEquipped = false;
        }

        /// <summary>
        /// 每帧递减限定时间窗口；窗口结束（计时归零）时重置回血次数。
        /// </summary>
        public override void PostUpdate()
        {
            if (healWindow > 0)
            {
                healWindow--;
                if (healWindow <= 0)
                    healsInWindow = 0;
            }
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

            Player.nebulaLevelLife = 0;

            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;

            Player.lifeRegenTime = 0;

            if (Player.lifeRegenCount > 0)
                Player.lifeRegenCount = 0;
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            if (!HasReaperWeaponEquipped) return;
            // 将自然再生系数设为零，完全禁止自然恢复
            regen *= 0f;
        }

        // ======================== 命中回血（已损2%，限定时间最多3次，保留33%距离下限） ========================

        /// <summary>
        /// 近战命中NPC时，恢复已损生命值2%的血量（与武者机制一致）。
        /// 限定时间（= 武器实际useTime）内最多回3次血；3次用完即锁定，直到窗口刷新。
        /// 保留距离衰减：128px内满效，128~320px线性衰减至33%，320px外仅33%。
        /// 
        /// 同时为命中的敌人打上【收割印记】——该敌人对收割者的伤害永久-10%（防守）。
        /// </summary>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasReaperWeaponEquipped) return;

            // ===== 收割印记：被收割者攻击的敌人，永久对收割者的伤害 -10% =====
            if (target != null && target.active)
                // OnHitNPC 中：记录"这个收割者"标记了它
            target.GetGlobalNPC<ReaperGlobalNPC>().ReaperMarks.Add(Player.whoAmI);
            int missingHealth = Player.statLifeMax2 - Player.statLife;
            if (missingHealth <= 0) return;

            // ─── 3次用完即锁：本窗口内已回满3次则锁定，直到窗口刷新 ───
            if (healsInWindow >= 3) return;

            // ─── 距离衰减（128px内满效，128~320px线性衰减至33%，320px外仅33%） ───
            float distance = Player.Center.Distance(target.Center);
            float distanceMult;
            if (distance <= 128f)
                distanceMult = 1f;
            else if (distance >= 320f)
                distanceMult = 0.33f;
            else
                distanceMult = 1f - (distance - 128f) / 192f * 0.67f;

            // ─── 治疗量 = 已损生命值 × 2% × 距离衰减（概率舍入） ───
            float healFloat = missingHealth * 0.03f * distanceMult;
            int healAmount = ValueUtils.ProbabilisticRound(healFloat);

            if (healAmount > 0)
            {
                Player.Heal(healAmount);
            }

            // 本次回血计入窗口，并重置窗口计时 = 武器实际useTime（与武者一致）
            healsInWindow++;
            Item held = Player.HeldItem;
            healWindow = (held != null && !held.IsAir)
                ? UseTimeHelper.GetActualUseTime(Player, held)
                : 10;
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

        // ======================== 复仇印记 + 收割印记减伤（近战） ========================

        /// <summary>
        /// 敌人近战命中收割者时：
        /// - 打上【复仇印记】：该敌人对所有玩家的伤害永久 -15%
        /// - 若该敌人已被【收割印记】标记（被收割者攻击过），其对收割者的伤害永久 -10%
        /// </summary>
        public override void ModifyHitByNPC(NPC attacker, ref Player.HurtModifiers modifiers)
        {
            if (!HasReaperWeaponEquipped) return;

            // ===== 复仇印记：攻击收割者的敌人，永久对所有玩家的伤害 -15% =====
            var rg = attacker.GetGlobalNPC<ReaperGlobalNPC>();
            rg.HasAttackedReaper = true;

            // ===== 收割印记减伤：被收割者攻击过的敌人，其近战对收割者伤害 -10% =====
            if (rg.ReaperMarks.Contains(Player.whoAmI))
                modifiers.FinalDamage *= 0.90f;
        }

        // ======================== 复仇印记 + 收割印记减伤（弹幕） ========================

        /// <summary>
        /// 敌人弹幕命中收割者时：
        /// - 打上【复仇印记】：该敌人对所有玩家的伤害永久 -15%
        /// - 若该敌人已被【收割印记】标记，其对收割者的伤害永久 -10%
        /// </summary>
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!HasReaperWeaponEquipped) return;
            if (!NPCProjectileSourceTracker.TryGetSourceNPC(proj, out NPC sourceNPC)) return;

            // ===== 复仇印记：敌人弹幕攻击收割者，永久对所有玩家的伤害 -15% =====
            var rg = sourceNPC.GetGlobalNPC<ReaperGlobalNPC>();
            rg.HasAttackedReaper = true;

            // ===== 收割印记减伤：被收割者攻击过的敌人，其弹幕对收割者伤害 -10% =====
            if (rg.ReaperMarks.Contains(Player.whoAmI))
                modifiers.FinalDamage *= 0.90f;
        }
    }
}