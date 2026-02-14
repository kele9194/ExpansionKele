using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.GenericProj
{
    public class FullMoonExplosion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // 设置爆炸效果
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 10; // 存活时间很短
            Projectile.tileCollide = false; // 不与地形碰撞
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 在生成时播放爆炸音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            
            // 创建爆炸粒子效果
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.BlueFlare, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        public override void AI()
        {
            // 快速淡出效果
            Projectile.alpha += 25;
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
            
            // 添加光照效果
            Lighting.AddLight(Projectile.Center, Color.LightBlue.ToVector3() * 0.5f);
        }

                public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中敌人时创建额外的视觉效果
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(new Vector2(target.Hitbox.X, target.Hitbox.Y), target.width, target.height, 
                    DustID.BlueFlare, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 增加护甲穿透
            modifiers.ArmorPenetration += 20;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 可以在这里添加自定义绘制效果
            return true;
        }
    }
}