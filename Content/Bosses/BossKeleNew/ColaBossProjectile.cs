using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using System;
using ReLogic.Content;
using InnoVault;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class ColaBossProjectile : ModProjectile
    {
        public static readonly Color ColaColor = new Color(214, 123, 44);
        
        private static Asset<Texture2D> _cachedTexture;

        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase meleePhase;

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
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 200;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            meleePhase=(Phase)(int)Math.Round(Projectile.ai[1]);
            
            NPC ownerNPC = npcIndex.GetNPCOwner();
            
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }

            Lighting.AddLight(Projectile.Center, 1f, 0.8f, 0.4f);

            int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 0, ColaColor, 1f);
            Dust dust = Main.dust[dustIndex];
            dust.noGravity = true;
            dust.scale = 1.5f;
            dust.fadeIn = 1f;
            dust.noLight = false;
            dust.color = ColaColor;
            Lighting.AddLight(dust.position, 1f, 0.8f, 0.4f);
            
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.7f;
            }

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null || !_cachedTexture.IsLoaded)
                return false;

            Texture2D texture = _cachedTexture.Value;
            
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }



    }
}