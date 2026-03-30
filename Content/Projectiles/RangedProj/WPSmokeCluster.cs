using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class WPSmokeCluster : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.RangedProj";
        public const int MAX_SHOTS = 6;
        public const int FRAMES_BETWEEN_SHOTS = 4;
        private int frameCounter = 0;
        private int shotsFired = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.hide = true;
        }

        public override void AI()
        {
            frameCounter++;
            
            if (frameCounter >= FRAMES_BETWEEN_SHOTS && shotsFired < MAX_SHOTS)
            {
                frameCounter = 0;
                FireSmoke(Projectile.Center);
                shotsFired++;
            }
            
            if (shotsFired >= MAX_SHOTS)
            {
                Projectile.Kill();
            }
        }

        // ... existing code ...

        private void FireSmoke(Vector2 position)
        {
            Player owner = Main.player[Projectile.owner];
            
            NPC lockedTarget = GetLockedTarget();
            
            Vector2 targetPos = position;
            if (lockedTarget != null && lockedTarget.active)
            {
                targetPos = lockedTarget.Center;
            }
            
            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            float u1 = Main.rand.NextFloat(1f);
            float u2 = Main.rand.NextFloat(1f);
            
            float maxradius =  Math.Min(Vector2.Distance(owner.Center, targetPos) / 8f, 200f);
            float radius = MathF.Sqrt(-2f * MathF.Log(u1)) * MathF.Cos(MathHelper.TwoPi * u2) * maxradius;
            radius = MathF.Max(0f, radius);
            
            
            Vector2 offset = new Vector2(
                MathF.Cos(randomAngle) * radius,
                MathF.Sin(randomAngle) * radius
            );
            
            Vector2 spawnPos = targetPos + offset;
            
            int projId = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                Vector2.Zero,
                ModContent.ProjectileType<WPSmokeProjectile>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
            
        }

        private NPC GetLockedTarget()
        {
            int targetId = (int)Projectile.ai[0] - 1;
            if (targetId >= 0 && targetId < Main.maxNPCs && Main.npc[targetId].active)
            {
                return Main.npc[targetId];
            }
            return null;
        }
    }
}