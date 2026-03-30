using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    // ... existing code ...
    public class WPSmokeProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.RangedProj";
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TOTAL_FRAMES = 4;
        private const int FRAMES_PER_ANIMATION = 8;
        private const float ALPHA_INCREASE_PER_FRAME = 8f;
        private float alpha = 0f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = TOTAL_FRAMES;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }

        // ... existing code ...
        public override void AI()
        {
            alpha += ALPHA_INCREASE_PER_FRAME;
            
            if (alpha > 255)
            {
                alpha = 255;
            }
            
            Projectile.alpha = (int)alpha;
            
            frameCounter++;
            
            if (frameCounter >= FRAMES_PER_ANIMATION)
            {
                frameCounter = 0;
                currentFrame++;
                
                if (currentFrame >= TOTAL_FRAMES)
                {
                    currentFrame = TOTAL_FRAMES - 1;
                }
            }
            
            Projectile.frame = currentFrame;
            
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
        }

        

         public override Color? GetAlpha(Color lightColor)
        {
            return new Color(Projectile.alpha, Projectile.alpha, Projectile.alpha, 200);
        }

        public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
    
    Vector2 drawPosition = Projectile.Center - Main.screenPosition;
    
    Rectangle frameRect = texture.Frame(1, TOTAL_FRAMES, 0, Projectile.frame);
    Color frameColor = new Color(255, 255, 255, 255 - Projectile.alpha);
    
    Main.EntitySpriteDraw(
        texture,
        drawPosition,
        frameRect,
        frameColor,
        Projectile.rotation,
        frameRect.Size(),
        Projectile.scale,
        SpriteEffects.None,
        0
    );
    
    if (Main.rand.NextFloat() < 0.25f)
    {
        int nextFrame = Math.Min(Projectile.frame + 1, TOTAL_FRAMES - 1);
        if (nextFrame != Projectile.frame)
        {
            Rectangle nextFrameRect = texture.Frame(1, TOTAL_FRAMES, 0, nextFrame);
            float blendAlpha = frameCounter / (float)FRAMES_PER_ANIMATION;
            Color nextFrameColor = new Color(255, 255, 255, (int)((255 - Projectile.alpha) * blendAlpha));
            
            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                nextFrameRect,
                nextFrameColor,
                Projectile.rotation,
                nextFrameRect.Size(),
                Projectile.scale,
                SpriteEffects.None,
                0
            );
        }
    }
    
    return false;
}
    }
}