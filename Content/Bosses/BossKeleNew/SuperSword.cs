using System;
using ExpansionKele.Content.Customs;
using InnoVault;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class SuperSword : ModProjectile
    {
        public override string Texture => this.GetRelativeTexturePath("./SuperSword");
        
        private static Asset<Texture2D> _cachedTexture;
        
        public float counter = 0;
        public float scale = 1f;
        public float alpha = 1f;
        public bool init = true;
        public int npcitemtime=20;
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
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.hide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
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
            
            float MaxUpdateTimes = npcitemtime * Projectile.MaxUpdates;
            //获取挥舞进度
            float progress = (counter / MaxUpdateTimes);
            counter++;
            
            if (init)
            {
                SoundEngine.PlaySound(SoundID.Item1, ownerNPC.Center);
                init = false;
            }
            Projectile.timeLeft = 3;
            alpha = 1;
            scale = 1f;
            float swingAngle;
            float cosValue = -(float)Math.Cos(Math.Pow(progress,0.5) * Math.PI);
            swingAngle = MathHelper.PiOver2 * cosValue;

            //，在不设置射弹速度时，Projectile.velocity此时为一个x+0/-0,y-0的奇怪系统，不能指示方向
            float baseRotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2*(2*progress-1);

            Projectile.rotation = Projectile.velocity.ToRotation() +MathHelper.PiOver4+ swingAngle*1;

            if (counter > MaxUpdateTimes)
            {
                Projectile.Kill();
            }
            if(meleePhase == Phase.phase2||meleePhase == Phase.phase3){
                if(counter==MaxUpdateTimes/2){
                Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                ownerNPC.Center+baseRotation.ToRotationVector2() * 160f, 
                baseRotation.ToRotationVector2()*12f, 
                ModContent.ProjectileType<ColaBossProjectile>(), 
                Projectile.damage,
                0,
                Main.myPlayer,
                ownerNPC.whoAmI,
                (int)meleePhase);
                }
            }
            if (meleePhase == Phase.phase3||meleePhase == BossKeleNew.Phase.phase4){
                if(counter==MaxUpdateTimes/2){
                for(int i = 0; i < (meleePhase == Phase.phase3?6:12); i++){
                    Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    ownerNPC.Center+new Vector2(0,-100), 
                    MathHelper.ToRadians(i*(meleePhase == Phase.phase3?60:30)).ToRotationVector2()*10f, 
                    ModContent.ProjectileType<ColaBossProjectile>(), 
                    Projectile.damage,
                    0,
                    Main.myPlayer,
                    ownerNPC.whoAmI,
                    (int)meleePhase);
                }
                }
            }

            if (meleePhase == Phase.phase4)
            {
                if(counter==MaxUpdateTimes/4||counter==MaxUpdateTimes/2||counter==MaxUpdateTimes*3/4){
                Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                ownerNPC.Center+baseRotation.ToRotationVector2() * 160f, 
                baseRotation.ToRotationVector2()*10f, 
                ModContent.ProjectileType<ColaBossProjectile>(), 
                Projectile.damage,
                0,
                Main.myPlayer,
                ownerNPC.whoAmI,
                (int)meleePhase);
                }
            }
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null || _cachedTexture.IsLoaded == false)
                return false;
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
                return false;
                
            Texture2D tex = _cachedTexture.Value;
            float rot =Projectile.rotation ;
            
            Vector2 origin = new Vector2(0, tex.Height);
            
            Main.EntitySpriteDraw(
                tex, 
                ownerNPC.Center - Main.screenPosition, 
                null, 
                lightColor * alpha, 
                rot, 
                origin, 
                Projectile.scale * scale, 
                SpriteEffects.None
            );

            return false;
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (_cachedTexture == null || _cachedTexture.IsLoaded == false)
                return base.Colliding(projHitbox, targetHitbox);
                
            Texture2D tex = _cachedTexture.Value;
            float weaponLength = tex.Width * Projectile.scale * scale*MathF.Sqrt(2);
            Vector2 bladeTip = Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength;
            
            float collisionPoint = 0;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, bladeTip, 6, ref collisionPoint))
            {
                return true;
            }
            
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void CutTiles()
        {
            if (_cachedTexture == null || _cachedTexture.IsLoaded == false)
                return;
                
            Texture2D tex = _cachedTexture.Value;
            float weaponLength = tex.Width * Projectile.scale * scale*MathF.Sqrt(2);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * weaponLength, 54, DelegateMethods.CutTiles);
        }

        
    }
}