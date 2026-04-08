using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class RinyaSubProjectile : ModProjectile
    {
        public Color purpleColor = new Color(0xbf,0x9c,0xf4);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 10f, 640f, 10f);

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.FireworksRGB, 0f, 0f, 100, purpleColor, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
                
                Dust glowDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, purpleColor, 0.8f);
                glowDust.noGravity = true;
                glowDust.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.8f, 1f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float currentCharge =Projectile.ai[0];
                float damageMultiplier = 0.3f + (currentCharge * 1.5f);
                modifiers.FinalDamage *= damageMultiplier;
                //Main.NewText(Projectile.ai[0]);
                if(Projectile.ai[1]==1f){
                    modifiers.FinalDamage *= 2;
                }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PurpleTorch, 0f, 0f, 100, purpleColor, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return purpleColor * Projectile.Opacity;
        }
    }
}