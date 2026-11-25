using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 重射手芯片的「贴身破甲 + 击退增强」。
    /// <para/>
    /// <b>只影响装备者自己造成的伤害</b>：全部改在本次命中的 <c>HitModifiers</c> 上，
    /// 不写 <c>npc.defense</c>、不碰 NPC 状态，所以队友与其他伤害来源完全不受影响。
    /// <para/>
    /// 距离越近越强：命中瞬间弹幕到玩家的距离在 200~400 像素之间线性插值，
    /// 更近取近端满值，更远取远端下限。
    /// </summary>
    public class HeavyShooterChipGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            // 直接命中没有弹幕位置，按「贴身」处理（满档）。
            Apply(player, 0f, ref modifiers);
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 无主弹幕的 owner 是 255，取 Main.player 之前必须先挡掉非玩家来源。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player player = Main.player[projectile.owner];
            Apply(player, Vector2.Distance(projectile.Center, player.Center), ref modifiers);
        }

        /// <summary>按距离插值，往本次命中写入破甲与击退。</summary>
        private static void Apply(Player player, float distance, ref NPC.HitModifiers modifiers)
        {
            if (!player.active || player.dead || player.ghost)
                return;

            HeavyShooterChipPlayer modPlayer = player.GetModPlayer<HeavyShooterChipPlayer>();
            if (modPlayer.Level == 0)
                return;

            bool equipped = modPlayer.IsEquipped;

            // 0 = 近端（≤200 像素），1 = 远端（≥400 像素）
            float t = MathHelper.Clamp(
                (distance - HeavyShooterChip.NearDistance) / (HeavyShooterChip.FarDistance - HeavyShooterChip.NearDistance),
                0f, 1f);

            float penetration = MathHelper.Lerp(
                equipped ? HeavyShooterChip.EquippedNearPen : HeavyShooterChip.FavoritedNearPen,
                equipped ? HeavyShooterChip.EquippedFarPen : HeavyShooterChip.FavoritedFarPen,
                t);

            float knockback = MathHelper.Lerp(
                equipped ? HeavyShooterChip.EquippedNearKnockback : HeavyShooterChip.FavoritedNearKnockback,
                equipped ? HeavyShooterChip.EquippedFarKnockback : HeavyShooterChip.FavoritedFarKnockback,
                t);

            // 纯比例破甲：按削防前的防御值取百分比（NPC.HitModifiers.GetDamage）。
            modifiers.ScalingArmorPenetration += penetration;

            modifiers.Knockback *= knockback;
        }
    }
}
