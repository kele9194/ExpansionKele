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
    /// 重剑手玩家状态管理器。
    /// 
    /// 重剑手特性（玻璃大炮，极限输出）：
    /// 1. 挡2
    /// 2. 防御 -50%
    /// 3. 近战伤害 ×1.5
    /// 4. 生命上限 +30%
    /// 5. 自然回血 +20%
    /// 6. 对团队阻挡目标额外 ×1.1 伤害
    /// </summary>
    public class CrusherPlayer : ModPlayer
    {
        /// <summary>当前帧是否手持重剑手刻印武器</summary>
        public bool HasCrusherWeaponEquipped { get; private set; } = false;

        // ======================== 生命周期 ========================

        public override void ResetEffects()
        {
            HasCrusherWeaponEquipped = false;
        }

        public override void PostUpdateEquips()
        {
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
            {
                HasCrusherWeaponEquipped = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(held);
            if (arkNum == 0)
            {
                HasCrusherWeaponEquipped = false;
                return;
            }

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Crusher)
            {
                HasCrusherWeaponEquipped = false;
                return;
            }

            HasCrusherWeaponEquipped = true;

            // ===== 同步阻挡（挡2） =====
            var blocking = Player.GetModPlayer<BlockingPlayer>();
            blocking.BlockingCap = BlockingSystem.GetBlockingCap(GuardSubclass.Crusher);
            blocking.CurrentGuardSubclass = GuardSubclass.Crusher;
        }

        // ======================== 防御 -50% ========================

        /// <summary>
        /// 总防御力（护甲+饰品+所有来源）效果减半。
        /// </summary>


        // ======================== 近战伤害 ×1.5 ========================

        /// <summary>
        /// 近战伤害提高至 150%。
        /// </summary>
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (!HasCrusherWeaponEquipped) return;

                damage *= 1.5f;

        }

        // ======================== 生命上限 +30% ========================

        /// <summary>
        /// 在 statLifeMax2（含装备/饰品加成）的基础上追加 +30%。
        /// 每帧 statLifeMax2 被重新计算，因此不会重复叠加。
        /// 防御减免到1/3
        /// </summary>
        public override void PostUpdateMiscEffects()
        {
            if (!HasCrusherWeaponEquipped) return;

            Player.statLifeMax2 += (int)(Player.statLifeMax2 * 0.30f);
            Player.statDefense /= 3;
        }

        // ======================== 自然回血 +20% ========================

        /// <summary>
        /// 自然生命再生速度提升 20%。
        /// </summary>
        public override void NaturalLifeRegen(ref float regen)
        {
            if (!HasCrusherWeaponEquipped) return;
            regen *= 1.20f;
        }

        // ======================== 阻挡增伤 ×1.1 ========================

        /// <summary>
        /// 对已被本队任意队友阻挡的目标额外造成 ×1.1 伤害。
        /// 叠加在 BlockingSystem 的通用阻挡增伤之上。
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasCrusherWeaponEquipped) return;

            var blocking = Player.GetModPlayer<BlockingPlayer>();
            if (blocking.IsInTeamBlockingSet(target))
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }
    }
}