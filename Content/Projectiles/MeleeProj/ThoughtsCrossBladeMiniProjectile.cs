using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using ExpansionKele.Content.Buff;
using ReLogic.Content;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class ThoughtsCrossBladeMiniProjectile : ModProjectile
    {
        // 存储剑影纹理的宽度和高度
        private int textureWidth;
        private int textureHeight;
        // 存储剑影的总长度
        private float textureLength;

        private float baseLineWidth = 16; // 更细的线宽
        // 存储初始方向
        private Vector2 initialDirection; // 默认为右上到左下方向
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
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 0.5f; // 一半大小
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.netUpdate = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }
        //要同步全部放AI
        
        public override void AI()
        {
            // 获取纹理尺寸
            Texture2D texture = _cachedTexture.Value;
            textureWidth = texture.Width;
            textureHeight = texture.Height;

            // 根据勾股定理计算剑影的总长度
            textureLength = (float)Math.Sqrt(textureWidth * textureWidth + textureHeight * textureHeight);

            // 使用AI[0]作为角度输入，正数为逆时针，负数为顺时针
            float angle = Projectile.ai[0];
            // 将角度转换为弧度
            float radians = MathHelper.ToRadians(angle);
            // 基于默认方向(-1, -1)应用旋转
            Vector2 defaultDirection = new Vector2(-1, -1);
            defaultDirection.Normalize();
            // 应用旋转变换
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            initialDirection = new Vector2(
                defaultDirection.X * cos - defaultDirection.Y * sin,
                defaultDirection.X * sin + defaultDirection.Y * cos
            );
            initialDirection.Normalize();
            // 同时设置Projectile.rotation用于绘制
            Projectile.rotation = radians;

            // 使用AI[1]存储scale
            Projectile.scale = Projectile.ai[1];
            // 更新textureLength以匹配scale
            textureLength *= Projectile.scale;
            Player player = Main.player[Projectile.owner];

            // 基本设置 - 移除了 player.ChangeDir(Projectile.direction)
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation;

            // 使用ai[2]作为计时器
            Projectile.ai[2]++;
            int timer = (int)Projectile.ai[2];
            int totalDuration = 20;

            if (timer >= totalDuration)
            {
                Projectile.Kill();
                return;
            }

            // 随着时间推移逐渐降低透明度，在20帧内从1降到0
            Projectile.Opacity = 1f - (timer / (float)totalDuration);
        }

        // ... existing code ...
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 施加2秒的SlicingBuff减益效果
            target.AddBuff(ModContent.BuffType<SlicingBuff>(), 120); // 2秒 = 120帧
        }
// ... existing code ...

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 使用更精确的碰撞检测
            float collisionPoint = 0f;
            // 使用实际的剑影长度进行碰撞检测，使用基于角度计算的方向向量
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center - initialDirection * (textureLength / 2),
                Projectile.Center + initialDirection * (textureLength / 2),
                baseLineWidth, ref collisionPoint
            );
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理
            Texture2D texture = _cachedTexture.Value;

            // 计算原点
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // 绘制主剑影，使用Projectile.rotation作为旋转角度
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null,
                Color.Purple * Projectile.Opacity, Projectile.rotation, origin,
                Projectile.scale, SpriteEffects.None, 0);


            return false;
        }
        

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 设置伤害为原始伤害的66%
            modifiers.FinalDamage *= 1.2f;
        }
    }
}