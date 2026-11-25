using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 攻城手芯片的「对大型目标增伤」。
    /// <para/>
    /// 只影响装备者自己造成的伤害，改的是本次命中的 <c>HitModifiers</c>，
    /// 不碰 NPC 状态，所以队友与其他伤害来源不受影响。
    /// <para/>
    /// 体型用 <c>npc.width + npc.height</c> 衡量并线性换算：僵尸 58 无增益、
    /// 蚁狮 100 约四成、克苏鲁之眼 200 约六成、骷髅王 240 约七成五，300 以上吃满。
    /// </summary>
    public class SiegeChipGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            Apply(npc, player, ref modifiers);
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 无主弹幕的 owner 是 255，取 Main.player 之前必须先挡掉非玩家来源。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Apply(npc, Main.player[projectile.owner], ref modifiers);
        }

        /// <summary>体型越大增伤越高：宽+高 60 起从 0% 线性提升，到 300 时吃满。</summary>
        private static void Apply(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            if (!player.active || player.dead || player.ghost)
                return;

            int size = npc.width + npc.height;
            float t = MathHelper.Clamp(
                (size - SiegeChip.MinLargeSize) / (float)(SiegeChip.MaxLargeSize - SiegeChip.MinLargeSize),
                0f, 1f);
            if (t <= 0f)
                return;

            SiegeChipPlayer modPlayer = player.GetModPlayer<SiegeChipPlayer>();
            if (modPlayer.Level == 0)
                return;

            float maxBonus = modPlayer.IsEquipped
                ? SiegeChip.EquippedMaxBonus
                : SiegeChip.FavoritedMaxBonus;

            modifiers.FinalDamage *= 1f + maxBonus * t;
        }
    }
}
