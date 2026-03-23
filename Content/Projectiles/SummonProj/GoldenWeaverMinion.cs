using System;
using ExpansionKele.Content.Buff;
using ExpansionKele.Content.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.SummonProj
{
    public class GoldenWeaverMinion : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.SummonProj";
        
        // AI 状态枚举
        private enum AttackState
        {
            Idle,           // 空闲状态（悬浮）
            Charging,       // 冲刺准备状态
            Dashing,        // 冲刺状态
            Seeking         // 寻找新目标状态
        }

        // 常量定义
        private const float DETECTION_RANGE = 600f;     // 目标检测范围
        private const float IDLE_HEIGHT = 50f;          // 空闲时距离玩家头顶的高度
        private const float DASH_SPEED = 25f;           // 提升冲刺速度（原 15f）
        private const float SEEK_SPEED = 12f;           // 寻找新目标速度
        private const int BASE_DASH_DURATION = 45;      // 基础冲刺持续时间
        private const int MAX_DASH_DURATION = 90;       // 最大冲刺持续时间
        private const int COOLDOWN_DURATION = 45;       // 缩短攻击冷却时间（原 60）
        private const float MAX_DASH_DISTANCE = 800f;   // 最大冲刺距离
        private const int MAX_CONSECUTIVE_DASHES = 3;   // 最大连续冲刺次数

        // 状态变量
        private AttackState currentState = AttackState.Idle;
        private Vector2 idlePosition = Vector2.Zero;    // 空闲位置
        private NPC targetNPC = null;                   // 当前目标
        private Vector2 dashDirection = Vector2.Zero;   // 冲刺方向
        private int stateTimer = 0;                     // 状态计时器
        private int cooldownTimer = 0;                  // 冷却计时器
        private Vector2 startPosition = Vector2.Zero;   // 冲刺起始位置
        private int consecutiveDashes = 0;              // 连续冲刺计数

        public override void SetStaticDefaults()
        {
            // 设置为仆从类型
            Main.projPet[Projectile.type] = true;
            
            // 设置弹幕 ID 集合
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // 不可被牺牲（永久存在）
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // 对邪教徒有抗性
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // 支持右键锁定目标
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.friendly = true; // 友善弹幕
            Projectile.minion = true; // 标记为仆从
            Projectile.DamageType = DamageClass.Summon; // 明确指定伤害类型
            Projectile.minionSlots = 0f; // 不占用召唤栏位（关键）
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 360; // 存在时间（虽然会自动续期）
            Projectile.ignoreWater = true; // 忽略水
            Projectile.usesLocalNPCImmunity = true; // 使用本地 NPC 无敌帧
            Projectile.localNPCHitCooldown = 20; // 本地 NPC 击中冷却
        }

        public override bool? CanCutTiles()
        {
            return false; // 不能切割草
        }

        public override bool MinionContactDamage()
        {
            return true; // 开启接触伤害
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            modifiers.FinalDamage *= GoldenWeaverNeedle.GetBonusMultiplier(player);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 活跃检查
            if (!CheckActive(owner))
            {
                return;
            }
            
            // 更新冷却计时器
            if (cooldownTimer > 0)
            {
                cooldownTimer--;
            }

            // 根据当前状态执行不同行为
            switch (currentState)
            {
                case AttackState.Idle:
                    HandleIdleState(owner);
                    break;
                case AttackState.Charging:
                    HandleChargingState();
                    break;
                case AttackState.Dashing:
                    HandleDashingState();
                    break;
                case AttackState.Seeking:
                    HandleSeekingState(owner);
                    break;
            }

            // 视觉效果
            Visuals();
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GoldenWeaverMinionBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<GoldenWeaverMinionBuff>()))
            {
                Projectile.timeLeft = 2; // 保持活跃
            }

            return true;
        }

        private void HandleIdleState(Player owner)
        {
            // 设置空闲位置在玩家头顶
            idlePosition = owner.Center + new Vector2(0, -IDLE_HEIGHT);
            
            // 平滑移动到空闲位置
            Vector2 directionToIdle = idlePosition - Projectile.Center;
            float distanceToIdle = directionToIdle.Length();
            
            if (distanceToIdle > 5f)
            {
                directionToIdle.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToIdle * 3f, 0.1f);
            }
            else
            {
                Projectile.velocity *= 0.8f;
            }

            // 检查是否有敌人在范围内
            if (cooldownTimer <= 0)
            {
                targetNPC = FindNearestEnemy(DETECTION_RANGE);
                if (targetNPC != null)
                {
                    // 切换到冲刺准备状态
                    currentState = AttackState.Charging;
                    stateTimer = 15; // 0.25 秒准备时间
                    startPosition = Projectile.Center;
                    consecutiveDashes = 0; // 重置连续冲刺计数
                }
            }

            // 设置旋转角度（竖直悬浮）
            Projectile.rotation = -MathHelper.PiOver4;
        }

        private void HandleChargingState()
        {
            stateTimer--;
            
            // 在准备期间微微震动，并持续更新目标位置
            Projectile.velocity = Main.rand.NextVector2Circular(1f, 1f);
            
            // 实时更新冲刺方向，追踪移动中的目标
            if (targetNPC != null && targetNPC.active)
            {
                dashDirection = (targetNPC.Center - Projectile.Center);
                dashDirection.Normalize();
                Projectile.rotation = dashDirection.ToRotation() + MathHelper.PiOver4;
            }
            
            if (stateTimer <= 0)
            {
                // 开始冲刺
                currentState = AttackState.Dashing;
                
                // 动态计算冲刺持续时间
                int calculatedDuration = CalculateDashDuration();
                stateTimer = Math.Min(calculatedDuration, MAX_DASH_DURATION);
                
                // 计算最终冲刺方向
                dashDirection = (targetNPC.Center - Projectile.Center);
                dashDirection.Normalize();
                Projectile.velocity = dashDirection * DASH_SPEED;
                startPosition = Projectile.Center;
            }

            // 设置旋转角度指向目标
            if (targetNPC != null)
            {
                Projectile.rotation = (targetNPC.Center - Projectile.Center).ToRotation() + MathHelper.PiOver4;
            }
        }

        private void HandleDashingState()
        {
            stateTimer--;
            
            // 在冲刺过程中持续追踪目标，动态调整方向
            if (targetNPC != null && targetNPC.active && !targetNPC.dontTakeDamage)
            {
                // 计算到目标的预测位置
                Vector2 predictedTargetPos = PredictTargetPosition(targetNPC, DASH_SPEED);
                Vector2 newDashDirection = (predictedTargetPos - Projectile.Center);
                
                // 平滑转向新方向，避免过于灵敏的转向
                float turnSpeed = 0.3f; // 转向速度，值越大转向越快
                newDashDirection.Normalize();
                dashDirection.Normalize();
                dashDirection = Vector2.Lerp(dashDirection, newDashDirection, turnSpeed);
                dashDirection.Normalize();
                
                // 更新冲刺速度方向
                Projectile.velocity = dashDirection * DASH_SPEED;
                
                // 更新旋转角度
                Projectile.rotation = dashDirection.ToRotation() + MathHelper.PiOver4;
                
                // 检查是否应该继续追击
                float distanceToTarget = Vector2.Distance(Projectile.Center, targetNPC.Center);
                float estimatedTimeToCatch = distanceToTarget / DASH_SPEED;
                
                // 如果剩余时间不足以追上目标且还可以连续冲刺，则延长冲刺
                if (stateTimer < estimatedTimeToCatch * 60 && 
                    consecutiveDashes < MAX_CONSECUTIVE_DASHES &&
                    distanceToTarget > 100f)
                {
                    stateTimer++; // 保持剩余时间不减少
                }
            }
            
            // 检查是否到达最大距离
            float distanceTraveled = Vector2.Distance(startPosition, Projectile.Center);
            bool hitTarget = CheckHitTarget();
            
            // 只有在达到最大距离或时间用完时才结束冲刺
            if (stateTimer <= 0 || distanceTraveled > MAX_DASH_DISTANCE)
            {
                // 完成冲刺后寻找新的目标
                NPC newTarget = FindNearestEnemy(DETECTION_RANGE);
                if (newTarget != null && newTarget != targetNPC)
                {
                    // 有新目标，直接进入寻找状态
                    targetNPC = newTarget;
                    currentState = AttackState.Seeking;
                    stateTimer = 30;
                    cooldownTimer = COOLDOWN_DURATION; // 使用完整冷却时间
                    consecutiveDashes = 0; // 重置连续冲刺计数
                }
                else if (targetNPC != null && targetNPC.active && consecutiveDashes < MAX_CONSECUTIVE_DASHES)
                {
                    // 目标还活着且可以继续追击，发起新一轮冲刺
                    currentState = AttackState.Charging;
                    stateTimer = 10; // 快速充能
                    consecutiveDashes++;
                    startPosition = Projectile.Center;
                }
                else
                {
                    // 没有新目标，返回玩家身边
                    currentState = AttackState.Idle;
                    Projectile.velocity *= 0.3f;
                    consecutiveDashes = 0; // 重置连续冲刺计数
                }
            }
            // 如果击中目标但还没完成冲刺，则继续冲刺
            else if (hitTarget && stateTimer > 10)
            {
                // 击中目标后短暂延迟再寻找新目标
                stateTimer = 10; // 强制剩余时间
                cooldownTimer = COOLDOWN_DURATION;
                consecutiveDashes = 0; // 重置连续冲刺计数
            }

            // 设置旋转角度
            Projectile.rotation = dashDirection.ToRotation() + MathHelper.PiOver4;
        }

        private void HandleSeekingState(Player owner)
        {
            stateTimer--;
            
            if (targetNPC != null && targetNPC.active && !targetNPC.dontTakeDamage)
            {
                // 向新目标移动
                Vector2 seekDirection = (targetNPC.Center - Projectile.Center);
                float seekDistance = seekDirection.Length();
                
                if (seekDistance > 20f)
                {
                    seekDirection.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, seekDirection * SEEK_SPEED, 0.15f);
                    
                    // 如果足够接近，开始新一轮冲刺
                    if (seekDistance < 100f && cooldownTimer <= 0)
                    {
                        currentState = AttackState.Charging;
                        stateTimer = 10;
                        startPosition = Projectile.Center;
                        return;
                    }
                }
                else
                {
                    // 已经很接近目标，开始冲刺
                    currentState = AttackState.Charging;
                    stateTimer = 10;
                    startPosition = Projectile.Center;
                    return;
                }
                
                // 设置旋转角度指向目标
                Projectile.rotation = seekDirection.ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                // 目标失效，返回空闲状态
                currentState = AttackState.Idle;
                Projectile.velocity *= 0.5f;
            }

            // 超时保护
            if (stateTimer <= 0)
            {
                currentState = AttackState.Idle;
                Projectile.velocity *= 0.5f;
            }
        }

        private NPC FindNearestEnemy(float maxDistance)
        {
            NPC nearest = null;
            float nearestDistance = maxDistance;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float distance = Vector2.Distance(npc.Center, Projectile.Center);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = npc;
                    }
                }
            }
            
            return nearest;
        }

        private bool CheckHitTarget()
        {
            // 检查是否与目标发生碰撞
            if (targetNPC != null && targetNPC.active)
            {
                Rectangle projRect = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                Rectangle npcRect = new Rectangle((int)targetNPC.position.X, (int)targetNPC.position.Y, targetNPC.width, targetNPC.height);
                return projRect.Intersects(npcRect);
            }
            return false;
        }

        /// <summary>
        /// 根据目标速度和距离动态计算所需的冲刺持续时间
        /// </summary>
        /// <returns>计算出的冲刺持续时间（帧数）</returns>
        private int CalculateDashDuration()
        {
            if (targetNPC == null || !targetNPC.active)
            {
                return BASE_DASH_DURATION;
            }

            // 计算到目标的距离
            float distanceToTarget = Vector2.Distance(Projectile.Center, targetNPC.Center);
            
            // 估算目标的平均速度
            float targetSpeed = targetNPC.velocity.Length();
            
            // 计算相对速度（我们的速度减去目标速度）
            float relativeSpeed = DASH_SPEED - targetSpeed;
            
            // 如果相对速度太慢，使用最大持续时间
            if (relativeSpeed <= 0)
            {
                return MAX_DASH_DURATION;
            }
            
            // 计算追上目标所需的时间（秒）
            float timeNeeded = distanceToTarget / relativeSpeed;
            
            // 转换为帧数（60 FPS），并添加安全系数 1.5
            int framesNeeded = (int)(timeNeeded * 60 * 1.5f);
            
            // 限制在合理范围内
            return (int)MathHelper.Clamp(framesNeeded, BASE_DASH_DURATION, MAX_DASH_DURATION);
        }

        /// <summary>
        /// 预测目标的未来位置，用于提前瞄准移动中的敌人
        /// </summary>
        /// <param name="target">目标 NPC</param>
        /// <param name="projectileSpeed">弹幕速度</param>
        /// <returns>预测的目标位置</returns>
        private Vector2 PredictTargetPosition(NPC target, float projectileSpeed)
        {
            if (target == null || !target.active)
            {
                return target?.Center ?? Projectile.Center;
            }

            // 计算到目标的向量
            Vector2 toTarget = target.Center - Projectile.Center;
            float distanceToTarget = toTarget.Length();
            
            // 估算到达目标所需的时间
            float timeToImpact = distanceToTarget / projectileSpeed;
            
            // 根据目标当前速度预测未来位置
            Vector2 predictedPosition = target.Center + target.velocity * timeToImpact;
            
            // 如果目标正在高速移动，添加额外的提前量
            if (target.velocity.Length() > 5f)
            {
                float extraLead = target.velocity.Length() * 0.5f;
                Vector2 normalizedVelocity = target.velocity;
                normalizedVelocity.Normalize();
                predictedPosition += normalizedVelocity * extraLead;
            }
            
            return predictedPosition;
        }

        private void Visuals()
        {
            // 添加金色粒子效果
            if (Main.rand.NextBool(6))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.GoldCoin, 0, 0, 100, Color.Yellow, 0.8f);
            }
            
            // 添加光照效果
            Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 0f);
            
            // 冲刺时添加额外效果
            if (currentState == AttackState.Dashing)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                        DustID.FireworksRGB, 
                        -Projectile.velocity.X * 0.2f, 
                        -Projectile.velocity.Y * 0.2f, 
                        100, Color.Gold, 1.2f);
                }
            }
            
            // 寻找目标时的特殊效果
            if (currentState == AttackState.Seeking)
            {
                if (Main.rand.NextBool(4))
                {
                    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                        DustID.GoldFlame, 
                        0, 0, 100, Color.Orange, 1f);
                }
            }
        }
    }
}