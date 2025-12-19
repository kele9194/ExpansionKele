using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class SixthSniperBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("第六发狙击弹");
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
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, Color.Gold, 1.5f);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SetCrit();
        }

        public override void AI()
        {
            // 添加拖尾效果
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, Color.Gold, 1f);

            // 设置光线
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.3f);

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
                Color.Gold,
                Projectile.rotation,
                origin,
                1.2f,
                SpriteEffects.None,
                0
            );

            return false; // 我们已经手动绘制了，不需要默认绘制
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 计算目标当前生命值的6%或已损失生命值的6%作为额外伤害
            int maxLife = target.lifeMax;
            int currentLife = target.life;
            int lostLife = maxLife - currentLife;

            // 取较大值作为额外伤害
            int extraDamage = (int)((currentLife * 0.06f > lostLife * 0.06f) ? currentLife * 0.06f : lostLife * 0.06f);

            // 应用额外伤害 使用ApplyDamageToNPC方法
            if (extraDamage > 0)
            {
                Main.player[Projectile.owner].ApplyDamageToNPC(target, extraDamage, 0f, Projectile.direction, false);
            }

            // 击中敌人时产生特效
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, Color.Gold, 2f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 死亡时产生粒子效果
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0f, 0f, 100, Color.Gold, 2f);
            }
        }
    }
}