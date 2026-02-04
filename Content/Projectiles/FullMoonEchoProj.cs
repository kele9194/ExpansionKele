using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles
{
    /// <summary>
    /// 满月回响弹幕：
    /// 初始高速飞行，在前 120 ticks 内减速至原始速度的 40%，
    /// 随后进入返回状态并逐渐加速至原始速度。
    /// 返回时造成 60% 伤害，并忽视敌人 20 点防御。
    /// 接触玩家后销毁，否则持续存在。
    /// </summary>
    public class FullMoonEchoProj : ModProjectile
    {
        // ====== 常量配置项 ======

        /// <summary>初始速度比例，用于控制发射初速</summary>
        private const float InitialSpeedMultiplier = 1f;

        /// <summary>减速阶段结束时的速度比例（默认为 20%）</summary>
        private const float FinalSpeedMultiplier = 0.2f;

        /// <summary>减速阶段持续时间（单位：tick）</summary>
        private const int DecelerationDuration = 120;

        /// <summary>返回阶段加速度因子，用于控制归航加速速度</summary>
        private const float ReturningAccelerationFactor = 0.02f;

        /// <summary>返回阶段最大速度比例（默认为 100%，即恢复到原始速度）</summary>
        private const float MaxReturnSpeedMultiplier = 1f;

        /// <summary>返回阶段造成的伤害比例（默认为 60%）</summary>
        private const float ReturningDamageMultiplier = 0.6f;

        /// <summary>忽视敌人防御值（默认为 20 点）</summary>
        private const int IgnoreDefenseValue = 20;

        // ====== 状态管理 ======

        /// <summary>表示当前弹幕状态（前进 or 返回）</summary>
        private enum ArrowState
        {
            Forward,
            Returning
        }

        /// <summary>当前弹幕所处的状态</summary>
        private ArrowState _state = ArrowState.Forward;

        /// <summary>记录弹幕发射时的初始速度大小</summary>
        private float _initialSpeed;

        /// <summary>是否已进入返回阶段</summary>
        private bool _isReturning = false;

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

        // ====== 弹幕初始化 ======

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不与地形碰撞
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;  // 使用本地无敌帧系统
            Projectile.localNPCHitCooldown = 20;

            Projectile.timeLeft = 600; // 初始生命周期为 600 ticks（10 秒）
        }

        // ====== 主循环逻辑 ======

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 记录初始速度（仅一次）
            if (Projectile.ai[0] == 0)
            {
                _initialSpeed = Projectile.velocity.Length();
                Projectile.ai[0] = 1;
            }

            // 根据状态执行不同行为
            switch (_state)
            {
                case ArrowState.Forward:
                    HandleForwardMotion();
                    break;
                case ArrowState.Returning:
                    HandleReturningMotion(owner);
                    break;
            }

            // 添加旋转动画，使箭矢始终朝向移动方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        // ====== 前进阶段逻辑 ======

        /// <summary>
        /// 前进阶段：发射后线性减速至设定比例（FinalSpeedMultiplier）
        /// </summary>
        private void HandleForwardMotion()
        {
            if (!_isReturning)
            {
                // 计算当前减速进度（从 0 到 1）
                float progress = (600 - Projectile.timeLeft) / (float)DecelerationDuration;
                if (progress > 1f) progress = 1f;

                // 插值计算当前速度
                float speedFactor = MathHelper.Lerp(InitialSpeedMultiplier, FinalSpeedMultiplier, progress);
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.velocity = direction * (_initialSpeed * speedFactor);

                // 达到指定减速时间后切换为返回状态
                if (Projectile.timeLeft <= 600 - DecelerationDuration)
                {
                    _state = ArrowState.Returning;
                    _isReturning = true;
                    Projectile.timeLeft = 1000; // 设置为极大值使其不会自动销毁
                }
            }
        }

        // ====== 返回阶段逻辑 ======

        /// <summary>
        /// 返回阶段：朝向玩家缓慢加速，接触后销毁。
        /// 返回过程中持续生成淡蓝色粒子特效。
        /// </summary>
        private void HandleReturningMotion(Player owner)
        {
            Vector2 directionToPlayer = owner.Center - Projectile.Center;
            float distance = directionToPlayer.Length();

            // 距离玩家较近时销毁
            if (distance < 16f)
            {
                Projectile.Kill();
                return;
            }

            directionToPlayer.Normalize();

            // 目标速度为初始速度的一定比例（MaxReturnSpeedMultiplier）
            float targetSpeed = _initialSpeed * MaxReturnSpeedMultiplier;
            Vector2 desiredVelocity = directionToPlayer * targetSpeed;

            // 平滑加速至目标速度
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.05f);

            // 添加返回特效
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.BlueTorch,
                    -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f,
                    100,
                    default,
                    1.2f
                );
                dust.noGravity = true;
            }
        }

        // ====== 攻击逻辑修改 ======

        /// <summary>
        /// 修改对 NPC 的打击效果：
        /// - 返回阶段：伤害减少为 60%
        /// - 所有阶段：忽视敌人 20 点防御
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (_state == ArrowState.Returning)
            {
                modifiers.SourceDamage *= ReturningDamageMultiplier;
                modifiers.ArmorPenetration += IgnoreDefenseValue;
            }
            else
            {
                modifiers.ArmorPenetration += IgnoreDefenseValue;
            }
        }

        // ====== 视觉绘制逻辑 ======

        /// <summary>
        /// 自定义绘制逻辑，实现拖尾、颜色变化、发光等视觉反馈。
        /// - 前进阶段：红色调
        /// - 返回阶段：蓝色调
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = _cachedTexture.Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            // 设置当前状态颜色
            Color drawColor = _state == ArrowState.Forward ? Color.Red : Color.Cyan;

            // 绘制拖尾效果
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin;
                float scale = Projectile.scale * (1f - k / (float)Projectile.oldPos.Length);
                Color trailColor = drawColor * ((1f - k / (float)Projectile.oldPos.Length) * 0.5f);
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, drawOrigin, scale, SpriteEffects.None, 0);
            }

            // 绘制主弹幕
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            // 添加发光图层
            Lighting.AddLight(Projectile.Center, drawColor.ToVector3() * 0.75f);

            return false; // 表示我们完全自定义了绘制过程
        }
    }
}