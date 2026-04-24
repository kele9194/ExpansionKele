using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public abstract class InvisibleNPCProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.BossKeleNew";
        
        protected abstract int MaxLifetime { get; }
        
        public sealed override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.extraUpdates = 0;
            Projectile.DamageType = DamageClass.Default;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            
            SetInvisibleDefaults();
        }
        
        protected virtual void SetInvisibleDefaults()
        {
        }
        
        public sealed override bool? CanDamage()
        {
            return false;
        }
        public sealed override bool ShouldUpdatePosition()
        {
            return false;
        }
        
    
        public sealed override Color? GetAlpha(Color lightColor)
        {
            return Color.Transparent;
        }
        
        public sealed override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}