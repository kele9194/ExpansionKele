using System;
using InnoVault;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static ExpansionKele.Content.Bosses.BossKeleNew.BossKeleNew;

namespace ExpansionKele.Content.Bosses.BossKeleNew
{
    public class BossHighVelocityBullet : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_242";
        [SyncVar]
        public int npcIndex;
        [SyncVar]
        public Phase rangedPhase;
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.light = 1f;
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
            }
            Projectile.rotation = Projectile.velocity.ToRotation() +MathHelper.PiOver2;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            NPC ownerNPC = npcIndex.GetNPCOwner();
            if (ownerNPC == null || !ownerNPC.active)
            {
                return;
            }
            if(rangedPhase==Phase.phase3){
                int maxHealAmount = (int)(ownerNPC.lifeMax * 0.5f);
                int missingHealth = ownerNPC.lifeMax - ownerNPC.life;
                int healAmount = Math.Min((int)(missingHealth * 0.04f), maxHealAmount - ownerNPC.life);
                
                if(healAmount > 0){
                    ownerNPC.life += healAmount;
                    ownerNPC.HealEffect(healAmount, false);
                    ownerNPC.netUpdate=true;
                }
            }
            if(rangedPhase==Phase.phase4){
                int maxHealAmount2 = (int)(ownerNPC.lifeMax * 0.25f);
                int missingHealth = ownerNPC.lifeMax - ownerNPC.life;
                int healAmount = Math.Min((int)(missingHealth * 0.04f), maxHealAmount2 - ownerNPC.life);
                
                if(healAmount > 0){
                    ownerNPC.life += healAmount;
                    ownerNPC.HealEffect(healAmount, false);
                    ownerNPC.netUpdate=true;
                }
            }
        }
    }
}