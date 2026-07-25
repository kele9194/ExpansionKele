using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 剑豪玩家状态管理器。
    /// 
    /// 剑豪特性：
    /// 1. 挡2
    /// 2. 使用动画时间×0.7（攻速加快）
    /// 3. 武器伤害变为原来65%
    /// 4. 攻击附带15点破甲 + 10%减防
    /// 5. 最终伤害×1.1（不受距离影响）
    /// </summary>
    public class SwordmasterPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持剑豪刻印武器</summary>
        public bool HasSwordmasterWeaponEquipped { get; private set; } = false;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasSwordmasterWeaponEquipped = false;
        }

        /// <summary>
        /// 使用攻速*2
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            if (HasSwordmasterWeaponEquipped)
            {
                return 2f; // 攻速*2
            }
            return 1f;
        }

        /// <summary>
        /// 装备更新后执行：检查手持武器是否为剑豪刻印，
        /// 设置 BlockingCap = 2，并应用 65% 武器伤害。
        /// </summary>
        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasSwordmasterWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasSwordmasterWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Swordmaster)
            {
                HasSwordmasterWeaponEquipped = false;
                return;
            }

            HasSwordmasterWeaponEquipped = true;

            // ===== 同步阻挡（挡2） =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Swordmaster); // 挡2
            blocking.CurrentGuardSubclass = GuardSubclass.Swordmaster;

            // ===== 武器伤害 × 0.65 =====
            Player.GetModPlayer<ExpansionKeleDamageMulti>().MultiplyMultiplicativeDamageBonus(0.65f);
        }

        // ======================== 近战命中效果 ========================

        /// <summary>
        /// 近战挥砍命中时：
        /// - 15点破甲
        /// - 10%减防（防御效果×0.9）
        /// - 最终伤害×1.1（不受距离影响）
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasSwordmasterWeaponEquipped) return;

            // 15点破甲
            modifiers.ArmorPenetration += 15;

            // 10%减防
            modifiers.DefenseEffectiveness *= 0.9f;

            // 最终伤害×1.1（不受距离影响）
            modifiers.FinalDamage *= 1.1f;
        }
    }
}