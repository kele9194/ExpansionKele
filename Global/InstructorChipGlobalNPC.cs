using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 教官芯片的「目标印记」。
    /// <para/>
    /// 用非召唤攻击（含鞭子）命中敌人时给该敌人挂印记，印记期间所有召唤类弹幕对它的伤害乘以倍率。
    /// <para/>
    /// 状态跟着敌人走而不是跟着玩家走：这样「谁打的那只怪」和「谁受益」是同一个目标，
    /// 玩家能看见自己标记了谁，也不会出现「弹幕必须飞过召唤物」那种没法主动控制的判定。
    /// </summary>
    public class InstructorChipGlobalNPC : GlobalNPC
    {
        // GlobalType 默认 InstancePerEntity = false，带实例字段会被加载期禁用
        public override bool InstancePerEntity => true;

        /// <summary>印记剩余帧数。</summary>
        public int MarkTimer;

        /// <summary>印记期间的召唤伤害倍率。</summary>
        public float MarkMultiplier = 1f;

        public override void PostAI(NPC npc)
        {
            if (MarkTimer > 0)
                MarkTimer--;
        }

        /// <summary>按玩家当前档位刷新印记（不叠加，只刷新）。</summary>
        public void ApplyMark(Player player)
        {
            if (player is null || !player.active)
                return;

            int level = player.GetModPlayer<InstructorChipPlayer>().Level;

            if (level == 0)
                return;

            MarkTimer = InstructorChip.MarkDuration;
            MarkMultiplier = level >= 3
                ? InstructorChip.EquippedBuffMultiplier
                : InstructorChip.FavoritedBuffMultiplier;
        }

        /// <summary>没有弹幕的近战（剑本体）也算触发源。</summary>
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // 纯召唤武器（法杖）不负责标记；鞭子物品是 SummonMeleeSpeed，不相等所以放行
            if (item.DamageType == DamageClass.Summon)
                return;

            ApplyMark(player);
        }
    }
}
