using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using ExpansionKele.Content.Buff;
namespace ExpansionKele.Content.Projectiles.RangedProj
{
public class FrostRayProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
            AIType = ProjectileID.Bullet;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; // 5的局部无敌帧
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.6f);
            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default(Color), 1f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 获取目标的GlobalNPC实例
            FrostShatterNPC frostShatterNPC = target.GetGlobalNPC<FrostShatterNPC>();
            
            // 添加或增加冰碎减益时间
            frostShatterNPC.AddFrostShatterTime(target, 60);
            
            // 应用debuff，持续时间为当前减益时间
            int debuffTime = frostShatterNPC.frostShatterTimes[target.whoAmI];
            target.AddBuff(ModContent.BuffType<FrostShatterDebuff>(), debuffTime);
        }
    }
}