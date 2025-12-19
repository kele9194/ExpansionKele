using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class AutoAimingSniperBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("自动瞄准狙击弹");
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 播放声音
            SoundEngine.PlaySound(SoundID.Item40, Projectile.Center);
            
            // 添加粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default, 1f);
            }
        }

        public override void AI()
        {
            // 添加拖尾效果
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default, 0.5f);
            
            // 设置光线
            Lighting.AddLight(Projectile.Center, 0.3f, 0.45f, 0.8f);

            // 设置旋转角度与移动方向一致
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            
            // 绘制子弹
            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                1f,
                SpriteEffects.None,
                0
            );
            
            return false; // 我们已经手动绘制了，不需要默认绘制
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中敌人时产生特效
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default, 1f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡时产生粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, default, 1.5f);
            }
        }
    }
}