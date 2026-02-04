using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class AAMissile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 0; // 设置为0以手动控制AI
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
    {
        // 定义最大追踪距离
        float maxTrackingDistance = Data.maxTrackingDistance; // 你可以根据需要调整这个值

        // 定义追踪速度和平滑度
        float speed = 30f;
        float turnResistance = 10f; // 调整这个值以改变追踪的平滑度
        Vector2 mousePosition = Main.MouseWorld;

        // 追踪目标
        ProjectileHelper.FindAndMoveTowardsTarget(Projectile, speed,maxTrackingDistance,turnResistance, mousePosition);
    }


       public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    target.immune[Projectile.owner] = 1;
}
public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.velocity.Y != 0)
            {
                modifiers.FinalDamage *= 1.5f; // 增加200%伤害
            }
        }
    }
}