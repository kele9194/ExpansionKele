using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework.Input;
using Terraria.DataStructures;
using static ExpansionKele.Content.Projectiles.MagicProj.MarkedSubArrow;
using InnoVault;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class MarkedArrow : ModProjectile
    {
        private static Asset<Texture2D> _cachedTexture;

        public override string Texture => this.GetRelativeTexturePath("./MarkedArrow");
        [SyncVar]
        public bool isshoot=false;

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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 4;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft = (int)(Projectile.ai[0]*100);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, Color.DeepPink, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.8f, 0.9f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.5f+(Projectile.ai[0]*1.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 600);
             if(Projectile.owner == Main.myPlayer){
            if(!isshoot){
            Player owner = Main.player[Projectile.owner];
            
            Vector2 MousePosition = Main.MouseWorld;
            float angle =MathHelper.ToRadians(15);
            //Projectile.Center = owner.MountedCenter;
            Vector2 MouseVector = MousePosition - owner.MountedCenter;
            Projectile.rotation = MouseVector.ToRotation();
            for(int i = 0; i < 5; i++){
                Vector2 normalizedVector=(MouseVector.ToRotation()+((-2+i)*angle)).ToRotationVector2();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                owner.MountedCenter+normalizedVector*50f, 
                normalizedVector*12f, 
                ModContent.ProjectileType<MarkedSubArrow>(), 
                damageDone, 
                0f, 
                owner.whoAmI, 
                target.whoAmI,
                (int)TrackingType.Homing
                );
            }
            isshoot=true;
            }
             }
            
           
        }
        public override void OnKill(int timeLeft)
        {
            if(Projectile.owner == Main.myPlayer){
            if(!isshoot){
            Player owner = Main.player[Projectile.owner];
            
            Vector2 MousePosition = Main.MouseWorld;
            float angle =MathHelper.ToRadians(15);
            //Projectile.Center = owner.MountedCenter;
            Vector2 MouseVector = MousePosition - owner.MountedCenter;
            Projectile.rotation = MouseVector.ToRotation();
            for(int i = 0; i < 5; i++){
                Vector2 normalizedVector=(MouseVector.ToRotation()+((-2+i)*angle)).ToRotationVector2();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                owner.MountedCenter+normalizedVector*50f, 
                normalizedVector*12f, 
                ModContent.ProjectileType<MarkedSubArrow>(), 
                Projectile.damage, 
                0f, 
                owner.whoAmI, 
                -1,
                (int)TrackingType.Line
                );
            }
            isshoot=true;
            }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
}