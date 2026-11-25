using System;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 速射手芯片一族的结算逻辑，两种效果：
    /// <list type="bullet">
    /// <item><b>对空中目标增伤</b>：芯片档就有（贴「优先攻击空中单位」的方舟定义）。</item>
    /// <item><b>叠层削防</b>：只有<b>模组档</b>才有（收藏模组 5 点 / 装备模组 15 点），
    /// 每击中该 NPC 一次 +1，走 <c>HitModifiers.Defense.Flat</c>。</item>
    /// </list>
    /// <para/>
    /// 实例字段依赖 <see cref="InstancePerEntity"/>；该实例会在 <c>NPC.SetDefaults</c> 时被重建
    /// （<c>GlobalLoaderUtils.SetDefaults</c>），所以 NPC 复用/新生成时计数自动归零，不需要手动清。
    /// </summary>
    public class RapidFireChipGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>装备模组时的削减上限：15 点。</summary>
        public const int MaxDefenseReduction = 15;

        /// <summary>仅收藏模组时的削减上限：5 点。</summary>
        public const int FavoritedDefenseReduction = 5;

        /// <summary>
        /// 当前已累计削减的防御点数。
        /// 不保证 ≤ 上限：上限由玩家侧状态动态决定，读取时一律走 <see cref="GetTeamReductionCap"/> 取小值。
        /// </summary>
        public int defenseReduction;

        /// <summary>空中判定：飞行单位基本都带 <c>noGravity</c>（恶魔眼 / 蝙蝠 / 鸟妖 / 蜂后……）。</summary>
        private static bool IsAerial(NPC npc) => npc.noGravity;

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            ApplyAerialBonus(npc, player, ref modifiers);
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            ApplyAerialBonus(npc, Main.player[projectile.owner], ref modifiers);
        }

        /// <summary>
        /// 芯片一族共用的「对空中目标增伤」：按档位取装备值或收藏值，
        /// 乘在防御之后的 <see cref="NPC.HitModifiers.FinalDamage"/> 上（0 防御敌人也全吃）。
        /// </summary>
        private static void ApplyAerialBonus(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            if (!player.active || player.dead || player.ghost)
                return;
            if (!IsAerial(npc))
                return;

            RapidFireChipPlayer modPlayer = player.GetModPlayer<RapidFireChipPlayer>();
            int aerial = modPlayer.AerialLevel;
            if (aerial == 0)
                return;

            float bonus = aerial >= 3
                ? RapidFireChip.EquippedAerialBonus
                : RapidFireChip.FavoritedAerialBonus;

            modifiers.FinalDamage *= 1f + bonus;
        }

        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            Accumulate(npc, player);
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            // 无主弹幕的 owner 是 255，取 Main.player 之前必须先挡掉非玩家来源。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Accumulate(npc, Main.player[projectile.owner]);
        }

        /// <summary>
        /// 全队当前的削防上限：装备模组 15 点，仅收藏模组 5 点，其余（含只拿芯片）为 0。
        /// 层数挂在敌人身上全队共享，所以取队伍中的最大值。
        /// </summary>
        private static int GetTeamReductionCap()
        {
            int cap = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead || p.ghost)
                    continue;

                int module = p.GetModPlayer<RapidFireChipPlayer>().ModuleLevel;
                int playerCap = module == 4 ? MaxDefenseReduction
                    : module == 2 ? FavoritedDefenseReduction
                    : 0;

                if (playerCap > cap)
                    cap = playerCap;
            }

            return cap;
        }

        /// <summary>累加一层削防（只有持有模组才累加，受全队当前上限限制）。</summary>
        private void Accumulate(NPC npc, Player player)
        {
            if (!player.active || player.dead || player.ghost)
                return;

            // 削防是模组专属维度，只拿芯片时不叠层。
            if (player.GetModPlayer<RapidFireChipPlayer>().ModuleLevel == 0)
                return;

            int cap = GetTeamReductionCap();

            // 从「装备模组」退回「收藏模组」时，已有的层数要立刻被压回 5。
            if (defenseReduction > cap)
                defenseReduction = cap;

            if (defenseReduction >= cap)
                return;

            defenseReduction++;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // 取小值：上限被调低后，即便还没人再打它，这一击也按新上限算。
            int effective = Math.Min(defenseReduction, GetTeamReductionCap());
            if (effective <= 0)
                return;

            // 与原版 ichor（-15）/ betsysCurse（-40）同样是 Flat 削减。
            // 注意 HitModifiers.GetDamage 会把防御夹到 >= 0，所以这些点数对「0 防御」的敌人毫无收益。
            modifiers.Defense.Flat -= effective;
        }
    }
}
