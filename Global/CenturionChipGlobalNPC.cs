using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 强攻手芯片的「按距离排名额外增伤」。
    /// <para/>
    /// 只影响装备者自己造成的近战伤害：改的是本次命中的 <c>HitModifiers</c>，不碰 NPC 状态。
    /// <para/>
    /// 走<b>乘算</b>（<c>FinalDamage × (1 + bonus)</c>）而不是加算：
    /// 「第 3 近 +15%」应该在任何属性水平下都是 +15%，不该被玩家已有的增伤稀释。
    /// </summary>
    public class CenturionChipGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Apply(npc, player, ref modifiers);
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 无主弹幕的 owner 是 255，取 Main.player 之前必须先挡掉非玩家来源。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            // 只认近战弹幕；召唤、魔法、远程都不触发。
            if (!projectile.DamageType.CountsAsClass(DamageClass.Melee))
                return;

            Apply(npc, Main.player[projectile.owner], ref modifiers);
        }

        /// <summary>按圈内人数写入额外最终伤害。</summary>
        private static void Apply(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            if (!player.active || player.dead || player.ghost)
                return;

            CenturionChipPlayer modPlayer = player.GetModPlayer<CenturionChipPlayer>();
            int level = modPlayer.Level;
            if (level == 0)
                return;

            // 被打的目标自己必须在圈内；否则「圈内有 N 个敌人」与它无关。
            if (!CenturionChipPlayer.IsMarkableTarget(player, npc))
                return;

            int count = CenturionChipPlayer.GetNearbyCount(player);

            bool equipped = level >= 3;

            float bonus = count switch
            {
                1 => equipped ? CenturionChip.EquippedCrowd1Bonus : CenturionChip.FavoritedCrowd1Bonus,
                2 => equipped ? CenturionChip.EquippedCrowd2Bonus : CenturionChip.FavoritedCrowd2Bonus,
                > 2 => equipped ? CenturionChip.EquippedCrowd3Bonus : CenturionChip.FavoritedCrowd3Bonus,
                _ => 0f
            };

            if (bonus <= 0f)
                return;

            modifiers.FinalDamage *= 1f + bonus;
        }
    }
}
