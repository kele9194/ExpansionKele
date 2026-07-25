using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ExpansionKele.Content.Buff;
namespace ExpansionKele.Content.Projectiles.RangedProj
{
public class LightGapProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 6;
            Projectile.aiStyle = ProjAIStyleID.Arrow;//换这个
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            AIType = ProjectileID.Bullet;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; // 5的局部无敌帧
        }

        public override void AI()
        {
            //Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.6f);
            if(Main.rand.NextDouble()<0.25f){
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarDust, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 0, default(Color), 1f);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
           modifiers.ArmorPenetration+=20;//破甲
           if(target.velocity.Y!=0){
            modifiers.FinalDamage*=1.1f;//对空加成
           }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
        }
    }
}