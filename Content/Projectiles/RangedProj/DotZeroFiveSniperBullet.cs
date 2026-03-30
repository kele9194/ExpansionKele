using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using ReLogic.Content;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class DotZeroFiveSniperBullet : ModProjectile
    {
        public static readonly int[] CanDamageFrac = new int[2] { 1, 3 };
        public static float damageRatio = 0.01f;
        private static Asset<Texture2D> _cachedTexture;

        public override void Load()
        {
            // 预加载纹理资源
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }

        public override void Unload()
        {
            // 清理资源引用
            _cachedTexture = null;
        }
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 20;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // 局部无敌帧为-1
            Projectile.alpha=4;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 设置弹幕在鼠标位置生成
            Projectile.Center = Main.MouseWorld;
            // 生成后速度变为0
            Projectile.velocity = Vector2.Zero;
            // 设置透明度为4
            Projectile.alpha = 4;
        }

        public override void AI()
        {
            // 添加拖尾效果
            Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 100, default, 1f);
            
            // 设置光线
            Lighting.AddLight(Projectile.Center, 0.3f, 0.45f, 0.7f);

            // 设置旋转角度与移动方向一致
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理
            Texture2D texture = _cachedTexture.Value;
            
            // 计算缩放后的尺寸 (默认2倍大小)
            float scaleMultiplier = 5f;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            
            // 绘制子弹，放大2倍
            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                Color.White*0.05f,
                Projectile.rotation,
                origin,
                scaleMultiplier,
                SpriteEffects.None,
                0
            );
            
            return false; // 我们已经手动绘制了，不需要默认绘制
        }

        // ... existing code ...
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 1/6 概率触发额外伤害
            if (Main.rand.NextFloat() < 1f / 6f)
            {
                // 计算额外伤害：目标当前生命值的 1%
                int extraDamage = (int)(target.lifeMax * 0.01f);
                
                // 确保至少有 1 点伤害
                if (extraDamage < 1)
                {
                    extraDamage = 1;
                }
                
                // 获取发射该弹幕的玩家
                Player player = Main.player[Projectile.owner];
                
                // 对目标造成额外伤害
                player.ApplyDamageToNPC(target, extraDamage, 0f, 0, false);
                
            }
        }
// ... existing code ...
        public override void OnKill(int timeLeft)
        {
            // 死亡时产生粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 100, default, 1.5f);
            }
            

        }
    }
}