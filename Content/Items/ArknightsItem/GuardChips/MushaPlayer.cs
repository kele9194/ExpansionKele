using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 武者玩家状态管理器。
    /// 
    /// 武者特性（来自明日方舟设定）：
    /// 1. 挡1
    /// 2. 抑制自然生命再生（lifeRegen = 0, lifeRegenTime = 0）
    /// 3. 每次命中回复已损生命值5%，冷却=武器使用时间（useTime）
    /// 4. 50%血量以下获得20%减伤
    /// 5. 根据已损血量百分比（最高70%）线性获得最高50%攻速加成（useSpeed）
    /// 6. 死亡触发复生（回复30%最大生命值），冷却10分钟，期间再死则真正死亡
    /// </summary>
    public class MushaPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持武者刻印武器</summary>
        public bool HasMushaWeaponEquipped { get; private set; } = false;

        /// <summary>回血冷却计数器（帧），冷却=武器useTime</summary>
        private int healCooldown = 0;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasMushaWeaponEquipped = false;
        }

        /// <summary>每帧递减回血冷却</summary>
        public override void PostUpdate()
        {
            if (healCooldown > 0)
                healCooldown--;
        }

        /// <summary>
        /// 装备更新后执行：检查手持武器是否为武者刻印，
        /// 设置 BlockingCap = 1。
        /// </summary>
        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasMushaWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasMushaWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Musha)
            {
                HasMushaWeaponEquipped = false;
                return;
            }

            HasMushaWeaponEquipped = true;

            // ===== 同步阻挡（挡1） =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Musha); // 挡1
            blocking.CurrentGuardSubclass = GuardSubclass.Musha;
        }

        // ======================== 抑制自然生命再生 ========================

        /// <summary>
        /// 手持武者武器时，抑制自然生命再生。
        /// </summary>
        public override void UpdateBadLifeRegen()
        {
            if (!HasMushaWeaponEquipped) return;   // ← 补上这一行

            Player.nebulaLevelLife = 0;

            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;

            Player.lifeRegenTime = 0;

            if (Player.lifeRegenCount > 0)
                Player.lifeRegenCount = 0;
        }

        public override void NaturalLifeRegen(ref float regen)
        {
            if (!HasMushaWeaponEquipped) return;
            regen *= 0f;
        }

        // ======================== 命中回血（已损5%，冷却=武器useTime） ========================

        /// <summary>
        /// 近战命中NPC时，回复已损生命值5%的血量。
        /// 冷却时间 = 手持武器的 base useTime（帧数）。
        /// 带概率舍入处理小数部分。
        /// </summary>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasMushaWeaponEquipped) return;
            if (healCooldown > 0) return;

            int missingHealth = Player.statLifeMax2 - Player.statLife;
            if (missingHealth <= 0) return;

            // ─── 距离衰减（128px内满效，128~320px衰减至33%，320px外仅33%） ───
            float distance = Player.Center.Distance(target.Center);
            float distanceMult;
            if (distance <= 128f)
                distanceMult = 1f;
            else if (distance >= 320f)
                distanceMult = 0.33f;
            else
                distanceMult = 1f - (distance - 128f) / 192f * 0.67f;

            // 已损血量 × 5% × 距离衰减，概率舍入
            float healFloat = missingHealth * 0.05f * distanceMult;
            int healAmount = ValueUtils.ProbabilisticRound(healFloat);

            if (healAmount > 0)
            {
                Player.Heal(healAmount);
            }

            // 设置冷却 = 手持武器的使用时间
        // 设置冷却 = 武器的实际使用时间（考虑所有攻速修正）
        Item held = Player.HeldItem;
        healCooldown = (held != null && !held.IsAir)
        ? UseTimeHelper.GetActualUseTime(Player, held)
        : 10;
        }

        // ======================== 50%血以下 -20%减伤 ========================

        /// <summary>
        /// 当前生命值 ≤ 50% 最大生命值时，获得20%减伤。
        /// </summary>
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!HasMushaWeaponEquipped) return;

            if (Player.statLife <= Player.statLifeMax2 * 0.5f)
            {
                modifiers.FinalDamage *= 0.80f; // 减少20%伤害
            }
        }

        // ======================== 线性攻速加成（最高50%） ========================

        /// <summary>
        /// 根据已损血量百分比（最高70%）线性提升最高50%攻速。
        /// 公式：bonus = (min(missingRatio, 0.70) / 0.70) × 0.50
        /// 满血时 0% 攻速，70%已损时 50% 攻速。
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            if (!HasMushaWeaponEquipped) return 1f;

            float missingRatio = 1f - (float)Player.statLife / Player.statLifeMax2;
            missingRatio = Math.Min(missingRatio, 0.70f); // 最高70%
            float speedBonus = (missingRatio / 0.70f) * 0.50f; // 线性映射到 0%~50%

            return 1f + speedBonus;
        }

        // ======================== 复活机制（10分钟冷却） ========================

        /// <summary>
        /// 即将死亡时触发复活判定：
        /// - 如果带有 <see cref="MushaRevivalCooldown"/> 冷却Debuff → 真正死亡
        /// - 否则 → 回复30%最大生命值并施加36000帧（10分钟）冷却Debuff
        /// </summary>
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (!HasMushaWeaponEquipped) return true;

            // 冷却中 → 真正死亡
            if (Player.HasBuff<MushaRevivalCooldown>())
                return true;

            // 复活：回复30%最大生命值
            Player.statLife = Math.Max(1, (int)(Player.statLifeMax2 * 0.30f));

            // 施加10分钟冷却（60帧/秒 × 600秒 = 36000帧）
            Player.AddBuff(ModContent.BuffType<MushaRevivalCooldown>(), 36000);

            // 音效 + 粒子效果
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item29, Player.position);
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height,
                    Terraria.ID.DustID.HealingPlus, 0f, -2f, 100, default, 1.5f);
            }

            return false; // 取消死亡
        }
    }
}