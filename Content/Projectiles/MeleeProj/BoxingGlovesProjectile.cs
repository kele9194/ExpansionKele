using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Diagnostics.Metrics;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    /// <summary>
    /// 拳击手套抛射体
    /// 
    /// AI 数组使用说明:
    /// <list type="bullet">
    ///     <item><description>ai[0] - 左右手标识：0=左手，1=右手 (由 Shoot 方法设置，决定使用哪个手的动画)</description></item>
    ///     <item><description>ai[1] - 初始速度 X 分量</description></item>
    ///     <item><description>ai[2] - 初始速度 Y 分量</description></item>
    /// <item><description>不要用ai[3]</description></item>
    /// </list>
    /// 
    /// 抛射体的 frame 由 ai[0] 决定，无需额外状态管理
    /// </summary>
    /// /// <summary>
    /// 拳击手套的手部类型
    /// </summary>
    public enum HandTypeEnum
    {
        LeftHand = -1,
        RightHand = 1
    }
    
    /// <summary>
    /// 弧线运动方向
    /// </summary>
    public enum ArcDirection
    {
        CounterClockwise = -1,
        Clockwise = 1
    }
     public class BoxingGlovesProjectile : ModProjectile
    {
        /// <summary>
        /// 获取左右手标识
        /// </summary>
        private int HandType => (int)Projectile.ai[0];
        
        private int _arcDirection;
        
        
        private const float ArcRadius = 550;
        private const float ArcAngle = MathF.PI / 4;
        private static readonly float MaxDistance = 2 * MathF.Sin(ArcAngle / 2)*ArcRadius;
        
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner < 0 || Projectile.owner >= Main.player.Length)
            {
                Projectile.Kill();
                return;
            }
            
            Player player = Main.player[Projectile.owner];
            
            if (!player.active)
            {
                Projectile.Kill();
                return;
            }
            if(HandType== (int)HandTypeEnum.LeftHand){
                Projectile.frame=0;
            }
            else if(HandType==(int)HandTypeEnum.RightHand){
                Projectile.frame=1;
            }
            
            
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = direction * 16f;
            
            Projectile.ai[1] = Projectile.velocity.X;
            Projectile.ai[2] = Projectile.velocity.Y;
            
            bool isLeftHand = HandType == (int)HandTypeEnum.LeftHand;
            bool mouseFacingRight = direction.X > 0;
            
            if (isLeftHand)
            {
 
                    _arcDirection = (int)ArcDirection.CounterClockwise;

            }
            else{
                    _arcDirection = (int)ArcDirection.Clockwise;
            }
        }

// ... existing code ...

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            
            float distanceFromPlayer = Vector2.Distance(Projectile.Center, player.Center);
            
            if (distanceFromPlayer >= MaxDistance)
            {
                Projectile.Kill();
                return;
            }
            
            Vector2 originalVelocity = new Vector2(Projectile.ai[1], Projectile.ai[2]);
            Vector2 direction = originalVelocity.SafeNormalize(Vector2.UnitX);
            float rotationAngle = (MathF.PI-ArcAngle)/2 * _arcDirection;
            Vector2 rotatedDirection = Vector2.Transform(direction, Matrix.CreateRotationZ(rotationAngle));
            
            
            float arcRadius = ArcRadius;
            Vector2 circleCenter = player.Center + rotatedDirection * arcRadius;
            
            Vector2 toCircleCenter = Projectile.Center - circleCenter;
            float currentDistanceFromCenter = toCircleCenter.Length();
            
            Vector2 radiusDirection = toCircleCenter.SafeNormalize(Vector2.UnitX);
            
            float distanceError = currentDistanceFromCenter - arcRadius;
            Projectile.Center -= radiusDirection * distanceError;
            
            toCircleCenter = Projectile.Center - circleCenter;
            radiusDirection = toCircleCenter.SafeNormalize(Vector2.UnitX);
            
            Vector2 tangentDirection = new Vector2(-radiusDirection.Y, radiusDirection.X) * _arcDirection;
            
            float speed = 20f;
            Vector2 newVelocity = tangentDirection * speed;
            
            Projectile.velocity = newVelocity + player.velocity;
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            
            
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Smoke, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
// ... existing code ...
        }
        
// ... existing code ...
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }
    }
}