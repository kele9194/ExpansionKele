using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.ArknightsItem.Accessories.Guard;
using ExpansionKele.Content.Projectiles.MeleeProj;

namespace ExpansionKele.Global
{
    /// <summary>
    /// 领主芯片的剑气发射器。
    /// <para/>
    /// <see cref="UseAnimation"/> 由 <c>Player.ApplyItemAnimation</c> 调用（<c>Player.cs:42436</c>），
    /// 只在实际开始挥动的那一帧执行一次，所以不会因为挥动动画持续多帧而重复发射。
    /// <para/>
    /// 剑气朝鼠标方向直飞，不做自动索敌：命中率交给玩家的瞄准，打空时也有明确的因果关系。
    /// </summary>
    public class LordChipGlobalItem : GlobalItem
    {
        public override void UseAnimation(Item item, Player player)
        {
            // 只让每个客户端处理自己的那次挥动，避免多人环境重复生成弹幕。
            if (Main.dedServ || player.whoAmI != Main.myPlayer)
                return;

            if (item.IsAir || !item.CountsAsClass(DamageClass.Melee))
                return;

            // 镐 / 斧 / 锤也是 DamageClass.Melee，不排除的话挖矿时会一直甩剑气。
            if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
                return;

            LordChipPlayer modPlayer = player.GetModPlayer<LordChipPlayer>();
            float ratio = modPlayer.SlashRatio;
            if (ratio <= 0f)
                return;

            int damage = (int)(player.GetWeaponDamage(item) * ratio);
            if (damage <= 0)
                return;

            // 方向完全交给玩家：朝鼠标方向直飞
            Vector2 aim = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

            Projectile.NewProjectile(
                player.GetSource_ItemUse(item),
                player.Center,
                aim * LordChip.SlashSpeed,
                ModContent.ProjectileType<LordSlashProjectile>(),
                damage,
                item.knockBack * 0.5f,
                player.whoAmI);
        }
    }
}
