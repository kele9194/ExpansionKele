using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MeleeProj;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

public class LordGlobalItem : GlobalItem
{
    public override void UseAnimation(Item item, Player player)
    {
        if (item.IsAir) return;
        
        GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(item);
        if (subclass != GuardSubclass.Lord) return;
        if (!item.DamageType.CountsAsClass(DamageClass.Melee)) return;

        // 发射领主剑气
        Vector2 velocity = Main.MouseWorld - player.Center;
        velocity = velocity.SafeNormalize(Vector2.UnitX) * 8f;

        int slashDamage = (int)(player.GetWeaponDamage(item) * 0.6f);
        Projectile.NewProjectile(
            player.GetSource_ItemUse(item),
            player.Center, velocity,
            ModContent.ProjectileType<LordSlashProjectile>(),
            slashDamage, item.knockBack * 0.5f, player.whoAmI
        );
    }
}