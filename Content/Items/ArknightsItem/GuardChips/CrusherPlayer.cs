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
    /// 2. 防御有效性 ×0（防御不提供减伤）
    /// 3. 生命上限 ×2
    /// 4. 每1点防御转化为 +0.5% 近战伤害加成
    /// 5. 玩家自身近战伤害 ×1.75
    /// 6. 抗药性 -17%（药水冷却缩短）
    /// 7. 自然回血 +20%
    /// 8. 对团队阻挡目标额外 ×1.1 伤害
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
        public override void UpdateEquips()
        {
            // 空手 / 选中空格槽时 HeldItem 是空气物品，必须提前返回：
            // 空气物品没有 GlobalItem 实例，直接查询刻印会抛 KeyNotFoundException
            Item held = Player.HeldItem;
            if (held == null || held.IsAir)
                return;

            GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(held);
            if (subclass != GuardSubclass.Crusher)
                return;

            Player.PotionDelayModifier *= 1 - 0.17f; // 冷却 × 0.83，减 17%
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

            // ===== 防御有效性 ×0（无视防御减伤） =====
            // 乘算修改，保留难度基础语义；PvP 仍固定 0.5 不受影响
            Player.DefenseEffectiveness *= 0f;

            Player.GetDamage(DamageClass.Melee) += (int)Player.statDefense * 0.005f;
        }

        // ======================== 近战伤害加成 ========================

        /// <summary>
        /// 近战伤害：
        /// - 玩家自身伤害 ×1.75
        /// - 每1点防御（statDefense 最终有效值）额外 +0.5% 近战伤害
        /// </summary>
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (!HasCrusherWeaponEquipped) return;

            damage *= 1.75f;                                 // 自身伤害 ×1.75
        }

        // ======================== 生命上限 ×2 ========================

        /// <summary>
        /// 在 statLifeMax2（含装备/饰品加成）的基础上翻倍。
        /// 每帧 statLifeMax2 被重新计算，因此不会重复叠加。
        /// 防御有效性已由 DefenseEffectiveness ×0 处理，无需再改 statDefense。
        /// </summary>
        public override void PostUpdateMiscEffects()
        {
            if (!HasCrusherWeaponEquipped) return;

            Player.statLifeMax2 *= 2;
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

        // ======================== 攻速降低 ========================

        /// <summary>
        /// 攻速降低为原来的 60%（更慢）。
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            if (!HasCrusherWeaponEquipped) return 1f;
            return 0.6f;
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