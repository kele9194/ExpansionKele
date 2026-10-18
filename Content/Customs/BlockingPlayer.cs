using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.GuardChips;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 阻挡机制玩家状态管理器（ModPlayer）。
    /// 每个玩家独立维护：
    /// - 当前阻挡上限（由近卫子职业决定）
    /// - 当前阻挡名单（每帧自动刷新）
    /// - 增伤/减伤系数的自动应用
    /// 
    /// 工作流程：
    /// 1. ResetEffects() → 每帧重置阻挡上限（由武器芯片设置）
    /// 2. PostUpdate() → 每帧重新计算阻挡名单
    /// 3. ModifyHitNPC() → 攻击名单内敌人时附加增伤
    /// 4. ModifyHitByNPC() → 被名单内敌人攻击时附加减伤
    /// </summary>
    public class BlockingPlayer : ModPlayer
    {
        // ======================== 公共属性 ========================

        /// <summary>当前阻挡上限（0 = 无阻挡能力）</summary>
        public int BlockingCap { get; set; } = 0;

        /// <summary>当前近卫子职业（由手持武器的刻印决定）</summary>
        public GuardSubclass CurrentGuardSubclass { get; set; } = GuardSubclass.Unassigned;

        /// <summary>当前重装子职业（由手持武器的刻印决定）</summary>
public DefenderSubclass CurrentDefenderSubclass { get; set; } = DefenderSubclass.Unassigned;

        /// <summary>当前帧的阻挡名单（只读，外部不要修改）</summary>
        public List<NPC> CurrentBlockingList { get; private set; } = new List<NPC>();

        // ======================== 生命周期 ========================

        /// <summary>
        /// 每帧重置临时状态。
        /// 阻挡上限和子职业由武器刻印在 ModifyWeaponDamage 等时机设置。
        /// </summary>
        public override void ResetEffects()
        {
            BlockingCap = 0;
            CurrentGuardSubclass = GuardSubclass.Unassigned;
            CurrentDefenderSubclass = DefenderSubclass.Unassigned;
        }

        /// <summary>
        /// 每帧更新后重新计算阻挡名单。
        /// </summary>
        public override void PostUpdate()
        {
            if (BlockingCap > 0)
            {
                CurrentBlockingList = BlockingSystem.GetBlockingList(Player, BlockingCap);
            }
            else
            {
                CurrentBlockingList.Clear();
            }
        }

        // ======================== 查询方法 ========================

        /// <summary>
        /// 检查目标 NPC 是否在当前阻挡名单中。
        /// </summary>
        public bool IsInBlockingList(NPC npc)
        {
            return CurrentBlockingList.Contains(npc);
        }

        // 原来的方法全部不动！
        // GetDamageBonus / GetDamageReduction / ModifyHitNPC / ModifyHitByNPC / ModifyHitByProjectile
        // 都还是用 IsInBlockingList（个人名单）

        /// <summary>
        /// 新增：检查目标是否被本队任意队友阻挡（用于其他机制）。
        /// </summary>
        public bool IsInTeamBlockingSet(NPC npc)
        {
            return BlockingSystem.IsInTeamBlockingSet(npc, Player.team);
        }

        /// <summary>
        /// 获取对指定目标的实时增伤系数（考虑距离衰减）。
        /// </summary>
        /// <param name="target">目标 NPC</param>
        /// <returns>增伤乘数（如 0.20 = +20%），不在名单中则返回 0</returns>
        public float GetDamageBonus(NPC target)
        {
            if (!IsInBlockingList(target)) return 0f;

            float dist = Vector2.Distance(Player.Center, target.Center);
            float factor = BlockingSystem.GetEffectFactor(dist);
            float maxBonus = BlockingSystem.GetMaxDamageBonus(BlockingCap);

            return maxBonus * factor;
        }

        /// <summary>
        /// 获取来自指定攻击者的实时减伤系数（考虑距离衰减）。
        /// </summary>
        /// <param name="attacker">攻击者 NPC</param>
        /// <returns>减伤乘数（如 0.15 = 减少15%伤害），不在名单中则返回 0</returns>
        public float GetDamageReduction(NPC attacker)
        {
            if (!IsInBlockingList(attacker)) return 0f;

            float dist = Vector2.Distance(Player.Center, attacker.Center);
            float factor = BlockingSystem.GetEffectFactor(dist);
            float maxReduction = BlockingSystem.GetMaxDamageReduction(BlockingCap);

            return maxReduction * factor;
        }

        /// <summary>
        /// 获取减伤拆分（含阻挡补偿）：返回 (防御前减伤, 防御后减伤)。
        /// 挡3/挡4 每少挡1个目标，将5%防御后减伤转化为防御前减伤，
        /// 转化量随阻挡效果系数（距离衰减）等比缩放。
        /// </summary>
        /// <param name="attacker">攻击者 NPC</param>
        /// <returns>元组：(beforeDefense 防御前减伤, afterDefense 防御后减伤)</returns>
        public (float beforeDefense, float afterDefense) GetDamageReductionSplit(NPC attacker)
        {
            if (!IsInBlockingList(attacker)) return (0f, 0f);

            float dist = Vector2.Distance(Player.Center, attacker.Center);
            float factor = BlockingSystem.GetEffectFactor(dist);
            float maxReduction = BlockingSystem.GetMaxDamageReduction(BlockingCap);
            float total = maxReduction * factor;
            if (total <= 0f) return (0f, 0f);

            float converted = BlockingSystem.GetUnderBlockConvertedReduction(BlockingCap, CurrentBlockingList.Count, factor);
            float beforeDefense = Math.Min(converted, total); // 保险起见保留上限（正常情况不会触发）
            return (beforeDefense, total - beforeDefense);
        }

        // ======================== 伤害挂钩 ========================

        /// <summary>
        /// 玩家攻击NPC时：如果目标在阻挡名单中，附加通用增伤和挡2补偿。
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float dist = Vector2.Distance(Player.Center, target.Center);
            float factor = BlockingSystem.GetEffectFactor(dist);
            float bonus = GetDamageBonus(target)
                        + BlockingSystem.GetUnderBlockDamageBonus(BlockingCap, CurrentBlockingList.Count, factor);
            if (bonus != 0f)
            {
                modifiers.FinalDamage *= 1f + bonus;
            }
        }

        /// <summary>
        /// 玩家被NPC攻击时：如果攻击者在阻挡名单中，附加减伤。包括补偿
        /// </summary>
        public override void ModifyHitByNPC(NPC attacker, ref Player.HurtModifiers modifiers)
        {
            var (beforeDefense, afterDefense) = GetDamageReductionSplit(attacker);
            if (beforeDefense > 0f)
                modifiers.SourceDamage *= 1f - beforeDefense;
            if (afterDefense > 0f)
                modifiers.FinalDamage *= 1f - afterDefense;
        }

        /// <summary>
        /// 玩家被弹幕击中时：如果弹幕来源于阻挡名单中的NPC，附加减伤。包括补偿
        /// </summary>
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (NPCProjectileSourceTracker.TryGetSourceNPC(proj, out NPC sourceNPC))
            {
                var (beforeDefense, afterDefense) = GetDamageReductionSplit(sourceNPC);
                if (beforeDefense > 0f)
                    modifiers.SourceDamage *= 1f - beforeDefense;
                if (afterDefense > 0f)
                    modifiers.FinalDamage *= 1f - afterDefense;
            }
        }
    }
}