using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using InnoVault;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;
using Terraria.Audio;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class BossSniperRifle : ModProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./BossSniperRifle");
        
        private static Asset<Texture2D> _cachedTexture;
        private static Asset<Texture2D> _cachedTexture2;
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase rangedPhase;
        
        
        
        
        public override void Load()
        {
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
            _cachedTexture2 = ModContent.Request<Texture2D>("ExpansionKele/Content/Bosses/BossKeleNew/BossSniperLaser");

        }
        
        public override void Unload()
        {
            _cachedTexture = null;
            _cachedTexture2 = null;
        }
        
        public override void SetStaticDefaults()
        {
            
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            
        }
        
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        
        public override bool? CanDamage()
        {
            return false;
        }
        
        
        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            rangedPhase=(Phase)(int)Math.Round(Projectile.ai[1]);
            
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }if (!ownerNPC.HasValidTarget)
            {
                return;
            }
            Player targetPlayer = Main.player[ownerNPC.target];
            Vector2 AimingVector = targetPlayer.Center - ownerNPC.Center;
            
            float bulletSpeed = GetBulletSpeedForPhase(rangedPhase);
            Vector2 predictedAimVector = CalculatePredictedAimVector(ownerNPC.Center, targetPlayer, bulletSpeed);
            
            Projectile.Center = ownerNPC.Center;

            if(Projectile.timeLeft>=60){
                switch (rangedPhase)
                {
                    case Phase.phase1:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,AimingVector.ToRotation(),0.15f);
                        break;
                    case Phase.phase2:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,AimingVector.ToRotation(),0.3f);
                        break;
                    case Phase.phase3:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,predictedAimVector.ToRotation(),0.5f);
                        break;
                    case Phase.phase4:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,predictedAimVector.ToRotation(),1f);
                        break;
                }
            }
            else if(30<=Projectile.timeLeft&&Projectile.timeLeft<60){
                switch (rangedPhase)
                {
                    case Phase.phase1:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,AimingVector.ToRotation(),0.1f);
                        break;
                    case Phase.phase2:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,AimingVector.ToRotation(),0.2f);
                        break;
                    case Phase.phase3:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,predictedAimVector.ToRotation(),0.25f);
                        break;
                    case Phase.phase4:
                        Projectile.rotation =MathHelper.Lerp(Projectile.rotation,predictedAimVector.ToRotation(),0.5f);
                        break;
                }
            }
                


            if(Projectile.timeLeft==30){
                    SoundEngine.PlaySound(ExpansionKele.SniperSound, ownerNPC.Center);
                    Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    ownerNPC.Center+Projectile.rotation.ToRotationVector2() * 90f, 
                    Projectile.rotation.ToRotationVector2()*bulletSpeed, 
                    ModContent.ProjectileType<BossHighVelocityBullet>(), 
                    Projectile.damage, 
                    0, 
                    Main.myPlayer, 
                    ownerNPC.whoAmI, 
                    (int)rangedPhase
                );
                
            }
        }

        private float GetBulletSpeedForPhase(Phase phase)
        {
            switch (phase)
            {
                case Phase.phase1:
                    return 15f;
                case Phase.phase2:
                    return 20f;
                case Phase.phase3:
                    return 25f;
                case Phase.phase4:
                    return 30f;
                default:
                    return 15f;
            }
        }

        public Vector2 CalculatePredictedAimVector(Vector2 shooterPosition, Player target, float projectileSpeed)
        {
            Vector2 toTarget = target.Center - shooterPosition;
            Vector2 targetVelocity = target.velocity;
            
            float distance = toTarget.Length();
            
            if (distance < 1f)
            {
                return toTarget;
            }
            
            float timeToImpact = distance / projectileSpeed;
            
            Vector2 predictedPosition = target.Center + targetVelocity * timeToImpact;
            
            Vector2 predictedDirection = predictedPosition - shooterPosition;
            
            if (predictedDirection.LengthSquared() < 0.001f)
            {
                return toTarget;
            }
            
            return predictedDirection;
        }
        
        
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null || !_cachedTexture.IsLoaded)
                return false;
            
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return false;
            }
            
            Texture2D tex = _cachedTexture.Value;
            Texture2D tex2 = _cachedTexture2.Value;
            
            int direction = Projectile.rotation.ToRotationVector2().X>0 ? 1 : -1;
            
            float rot = direction > 0?Projectile.rotation:Projectile.rotation+MathHelper.Pi;
            Vector2 origin = direction > 0 ?new Vector2(tex.Width / 4f, tex.Height / 2f):new Vector2(3*tex.Width / 4f, tex.Height / 2f);
            SpriteEffects effect = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 origin2 = direction > 0 ?new Vector2(0, tex2.Height / 2f):new Vector2(tex2.Width, tex2.Height / 2f);
            
        
            
            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                rot,
                origin,
                Projectile.scale,
                effect,
                0
            );
            float alpha = 255f; // 可以根据需要调整这个值
            if(rangedPhase==Phase.phase1){
                alpha=191f;
            }
            if(rangedPhase==Phase.phase2){
                alpha=127f;
            }
            if(rangedPhase==Phase.phase4){
                alpha=63f;
            }

            Color drawColor = lightColor * (alpha / 255f);
            Main.spriteBatch.Draw(
                tex2,
                Projectile.Center - Main.screenPosition+Projectile.rotation.ToRotationVector2() * 60f,
                null,
                lightColor,
                rot,
                origin2,
                Projectile.scale,
                effect,
                0
            );
            
            return false;
        }
    }
}