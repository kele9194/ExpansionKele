using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles
{
    public class ResentmentProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("怨憎");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1; // 只能穿透一个敌人
            Projectile.timeLeft = 600;
            Projectile.alpha = 255; // 透明度
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // 添加粒子效果
            Lighting.AddLight(Projectile.Center, 0.05f, 0.2f, 0.3f);
            Projectile.rotation= Projectile.velocity.ToRotation()+MathHelper.PiOver2;
            
            // 创建轨迹粒子
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            
            // 弹幕命中敌人恢复10点生命值
            player.Heal(10);
            
            // 击中敌人后产生特效
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 弹幕消失时的粒子效果
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
            
            // 播放音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(130, 0, 0, Projectile.alpha);
        }
    }
}