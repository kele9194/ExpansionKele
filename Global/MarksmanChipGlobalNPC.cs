using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 神射手芯片的「低防目标额外暴击伤害」。
    /// <para/>
    /// 判定放在 <c>ModifyHitByProjectile</c> 而不是 <c>ModifyIncomingHit</c>：
    /// 后者的上下文里拿不到「这一击的弹幕伤害」，也就无法算 40% 的浮动阈值。
    /// <para/>
    /// 只对弹幕生效 —— 无弹幕的直接命中（<c>ModifyHitByItem</c>）不会触发，这是刻意的。
    /// </summary>
    public class MarksmanChipGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 无主弹幕的 owner 是 255，取 Main.player 之前必须先挡掉非玩家来源。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player player = Main.player[projectile.owner];
            if (!player.active || player.dead || player.ghost)
                return;

            MarksmanChipPlayer modPlayer = player.GetModPlayer<MarksmanChipPlayer>();

            float critDamageBonus;
            if (modPlayer.equipped)
                critDamageBonus = MarksmanChip.EquippedCritDamageBonus;
            else if (modPlayer.favorited)
                critDamageBonus = MarksmanChip.FavoritedCritDamageBonus;
            else
                return;

            // 低防判定：不设上下限，纯按「防御 < 本次伤害 × 40%」比较。
            if (npc.defense >= projectile.damage * MarksmanChip.FragileDefenseRatio)
                return;

            // CritDamage 默认 2.0x，+0.25f 即「额外 25% 暴击伤害」（2.25x）。
            // 注意它只在暴击时参与结算（NPC.HitModifiers.GetDamage 里 crit ? CritDamage : NonCritDamage）。
            modifiers.CritDamage += critDamageBonus;
        }
    }
}
