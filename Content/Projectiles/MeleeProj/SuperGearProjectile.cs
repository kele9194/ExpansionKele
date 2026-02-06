using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class SuperGearProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles";
        float u = 0.1f; // 转速倍率
        bool bounce = false; // 是否处于反弹状态

        public override void SetStaticDefaults()
        {
        }
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
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 200;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.scale=0.625f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.netUpdate = true;
            
            // 不受重力影响
            Projectile.aiStyle = -1;
        }

        // ... existing code ...
        public override void AI()
        {
            // 齿轮始终朝向移动方向
            if (Projectile.velocity != Vector2.Zero)
            {
                // 计算基于穿透次数的旋转速度

                float t = Projectile.penetrate; // 剩余穿透次数
                float rotationSpeed;
                
                // 如果是初始状态，使用初始旋转速度公式
                if (t == 6) // 初始穿透次数为6
                {
                    rotationSpeed = 90f * u; // 90*u rad/s
                }
                else
                {
                    // 使用新公式: 10*u*sqrt(12*t+9)
                    rotationSpeed = 10f * u * (float)Math.Sqrt(12f * t + 9f);
                }
                
                // 直接使用旋转速度值作为角度增量（已考虑为合适的单位）
                Projectile.rotation += MathHelper.ToRadians(rotationSpeed);
            }
            
            // 添加简单的尾迹粒子效果
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
// ... existing code ...
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取弹丸纹理
            Texture2D texture = _cachedTexture.Value;
            
            // 计算绘制原点为中心点
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // 计算屏幕位置
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            
            // 使用自定义缩放值进行绘制，保持中心对齐
            float customScale = 0.625f; // 使用你原来的缩放值
            
            // 绘制弹丸
            Main.EntitySpriteDraw(
                texture, 
                drawPosition, 
                null, 
                lightColor, 
                Projectile.rotation, 
                origin, 
                customScale, 
                SpriteEffects.None, 
                0
            );
            
            // 返回false表示我们已经处理了绘制，不需要默认绘制
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 击中敌人后不完全停止移动，而是稍微减速并改变方向
            Projectile.velocity *= 0.1f; // 保持一定速度而不是完全停止
            //Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4); // 随机改变方向
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 反弹功能 - 消耗一次穿透次数
            if (Projectile.penetrate > 1) // 确保还有穿透次数用于反弹
            {
                // 消耗一次穿透次数
                Projectile.penetrate--;
                
                // 反弹逻辑 - 保持原速
                if (Projectile.velocity.X != oldVelocity.X)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
                
                // 确保速度大小不变
                float speed = oldVelocity.Length();
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * speed;
                
                Projectile.netUpdate = true;
                return false; // 不销毁弹幕
            }
            
            // 穿透次数不足时不反弹，正常销毁
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // 消失时产生粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemDiamond, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }
    }
}