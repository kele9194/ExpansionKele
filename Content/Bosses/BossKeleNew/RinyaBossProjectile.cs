using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using InnoVault;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class RinyaBossProjectile : ModProjectile
    {
        [SyncVar]
        public int npcIndex;
        
        [SyncVar]
        public Phase summonPhase;
        

        
        
        [SyncVar]
        public float counter = 0f;

        [SyncVar]
        public float scale=0f;
        
        
        public static Asset<Texture2D> _cachedTexture;
        
        
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
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;
            Projectile.extraUpdates = 0;
        }
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        public override bool? CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            npcIndex=(int)Math.Round(Projectile.ai[0]);
            summonPhase = (Phase)(int)Math.Round(Projectile.ai[1]);
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }

            if(!ownerNPC.HasValidTarget){
                return;
            }
            
            Player targetPlayer = Main.player[ownerNPC.target];
            Projectile.Center = ownerNPC.Center;
            Projectile.rotation = counter*MathHelper.Pi/60f;
            counter++;
            if(counter<=25){
                scale+=1/25f;
            }
            if(counter>=275){
                scale-=1/25f;
            }
            if(counter>=25 && counter<=275&&counter%15==0){
                for(int i = 0; i < 5; i++){
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center+(Projectile.rotation+MathHelper.ToRadians(i*72f)).ToRotationVector2() * 92f,
                    (Projectile.rotation+MathHelper.ToRadians(i*72f)).ToRotationVector2()*getSpeed(summonPhase),
                    ModContent.ProjectileType<RinyaSubBossProjectile>(),
                    Projectile.damage,
                    0,
                    Main.myPlayer,
                    ownerNPC.whoAmI,
                    (int)summonPhase
                );
                }
            }     



            if(summonPhase==Phase.phase3){
                if(counter==50){
                    for(int i = 0; i < 5; i++){
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromAI(),
                            Projectile.Center+(Projectile.rotation+MathHelper.ToRadians(i*72f)).ToRotationVector2() * 92f,
                            Projectile.DirectionTo(targetPlayer.Center)*getSpeed(summonPhase),
                            ModContent.ProjectileType<RinyaSubBossProjectile>(),
                            Projectile.damage,
                            0,
                            Main.myPlayer,
                            ownerNPC.whoAmI,
                            (int)summonPhase
                        );
                    }
                }
            }

            if(summonPhase==Phase.phase4){
                if(counter==50||counter==250){
                    foreach(Player player in Main.player){
                        if(player.active && !player.dead){
                            for(int i = 0; i < 5; i++){
                                Projectile.NewProjectile(
                                    Projectile.GetSource_FromAI(),
                                    Projectile.Center+(Projectile.rotation+MathHelper.ToRadians(i*72f)).ToRotationVector2() * 92f,
                                    ownerNPC.DirectionTo(player.Center)*getSpeed(summonPhase),
                                    ModContent.ProjectileType<RinyaSubBossProjectile>(),
                                    Projectile.damage,
                                    0,
                                    Main.myPlayer,
                                    ownerNPC.whoAmI,
                                    (int)summonPhase
                                );
                            }
                        }
                    }
                }
            }



            

            

            if(counter>=300){
                Projectile.Kill();
                return;
            }
            
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }



        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null){
                return true;
            }
            
            Texture2D tex = _cachedTexture.Value;
            NPC ownerNPC = npcIndex.GetNPCOwner();
            
            if (ownerNPC == null || !ownerNPC.active){
                return true;
            }
            
            float alpha = 0.75f;
            
            Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
            
            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * alpha,
                Projectile.rotation,
                origin,
                Projectile.scale*scale,
                SpriteEffects.None,
                0
            );
            
            return false;
        }
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
        //     if (_cachedTexture == null)
        //         return base.Colliding(projHitbox, targetHitbox);
            
        //     Texture2D tex = _cachedTexture.Value;
        //     float x = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
        //     float y = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
            
        //     float distanceX = Projectile.Center.X - x;
        //     float distanceY = Projectile.Center.Y - y;
            
        //     return distanceX * distanceX + distanceY * distanceY <= Math.Pow(Projectile.scale, 2) * tex.Width / 2f * tex.Height / 2f;
        // }
        return false;
    }

    public float getSpeed(Phase phase){
            switch (phase)
            {
                case Phase.phase1:
                    return 9f;
                case Phase.phase2:
                    return 12f;
                case Phase.phase3:
                    return 15f;
                case Phase.phase4:
                    return 18f;
                default:
                    return 15f;
            }
        }
        public int getCoolDown(Phase phase){
            switch (phase)
            {
                case Phase.phase1:
                    return 18;
                case Phase.phase2:
                    return 12;
                case Phase.phase3:
                    return 16;
                case Phase.phase4:
                    return 16;
                default:
                    return 20;
            }
        }
    
}
}