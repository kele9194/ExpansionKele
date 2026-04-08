using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using System.Linq;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class MarkedSubArrow : ModProjectile
    {
        private static Asset<Texture2D> _cachedTexture;
        private int markedTargetIndex = -1;
        private TrackingType trackingType = TrackingType.Homing;

        public enum TrackingType
        {
            Homing =0,
            Line =1
        }

        public override string Texture => this.GetRelativeTexturePath("./MarkedArrow");

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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.extraUpdates = 1;
        }
        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            if (Projectile.ai[1] != (float)TrackingType.Homing)
            {
                trackingType = (TrackingType)(int)Projectile.ai[1];
            }
            if (Projectile.ai[0] == (float)TrackingType.Homing){
                int targetIndex = (int)Projectile.ai[0];
            if (IsValidMarkedTarget(targetIndex))
            {
                markedTargetIndex = targetIndex;
            }
            else
            {
                markedTargetIndex = -1;
            }
            }
        }
    

        public override void AI()
        {
            if (trackingType == TrackingType.Line)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                        DustID.Shadowflame, 0f, 0f, 100, Color.DeepPink, 0.8f);
                    dust.noGravity = true;
                    dust.velocity *= 0.2f;
                }

                Lighting.AddLight(Projectile.Center, 0.2f, 0.7f, 0.8f);
                return;
            }

            NPC markedTarget = GetMarkedTarget();
            
            if (markedTarget != null)
            {
                Vector2 direction = markedTarget.Center - Projectile.Center;
                float distance = direction.Length();
                
                if (distance > 0)
                {
                    direction.Normalize();
                    float speed = 12f;
                    
                    if (distance < 200f)
                    {
                        speed = 16f;
                    }
                    
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.15f);
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                if (Projectile.Hitbox.Intersects(markedTarget.Hitbox))
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                Projectile.penetrate = 1;
                ProjectileHelper.FindAndMoveTowardsTarget(
                    Projectile, 
                    speed: 12f, 
                    maxTrackingDistance: 800f, 
                    turnResistance: 15f, 
                    bossPriority: true
                );
                
                if (Projectile.timeLeft < 60)
                {
                    Projectile.Kill();
                }
            }

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, Color.DeepPink, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.7f, 0.8f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.6f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(Projectile.owner == Main.myPlayer){
                if(Projectile.ai[0]==(float)TrackingType.Homing){
                target.AddBuff(BuffID.ShadowFlame, 600);
                }
                else{
                    target.AddBuff(BuffID.ShadowFlame,120);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null)
                return true;

            Texture2D tex = _cachedTexture.Value;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

            Main.EntitySpriteDraw(tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * 0.8f,
                Projectile.rotation,
                origin,
                Projectile.scale * 0.8f,
                SpriteEffects.None,
                0);

            return false;
        }

        private bool IsValidMarkedTarget(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
                return false;
                
            NPC target = Main.npc[targetIndex];
            
            return target.active && target.life > 0 && !target.friendly && !target.immortal && !target.dontTakeDamage;
        }
        
        private NPC GetMarkedTarget()
        {
            if (markedTargetIndex < 0 || markedTargetIndex >= Main.maxNPCs)
                return null;
                
            NPC target = Main.npc[markedTargetIndex];
            
            if (!IsValidMarkedTarget(markedTargetIndex))
            {
                markedTargetIndex = -1;
                return null;
            }
            
            return target;
        }
    }
}