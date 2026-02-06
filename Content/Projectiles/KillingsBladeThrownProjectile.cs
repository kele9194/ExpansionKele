using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using System;
using ReLogic.Content;

namespace ExpansionKele.Content.Projectiles
{
    public class KillingsBladeThrownProjectile : ModProjectile
    {
        
        // 旋转半径
        // ... existing code ...
        // 旋转半径
        // ... existing code ...
        // 旋转半径
        private const float ROTATION_RADIUS = 48f;
        
        // 初始旋转速度（30度每帧）
        private static readonly float INITIAL_ROTATION_SPEED = MathHelper.ToRadians(60);
        
        // 初始移动速度
        private const float INITIAL_MOVEMENT_SPEED = 24f;
        
        // 减速过程持续帧数
        private const int DECELERATION_DURATION = 150;
        
        // 初始偏移角度
        private float _initialAngle;
        
        // 抛射体中心位置
        private Vector2 _centerPosition;
        
        // 旋转角度
        private float _rotationAngle;
        
        // 已经过的帧数
        private int _elapsedFrames = 0;
// ... existing code ...
// ... existing code ...
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
        
        public override void SetStaticDefaults()
        {
        }

        // ... existing code ...
        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 58;
            Projectile.aiStyle = -1; // 不使用默认AI
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 200; // 改为200帧
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        
        // ... existing code ...
        public override void OnSpawn(IEntitySource source)
        {
            // 获取纹理尺寸并设置碰撞箱
            Texture2D texture = _cachedTexture.Value;
            Projectile.width = texture.Width;
            Projectile.height = texture.Height;
            
            // 初始化参数
            _initialAngle = Projectile.velocity.ToRotation();
            // 设置中心点为初始位置
            _centerPosition = Projectile.Center;
            // 初始旋转角度为发射角度
            _rotationAngle = _initialAngle;
            _elapsedFrames = 0; // 初始化计数器
            Projectile.netUpdate = true;
        }

        // ... existing code ...
        public override void AI()
        {
            _elapsedFrames++; // 增加已过的帧数
            
            // 计算减速比例（0到1之间）
            float progress = Math.Min(1f, (float)_elapsedFrames / DECELERATION_DURATION);
            
            // 使用平方函数使减速更自然
            float decelerationFactor = 1f - progress * progress;
            
            // 计算当前旋转速度
            float currentRotationSpeed = INITIAL_ROTATION_SPEED * decelerationFactor;
            
            // 更新旋转角度
            _rotationAngle += currentRotationSpeed * Projectile.direction;
            
            // 设置旋转角度以匹配视觉效果
            Projectile.rotation = _rotationAngle + MathHelper.PiOver4;
            
            // 中心点沿初始方向移动，实现直线飞行
            Vector2 movement = _initialAngle.ToRotationVector2() * INITIAL_MOVEMENT_SPEED * decelerationFactor;
            _centerPosition += movement;
            Projectile.Center = _centerPosition;
            Projectile.netUpdate = true;
        }
// ... existing code ...
// ... existing code ...
// ... existing code ...
        
        public override void OnKill(int timeLeft)
        {
            // 生成粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f);
            }
        }
    }
}