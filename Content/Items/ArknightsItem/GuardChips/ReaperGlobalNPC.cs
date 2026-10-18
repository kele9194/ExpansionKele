using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 收割者 GlobalNPC：为每个 NPC 维护与收割者交互的永久标记。
    ///
    /// 1. HasAttackedReaper：该敌人攻击过收割者 → 对所有玩家的伤害永久 -15%
    /// 2. IsMarkedByReaper：该敌人被收割者攻击过 → 其对收割者的伤害永久 -10%（防守）
    ///
    /// "永久"实现：GlobalNPC 实例字段在整个 NPC 存活期间始终有效；
    /// 敌人重新生成时是新实例，标记自然清空。
    /// </summary>
    public class ReaperGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>该 NPC 是否攻击过收割者（团队共享，对所有玩家 -15%）</summary>
        public bool HasAttackedReaper { get; set; } = false;

        /// <summary>标记过该 NPC 的收割者玩家索引（每个收割者独立 -10%）</summary>
        public readonly HashSet<int> ReaperMarks = new HashSet<int>();

        // 接触伤害（团队减伤，不变）
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (!HasAttackedReaper) return;
            modifiers.FinalDamage *= 0.85f;
        }
    }

    /// <summary>
    /// 收割者 GlobalProjectile：敌人弹幕命中任意玩家时，
    /// 若发射它的 NPC 曾攻击过收割者，伤害 ×0.85（弹幕版团队减伤）。
    /// </summary>
    public class ReaperGlobalProjectile : GlobalProjectile
    {
        public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
        {
            // 反查发射该弹幕的来源 NPC
            if (!NPCProjectileSourceTracker.TryGetSourceNPC(projectile, out NPC sourceNPC)) return;
            if (!sourceNPC.GetGlobalNPC<ReaperGlobalNPC>().HasAttackedReaper) return;
            modifiers.FinalDamage *= 0.85f;
        }
    }
}