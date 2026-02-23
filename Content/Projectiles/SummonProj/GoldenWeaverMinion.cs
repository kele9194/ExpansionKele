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
        // AI状态枚举
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
        private const float DASH_SPEED = 25f;           // 提升冲刺速度（原15f）
        private const float SEEK_SPEED = 12f;           // 寻找新目标速度
        private const int DASH_DURATION = 45;           // 延长冲刺持续时间（原30）
        private const int COOLDOWN_DURATION = 45;       // 缩短攻击冷却时间（原60）
        private const float MAX_DASH_DISTANCE = 800f;   // 最大冲刺距离

        // 状态变量
        private AttackState currentState = AttackState.Idle;
        private Vector2 idlePosition = Vector2.Zero;    // 空闲位置
        private NPC targetNPC = null;                   // 当前目标
        private Vector2 dashDirection = Vector2.Zero;   // 冲刺方向
        private int stateTimer = 0;                     // 状态计时器
        private int cooldownTimer = 0;                  // 冷却计时器
        private Vector2 startPosition = Vector2.Zero;   // 冲刺起始位置

        public override void SetStaticDefaults()
        {
            // 设置为仆从类型
            Main.projPet[Projectile.type] = true;
            
            // 设置弹幕ID集合
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
            Projectile.usesLocalNPCImmunity = true; // 使用本地NPC无敌帧
            Projectile.localNPCHitCooldown = 20; // 本地NPC击中冷却
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
                    stateTimer = 15; // 0.25秒准备时间
                    startPosition = Projectile.Center;
                }
            }

            // 设置旋转角度（竖直悬浮）
            Projectile.rotation = -MathHelper.PiOver4;
        }

        private void HandleChargingState()
        {
            stateTimer--;
            
            // 在准备期间微微震动
            Projectile.velocity = Main.rand.NextVector2Circular(1f, 1f);
            
            if (stateTimer <= 0)
            {
                // 开始冲刺
                currentState = AttackState.Dashing;
                stateTimer = DASH_DURATION;
                
                // 计算冲刺方向
                dashDirection = (targetNPC.Center - Projectile.Center);
                dashDirection.Normalize();
                Projectile.velocity = dashDirection * DASH_SPEED;
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
            
            // 保持冲刺速度
            Projectile.velocity = dashDirection * DASH_SPEED;
            
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
                }
                else
                {
                    // 没有新目标，返回玩家身边
                    currentState = AttackState.Idle;
                    Projectile.velocity *= 0.3f;
                }
            }
            // 如果击中目标但还没完成冲刺，则继续冲刺
            else if (hitTarget && stateTimer > 10)
            {
                // 击中目标后短暂延迟再寻找新目标
                stateTimer = 10; // 强制剩余时间
                cooldownTimer = COOLDOWN_DURATION;
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