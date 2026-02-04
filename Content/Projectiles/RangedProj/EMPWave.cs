using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class EMPWave : ModProjectile
    {
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
        private const int AnimationDuration = 20; // 动画持续时间（帧）
        
        public override void SetDefaults()
        {
            Projectile.width = 600; // 300像素半径 * 2
            Projectile.height = 600;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = AnimationDuration; // 设置持续时间为20帧
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -1; // 不使用默认AI
            Projectile.damage = 1; // 造成1点伤害
        }

        public override void AI()
        {
            // 计算动画进度 (0.0 到 1.0)
            float progress = 1f - (Projectile.timeLeft / (float)AnimationDuration);
            
            // 缩放从1倍到2倍
            Projectile.scale = 1f + progress * 1f;
            
            // 透明度从200逐渐降至0
            Projectile.alpha = (int)(200 * (1f - progress));
            
            // 持续减慢范围内的敌人
            SlowDownEnemies();
        }

        private void SlowDownEnemies()
        {
            const float range = 256;
            
            // 减速附近的敌人
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance <= range * Projectile.scale)
                    {
                        // 减慢敌人速度到0
                        npc.velocity *= 0f;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用贴图绘制EMP波效果
            Texture2D texture = _cachedTexture.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = Color.White * (1f - (Projectile.alpha / 255f)); // 根据alpha值调整透明度
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 scale = new Vector2(Projectile.scale);
            
            Main.EntitySpriteDraw(texture, drawPos, null, color, 0f, origin, scale, SpriteEffects.None, 0);
            
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            // 在整个动画过程中都可以造成伤害
            return true;
        }
    }
}