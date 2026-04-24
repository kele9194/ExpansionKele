using ExpansionKele.Content.Items.Weapons.Ranged;
using InnoVault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class FFLaser : ModProjectile
    {
        private const float PARALLEL_OFFSET = 10f;
        private const int MAX_LASER_LENGTH = 900;
        
        private static Asset<Texture2D> _cachedTexture;
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public int laserLength;
        [SyncVar]
        public bool init=true;

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
            
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.alpha = 0;
            Projectile.light =1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = 2;
            if(init){
                npcIndex=(int)Projectile.ai[0];
                NPC npc = npcIndex.GetNPCOwner();
                Vector2 explosionPos;
                    if(npc!=null){
                        laserLength=(int)(npc.Center-Projectile.Center).Length();
                        explosionPos = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * laserLength;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                        explosionPos, 
                        Vector2.Zero, 
                        ModContent.ProjectileType<FFExplosion>(), 
                        Projectile.damage, 
                        0f, 
                        Projectile.owner);
                        SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                    }
                    else{
                        laserLength=MAX_LASER_LENGTH;
                        
                    }
                
                init=false;
            }
            Projectile.alpha +=10;
            if(Projectile.alpha>=250){
                Projectile.Kill();
                
            }   
            
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }



        public override bool PreDraw(ref Color lightColor)
        {
            
            Texture2D tex = _cachedTexture.Value;
            
            Rectangle sourceRect = new Rectangle(0, 0, laserLength, tex.Height);
            
            if (sourceRect.Width > tex.Width)
            {
                sourceRect.Width = tex.Width;
            }
            
            Vector2 origin = new Vector2(0, tex.Height / 2f);
            
            Main.EntitySpriteDraw(tex, 
                Projectile.Center - Main.screenPosition, 
                sourceRect, 
                lightColor * Projectile.Opacity, 
                Projectile.rotation, 
                origin, 
                Projectile.scale, 
                SpriteEffects.None, 
                0);
            return false;
        }


        public override bool? CanDamage()
        {
            return false;
        }
    }

    public class FFExplosion : ModProjectile
    {
        private static Asset<Texture2D> _cachedTexture;
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
            Projectile.light =1;
        }

        public override void AI()
        {
            Projectile.timeLeft = 2;
            Projectile.alpha +=5;
            if(Projectile.alpha>=250){
                Projectile.Kill();
                
            }
        }

        public override bool? CanDamage()
        {
            return Projectile.alpha<=125;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration += 9999;
            modifiers.DefenseEffectiveness *= 0f;
            base.ModifyHitNPC(target, ref modifiers);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = _cachedTexture.Value;
            
            Vector2 origin = new Vector2(tex.Width / 2, tex.Height / 2);
            
            Main.EntitySpriteDraw(tex, 
                Projectile.Center - Main.screenPosition, 
                null, 
                lightColor * Projectile.Opacity, 
                Projectile.rotation, 
                origin, 
                Projectile.scale, 
                SpriteEffects.None, 
                0
            );
            return false;
        }
    }
}
