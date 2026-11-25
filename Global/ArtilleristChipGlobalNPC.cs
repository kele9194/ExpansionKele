using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Sniper;
using ExpansionKele.Content.Projectiles;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 炮手芯片的溅射触发点。
    /// <para/>
    /// 触发放在 <c>OnHitByProjectile</c>（命中结算之后）：此时弹幕位置就是命中点，
    /// 且能通过 <see cref="ArtilleristChipGlobalProjectile.aoeTriggered"/> 保证每发弹幕只炸一次。
    /// <para/>
    /// 真正的伤害交给 <see cref="ArtilleristAoeProj"/> 这个溅射弹幕结算 ——
    /// 它走 <c>ModifyHitByProjectile</c> 管线，其他芯片的破甲 / 暴击 / 大体型增伤都会作用上去。
    /// </summary>
    public class ArtilleristChipGlobalNPC : GlobalNPC
    {
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            // 溅射弹幕自己也会命中敌人，必须排除，否则它会给自己再造一圈无限递归。
            if (projectile.type == ModContent.ProjectileType<ArtilleristAoeProj>())
                return;

            // 只认远程伤害的弹幕：召唤物、魔法、近战弹幕都不触发。
            // 用 CountsAsClass 而不是 ==，否则继承自 Ranged 的自定义伤害类型会被漏判。
            if (!projectile.DamageType.CountsAsClass(DamageClass.Ranged))
                return;

            // 无主弹幕的 owner 是 255。
            if (!projectile.friendly || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            // 只在服务端结算，避免客户端重复造成伤害。
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            ArtilleristChipGlobalProjectile projGlobal = projectile.GetGlobalProjectile<ArtilleristChipGlobalProjectile>();
            if (projGlobal.aoeTriggered)
                return;

            Player player = Main.player[projectile.owner];
            if (!player.active || player.dead || player.ghost)
                return;

            ArtilleristChipPlayer modPlayer = player.GetModPlayer<ArtilleristChipPlayer>();
            if (modPlayer.Level == 0)
                return;

            // 先打标记，保证这一发之后不再触发（哪怕下面因为伤害为 0 提前返回）。
            projGlobal.aoeTriggered = true;

            float ratio = modPlayer.IsEquipped
                ? ArtilleristChip.EquippedBlastRatio
                : ArtilleristChip.FavoritedBlastRatio;

            int blastDamage = (int)(projectile.damage * ratio);
            if (blastDamage <= 0)
                return;

            // 主目标也吃（不做 localNPCImmunity 屏蔽），符合「范围伤害也包括中心目标」的预期。
            // 伤害交给溅射弹幕：它走 ModifyHitByProjectile 管线，其他芯片的破甲/暴击/大体型增伤都会生效。
            Projectile.NewProjectile(
                new EntitySource_Parent(projectile),
                projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ArtilleristAoeProj>(),
                blastDamage,
                0f,
                projectile.owner);
        }
    }
}
