using ExpansionKele.Content.Buff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class ChromiumBulletProjectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SlicingBuff>(), 180);

            Projectile.damage = (int)(Projectile.damage * 0.5f);
        }
    }
}