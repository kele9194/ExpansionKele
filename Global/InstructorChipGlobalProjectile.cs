using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 教官芯片的弹幕侧逻辑。
    /// <list type="bullet">
    /// <item>非召唤弹幕（含鞭子）命中敌人 → 给敌人挂印记</item>
    /// <item>召唤类弹幕命中带印记的敌人 → 最终伤害乘以印记倍率</item>
    /// </list>
    /// <para/>
    /// 判定统一走 <see cref="DamageClass.CountsAsClass"/>：<c>SummonMeleeSpeed</c>（鞭子）
    /// 对 <c>Summon</c> 做了效果继承，所以鞭子天然算作召唤类，既能吃增伤、又不至于被排除在触发之外。
    /// <para/>
    /// ⚠️ 召唤类弹幕默认不打印记（否则自产自销、永久常驻），唯独鞭子例外：
    /// 它既是触发器也是受益者。
    /// <para/>
    /// ⚠️ 本类不带实例字段 —— <c>GlobalType</c> 默认 <c>InstancePerEntity = false</c>。
    /// </summary>
    public class InstructorChipGlobalProjectile : GlobalProjectile
    {
        /// <summary>召唤类弹幕（含鞭子）打带印记的敌人时增伤。</summary>
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!projectile.friendly || !target.active || target.friendly || target.immortal)
                return;

            if (!projectile.DamageType.CountsAsClass(DamageClass.Summon))
                return;

            InstructorChipGlobalNPC mark = target.GetGlobalNPC<InstructorChipGlobalNPC>();

            if (mark.MarkTimer <= 0)
                return;

            modifiers.FinalDamage *= mark.MarkMultiplier;
        }

        /// <summary>非召唤弹幕与鞭子命中敌人时给敌人挂印记。</summary>
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!projectile.friendly || !target.active || target.friendly || target.immortal)
                return;

            // 召唤类不自己打印记，除非是鞭子（鞭子既触发也受益）
            if (projectile.DamageType.CountsAsClass(DamageClass.Summon) && !IsWhipProjectile(projectile))
                return;

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            target.GetGlobalNPC<InstructorChipGlobalNPC>().ApplyMark(Main.player[projectile.owner]);
        }

        /// <summary>
        /// 鞭子弹幕：原版靠 <see cref="ProjectileID.Sets.IsAWhip"/>，
        /// 模组鞭子忘了登记该集合时靠伤害类型兜底。
        /// </summary>
        private static bool IsWhipProjectile(Projectile projectile)
        {
            return ProjectileID.Sets.IsAWhip[projectile.type]
                || projectile.DamageType == DamageClass.SummonMeleeSpeed;
        }
    }
}
