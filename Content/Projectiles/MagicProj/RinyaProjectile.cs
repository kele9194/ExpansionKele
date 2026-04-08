using System;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles.MagicProj;
using Microsoft.Build.Evaluation;
using InnoVault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Items.Weapons.Magic;

namespace ExpansionKele.Content.Projectiles.MagicProj
{ 

public class RinyaProjectile : ModProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./RinyaProjectileA");
        
        public static Asset<Texture2D> _cachedTexture;
        
        [SyncVar]
        public float currentCharge = 0f;
        
        public const float MAX_CHARGE = 1f;
        
        public override void Load()
        {
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }
        
        public override void Unload()
        {
            _cachedTexture = null;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
            Projectile.extraUpdates = 1;
            
        }
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        [SyncVar]
        public Vector2 MousePosition;

        [SyncVar]
        public bool isReleased=false;

        [SyncVar]
        public Vector2 targetPosition;

        [SyncVar]
        public float randomFloat;

        [SyncVar]
        public bool isshoot;

        [SyncVar]
        public float randomAngle ;
        [SyncVar]
        public float UseSpeedMul;
        
        
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            UseSpeedMul=UseTimeHelper.GetTotalUseMultiplier(player,player.HeldItem, true);
            
            if (!isReleased)
            {
                Texture2D tex = _cachedTexture.Value;
                Projectile.timeLeft = 180;
                MousePosition = Main.MouseWorld;
                Vector2 MouseVector = MousePosition - player.MountedCenter;

                if (Projectile.owner == Main.myPlayer)
                {
                    randomFloat = Main.rand.NextFloat();
                }

                targetPosition = player.MountedCenter + Vector2.Normalize(MouseVector) * (float)(240+randomFloat*20);
                Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition, 0.15f);

                Projectile.alpha =(int)(255*MathHelper.Lerp(0.5f, 0.8f, currentCharge));
                Projectile.scale = MathHelper.Lerp(0.2f, 0.7f, currentCharge);
                
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)(Math.PI * 0.5f));
                
                if (player.channel)
                {
                    currentCharge += 0.01f*UseSpeedMul/Projectile.MaxUpdates;
                    if (currentCharge >= MAX_CHARGE)
                    {
                        currentCharge = MAX_CHARGE;
                    }
                    Projectile.rotation = MouseVector.ToRotation();
                    player.heldProj = Projectile.whoAmI;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                }
                else
                {
                    

                    isReleased = true;
                    Vector2 initialVelocity = Vector2.Normalize(MouseVector) * 10f;
                    Projectile.velocity = initialVelocity;
                    if(currentCharge<=0.2f){
                        Projectile.Kill();
                    }
                }
            }
            else
            {

                ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 10f, 640f, 10f);
            }

            if (Projectile.owner == Main.myPlayer)
                {
                    isshoot = Main.rand.NextFloat()<=1/12f*UseSpeedMul/Projectile.MaxUpdates;
                    if(isshoot){
                    float spawnRadius = 1.1f * Projectile.scale * (_cachedTexture?.Value?.Width ?? 100) / 2f;

                    randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 spawnOffset = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * spawnRadius;
                    Vector2 spawnPosition = Projectile.Center + spawnOffset;
                    
                    Vector2 subVelocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * 15f;
                    
                    
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPosition,
                        subVelocity,
                        ModContent.ProjectileType<RinyaSubProjectile>(),
                        (int)(Projectile.damage*1f),
                        Projectile.knockBack * 0.5f,
                        Projectile.owner,
                        currentCharge,
                        isReleased ? 1f : 0f
                        
                    );
                    }
        }
        }

        public override bool ShouldUpdatePosition()
        {
            if (!isReleased)
            {
                return false;
            }
            return true;

        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

                float damageMultiplier = 0.3f + (currentCharge * 1.5f);
                modifiers.FinalDamage *= damageMultiplier;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null)
                return true;
            
            
            Texture2D tex = _cachedTexture.Value;
            Player owner = Main.player[Projectile.owner];
            Vector2 MouseVector = MousePosition-Projectile.Center;
            int direction=MouseVector.X > 0 ? 1 : -1;
            Vector2 normalizedMouseVector = Vector2.Normalize(MouseVector);
            

            
            float alpha = MathHelper.Lerp(0.5f, 0.8f, currentCharge);;

            
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            float spinRatio=0.05f;

            float spinRotation =(float)(Main.timeForVisualEffects * spinRatio);
            
            
            Main.EntitySpriteDraw(
                tex,//贴图
                Projectile.Center +owner.gfxOffY * Vector2.UnitY - Main.screenPosition,//相对位置，继承projectile.center
                null,//默认全图
                Color.White*alpha,//光影效果
                spinRotation,//自旋速度，一般于抛射体苏苏有关
                origin,//自旋点左上为0，0
                Projectile.scale,//贴图大小
                SpriteEffects.None,
                1
            );
            
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Texture2D tex = _cachedTexture.Value;
            float x = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
            float y = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
            //选定最接近抛射体中心的点，并进行平方计算
            float distancex = Projectile.Center.X - x;
            float distancey = Projectile.Center.Y - y;
            return distancex * distancex + distancey * distancey <= Math.Pow(Projectile.scale,2)*tex.Width/2f*tex.Height/2f;
        }
    }
}
