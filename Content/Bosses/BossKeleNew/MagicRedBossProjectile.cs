using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using InnoVault;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;
using ExpansionKele.Content.Customs;
using Microsoft.Build.Evaluation;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public static class DataBase{
        public static float getSpeed(this Phase phase){
            switch (phase)
            {
                case Phase.phase1:
                    return 12f;
                case Phase.phase2:
                    return 14f;
                case Phase.phase3:
                    return 16f;
                case Phase.phase4:
                    return 18f;
                default:
                    return 15f;
            }
        }
    }
    public class MagicRedBossProjectile : ModProjectile
    {
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase magicPhase;
        [SyncVar]
        public float counter = 0f;
        public const float MaxCounterFrames = 60f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            magicPhase = (Phase)(int)Math.Round(Projectile.ai[1]);
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

            float MaxCounterFrames = 60f;
            float normalizedTime = MathHelper.Clamp(counter / MaxCounterFrames,0f,1f);
            counter++;
            
            switch (magicPhase)
            {
                case Phase.phase1:
                    float trackingP1 = MathHelper.Clamp(MathHelper.Lerp(0.4f, 0f, normalizedTime),0f,1f);
                    NPCHomingProjectileExtensions.BaseUpdateTraking(Projectile, targetPlayer, magicPhase.getSpeed(), trackingP1,MathHelper.Pi/60);
                    break;
                    
                case Phase.phase2:
                    float trackingP2 = MathHelper.Clamp(MathHelper.Lerp(0.5f, 0f, normalizedTime),0f,1f);
                    NPCHomingProjectileExtensions.BaseUpdateTraking(Projectile, targetPlayer, magicPhase.getSpeed(), trackingP2,MathHelper.Pi/60);
                    break;
                    
                case Phase.phase3:
                    float trackingP3 = MathHelper.Lerp(0.5f, 0f, normalizedTime);
                    NPCHomingProjectileExtensions.UpdateTrackingWithPrediction(Projectile, targetPlayer, magicPhase.getSpeed(), trackingP3 ,MathHelper.Pi/60);
                    break;
                    
                case Phase.phase4:
                    float trackingP4 = MathHelper.Lerp(0.5f, 0.1f, normalizedTime);
                    NPCHomingProjectileExtensions.UpdateTrackingWithPrediction(Projectile, targetPlayer, magicPhase.getSpeed(), trackingP4 ,MathHelper.Pi/60);
                    break;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        

        

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Main.NewText("3");
                return;
            }

            target.AddBuff(BuffID.OnFire, 180);
            Projectile.Kill();
            Projectile.netUpdate=true;
            
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            
        }
    }

    public class MagicBlueBossProjectile : ModProjectile
    {
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase magicPhase;
        [SyncVar]
        public float counter = 0f;
        public const float MaxCounterFrames = 60f;
        [SyncVar]
        public bool init = true;
        [SyncVar]
        public Vector2 Center;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override bool? CanDamage(){
            return Projectile.timeLeft <=580;
        }

        // ... existing code ...
        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            magicPhase = (Phase)(int)Math.Round(Projectile.ai[1]);
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }

            if(!ownerNPC.HasValidTarget){
                return;
            }
            if(init){
                Center=ownerNPC.Center;
                init = false;
            }
            
            Vector2 inwardDirection = Center - Projectile.Center;
            float distanceToCenter = inwardDirection.Length();
            
            if (distanceToCenter < 120f)
            {
                Projectile.Kill();
                return;
            }
            
            inwardDirection.Normalize();
            
            float fixedSpeed = 10f;
            float angleOffset = MathHelper.ToRadians(80f);
            
            Vector2 spiralDirection = inwardDirection.RotatedBy(angleOffset);
            
            Projectile.velocity = spiralDirection * fixedSpeed;
            
            counter++;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
// ... existing code ...
        


        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                return;
            }

            target.AddBuff(BuffID.ManaSickness, 180);
            target.statMana -= 20;
            if (target.statMana < 0)
            {
                target.statMana = 0;
            }
            target.ManaEffect(20);
        }
    }

    public class MagicPurpleBossProjectile : ModProjectile
    {
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase magicPhase;
        [SyncVar]
        public float counter = 0f;
        public const float MaxCounterFrames = 60f;
        private Vector2 initialDirection;
        private bool directionInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;

        }
        public override bool? CanDamage(){
            return Projectile.timeLeft <=580;
        }

        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            magicPhase = (Phase)(int)Math.Round(Projectile.ai[1]);
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }

            if(!ownerNPC.HasValidTarget){
                return;
            }
            

            if (!directionInitialized)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                directionInitialized = true;
            }

            float accelerationTime = 60f;
            float maxSpeed = magicPhase.getSpeed();
            
            if (counter < accelerationTime)
            {
                float progress = counter / accelerationTime;
                float currentSpeed = maxSpeed * progress;
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * currentSpeed;
            }
            else
            {
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * maxSpeed;
            }
            
            counter++;

            if (counter >= 80f)
            {
                Projectile.Kill();
                return;
            }

        }



        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                return;
            }

            target.AddBuff(BuffID.Venom, 180);
            
            
        }
    }

    public class MagicCyanBossProjectile : ModProjectile
    {
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase magicPhase;
        [SyncVar]
        public float counter = 0f;
        public const float MaxCounterFrames = 60f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 200;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            npcIndex = (int)Math.Round(Projectile.ai[0]);
            magicPhase = (Phase)(int)Math.Round(Projectile.ai[1]);
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                Projectile.Kill();
                return;
            }

            if(!ownerNPC.HasValidTarget){
                return;
            }
            
            Projectile.rotation=Projectile.velocity.ToRotation();
            Projectile.velocity*=0.99f;
        }


        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                return;
            }

            target.AddBuff(BuffID.Frostburn, 180);

        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            
        }
    }

    

    
}