using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 收割者芯片的「削敌」部分：被收割印记命中的敌人，10 秒内攻击与防御各降一档。
    /// <para/>
    /// 印记挂在 NPC 上（<c>InstancePerEntity</c>），所以是<b>队伍共享</b>的 ——
    /// 队里任何一个人打上印记，所有人打它都受益，符合「这只怪被削了」的语义。
    /// <para/>
    /// ⚠️ GlobalNPC 的实例字段不会自动网络同步，但伤害由服务端结算，
    /// 而服务端的 <c>PostAI</c> 与命中钩子同样会跑，所以服务端那一份计时器是准的。
    /// </summary>
    public class ReaperChipGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>印记剩余帧数。</summary>
        public int debuffTimer;

        /// <summary>印记当前的防御削减比例（0 = 无）。</summary>
        public float defenseReduction;

        /// <summary>印记当前的攻击削减比例（0 = 无）。</summary>
        public float attackReduction;

        /// <summary>
        /// 给该 NPC 挂上收割印记（可刷新）。
        /// <para/>
        /// 削减比例取<b>较大值</b>：装备档与收藏档玩家交替命中时，不会因为收藏档刷新而变弱。
        /// </summary>
        public static void ApplyDebuff(NPC npc, float defenseReduction, float attackReduction)
        {
            if (npc == null || !npc.active)
                return;

            ReaperChipGlobalNPC global = npc.GetGlobalNPC<ReaperChipGlobalNPC>();

            global.debuffTimer = ReaperChip.EffectDuration;
            global.defenseReduction = System.Math.Max(global.defenseReduction, defenseReduction);
            global.attackReduction = System.Math.Max(global.attackReduction, attackReduction);
        }

        public override void PostAI(NPC npc)
        {
            if (debuffTimer <= 0)
                return;

            if (--debuffTimer <= 0)
            {
                debuffTimer = 0;
                defenseReduction = 0f;
                attackReduction = 0f;
            }
        }

        /// <summary>印记期内，玩家直接命中它的伤害按「防御 −X%」结算。</summary>
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (debuffTimer <= 0 || defenseReduction <= 0f)
                return;

            modifiers.Defense *= 1f - defenseReduction;
        }

        /// <summary>印记期内，弹幕命中它的伤害按「防御 −X%」结算。</summary>
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (debuffTimer <= 0 || defenseReduction <= 0f)
                return;

            modifiers.Defense *= 1f - defenseReduction;
        }

        /// <summary>印记期内，它打玩家的伤害 −X%。</summary>
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (debuffTimer <= 0 || attackReduction <= 0f)
                return;

            modifiers.SourceDamage *= 1f - attackReduction;
        }
    }
}
