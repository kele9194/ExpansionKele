using System;
using ExpansionKele.Content.Customs;
using InnoVault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    
    public class RinyaSubBossProjectile : ModProjectile
    {
        
        public Color purpleColor = new Color(0xbf, 0x9c, 0xf4);

        [SyncVar]
        public int npcIndex;
        
        [SyncVar]
        public Phase summonPhase;
        

        
        
        [SyncVar]
        public float counter = 0f;

        public static Asset<Texture2D> _cachedTexture;
        
        
        public override void Load()
        {
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }
        
        public override void Unload()
        {
            _cachedTexture = null;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 9;
            Projectile.height = 9;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.scale=0.5f;
        }

        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            summonPhase = (Phase)(int)Math.Round(Projectile.ai[1]);
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }
            
            if (!ownerNPC.HasValidTarget)
            {
                return;
            }
            Player targetPlayer = Main.player[ownerNPC.target];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            
            
            

            if (Main.rand.NextBool(5))
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

        

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null || !_cachedTexture.IsLoaded){
                return true;
            }
            
            Texture2D tex = _cachedTexture.Value;
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                GetAlpha(lightColor) ?? lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            
            return false;
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