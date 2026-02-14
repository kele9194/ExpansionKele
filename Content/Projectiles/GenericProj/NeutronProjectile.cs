using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Buff;


namespace ExpansionKele.Content.Projectiles.GenericProj{
public class NeutronProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 1; // 使用默认的飞行AI
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1; // 穿透一次
            Projectile.timeLeft = 600; // 弹幕存在时间
            Projectile.alpha = 63;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<NeutronDecay>(), 36000); // 添加NeutronDecay减益，持续36000ticks
        }
    }
}