using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using ExpansionKele.Content.Buff;

namespace ExpansionKele.Content.Projectiles.SummonProj
{
    /// <summary>
    /// SpaceFunnel浮游炮 - 飞行的浮游炮单位
    /// 会定期发射可无限穿透的绿色光束
    /// </summary>
    public class SpaceFunnelMinion : ModProjectile
    {
        private const int SHOOT_COOLDOWN = 33; // 发射间隔
        private const float DETECTION_RANGE = 900f; // 目标检测范围
        private const float MAX_ATTACK_RANGE = 840f; // 最大攻击距离（新增）
        private const float MIN_ATTACK_RANGE = 0f;  // 最小攻击距离（新增）
        private const float MOVEMENT_SPEED = 8f; // 移动速度
        private const float IDLE_DISTANCE = 100f; // 闲置距离


        private const float Optimal_Distance = 125;
        
        // 添加目标跟踪字段
        private NPC currentTarget = null;
        private int targetSearchTimer = 0;
        private const int TARGET_SEARCH_INTERVAL = 30; 
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("空间浮游炮");
            
            // 设置动画帧数,默认为1
            Main.projFrames[Projectile.type] = 1;
            
            // 设置为仆从类型
            Main.projPet[Projectile.type] = true;
            
            // 设置弹幕ID集合
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // 可以被牺牲
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // 对邪教徒有抗性
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // 支持右键锁定目标
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.friendly = true; // 友善弹幕
            Projectile.minion = true; // 标记为仆从
            Projectile.DamageType = DamageClass.Summon; // 明确指定伤害类型
            Projectile.minionSlots = 1f; // 每个浮游炮占用1个召唤栏
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 18000; // 存在时间
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
            return false; // 浮游炮主要通过光束攻击，关闭接触伤害
        }

        // 主要AI方法
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 活跃检查 - 确保玩家存活且拥有对应buff
            if (!CheckActive(owner))
            {
                return;
            }

            // 更新计时器
            Projectile.ai[0]++;
            
            // 通用行为 - 处理基本的位置和移动逻辑
            GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
            
            // 寻找目标 - 搜索附近的敌人
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            
            // 移动逻辑 - 根据是否有目标来决定移动方式
            Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
            
            // 攻击逻辑 - 定期发射光束
            AttackLogic(foundTarget, targetCenter);
            
            // 视觉效果 - 处理动画和光照
            Visuals();
        }

        // 活跃检查 - 确保召唤物正常工作
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<SpaceFunnelMinionBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<SpaceFunnelMinionBuff>()))
            {
                Projectile.timeLeft = 2; // 保持活跃
            }

            return true;
        }

        // 通用行为 - 处理基本的闲置位置计算
        private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
        {
            // 基础闲置位置设置 - 围绕玩家形成阵型
            Vector2 idlePosition = owner.Center;
            
            // 根据minionPos创建圆形阵型
            float angleOffset = Projectile.minionPos * MathHelper.TwoPi / 6f; // 6个位置均匀分布
            float radius = 120f + Projectile.minionPos * 20f; // 半径随位置递增
            
            Vector2 formationOffset = angleOffset.ToRotationVector2() * radius;
            idlePosition += formationOffset;

            // 计算到闲置位置的向量和距离
            vectorToIdlePosition = idlePosition - Projectile.Center;
            distanceToIdlePosition = vectorToIdlePosition.Length();

            // 远距离传送机制
            if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
            {
                Projectile.position = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            // 处理与其他召唤物的重叠
            float overlapVelocity = 0.05f;
            foreach (var other in Main.ActiveProjectiles)
            {
                if (other.whoAmI != Projectile.whoAmI && 
                    other.owner == Projectile.owner && 
                    other.type == Projectile.type &&
                    Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X)
                        Projectile.velocity.X -= overlapVelocity;
                    else
                        Projectile.velocity.X += overlapVelocity;

                    if (Projectile.position.Y < other.position.Y)
                        Projectile.velocity.Y -= overlapVelocity;
                    else
                        Projectile.velocity.Y += overlapVelocity;
                }
            }
        }

        // 寻找目标 - 实现敌人的搜索和锁定逻辑
                // 寻找目标 - 实现敌人的搜索和锁定逻辑
                // 寻找目标 - 实现敌人的搜索和锁定逻辑
                // 寻找目标 - 实现敌人的搜索和锁定逻辑
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = MAX_ATTACK_RANGE; // 修改为最大攻击距离
            targetCenter = Projectile.position;
            foundTarget = false;

            // 增加目标搜索计时器
            targetSearchTimer++;
            
            // 定期重新搜索目标或当前目标无效时
            if (targetSearchTimer >= TARGET_SEARCH_INTERVAL || currentTarget == null || !currentTarget.active || !currentTarget.CanBeChasedBy())
            {
                currentTarget = null;
                targetSearchTimer = 0;
                
                // 处理玩家手动锁定的目标
                if (owner.HasMinionAttackTargetNPC)
                {
                    NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    
                    // 检查是否在合理攻击范围内
                    if (between < MAX_ATTACK_RANGE && between > MIN_ATTACK_RANGE && npc.CanBeChasedBy())
                    {
                        currentTarget = npc;
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }

                // 自动搜索附近敌人 - 添加合理的距离限制
                if (!foundTarget)
                {
                    foreach (var npc in Main.ActiveNPCs)
                    {
                        if (npc.CanBeChasedBy())
                        {
                            float between = Vector2.Distance(npc.Center, Projectile.Center);
                            
                            // 添加距离限制：必须在合理攻击范围内
                            if (between > MIN_ATTACK_RANGE && between < MAX_ATTACK_RANGE)
                            {
                                bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                                bool inRange = between < distanceFromTarget;

                                // 只需要在范围内且是最近的目标即可
                                if (((closest && inRange) || !foundTarget))
                                {
                                    currentTarget = npc;
                                    distanceFromTarget = between;
                                    targetCenter = npc.Center;
                                    foundTarget = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // 使用已锁定的目标，但要检查距离是否仍然合理
                if (currentTarget != null && currentTarget.active)
                {
                    float between = Vector2.Distance(currentTarget.Center, Projectile.Center);
                    
                    // 如果目标超出攻击范围，取消锁定
                    if (between > MAX_ATTACK_RANGE || between < MIN_ATTACK_RANGE)
                    {
                        currentTarget = null;
                        foundTarget = false;
                    }
                    else
                    {
                        distanceFromTarget = between;
                        targetCenter = currentTarget.Center;
                        foundTarget = true;
                    }
                }
            }

            // 根据是否有目标设置友善状态
            Projectile.friendly = foundTarget;
        }

                // 移动逻辑 - 实现智能的移动和攻击行为
        private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            float speed = MOVEMENT_SPEED;
            float inertia = 15f;

            if (foundTarget)
            {
                // 有目标时：保持攻击距离
                Vector2 direction = targetCenter - Projectile.Center;
                float optimalDistance = Optimal_Distance; // 调整最佳攻击距离到125像素
                
                if (distanceFromTarget > optimalDistance + 50f)
                {
                    // 距离太远，靠近目标
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
                else if (distanceFromTarget < optimalDistance - 50f)
                {
                    // 距离太近，远离目标
                    direction.Normalize();
                    direction *= -speed * 0.5f; // 较慢的后退速度
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
                else
                {
                    // 在最佳距离内，小幅调整位置
                    direction = direction.RotatedBy(MathHelper.PiOver2) * 0.3f; // 侧向移动
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
            }
            else
            {
                // 无目标时：返回闲置位置
                if (distanceToIdlePosition > 20f)
                {
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                }
                else if (Projectile.velocity == Vector2.Zero)
                {
                    // 静止时给一点微小的速度防止卡住
                    Projectile.velocity.X = -0.1f;
                    Projectile.velocity.Y = -0.05f;
                }
            }
        }

        // 攻击逻辑 - 定期发射光束
        private void AttackLogic(bool foundTarget, Vector2 targetCenter)
        {
            if (foundTarget && Projectile.ai[0] >= SHOOT_COOLDOWN)
            {
                // 重置计时器
                Projectile.ai[0] = 0;
                
                // 计算发射方向
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= 12f; // 光束速度
                
                // 创建光束弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    direction,
                    ModContent.ProjectileType<SpaceFunnelBeam>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
                
                // 播放音效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.3f }, Projectile.Center);
            }
        }

        // 视觉效果 - 处理动画、旋转和光照
                // 视觉效果 - 处理动画、旋转和光照
                // 视觉效果 - 处理动画、旋转和光照
        private void Visuals()
        {
            // 有目标时始终朝向目标，无论是否即将射击
            if (currentTarget != null && currentTarget.active)
            {
                Vector2 directionToTarget = currentTarget.Center - Projectile.Center;
                // 头部图片向右，所以需要调整角度
                Projectile.rotation = directionToTarget.ToRotation();
            }
            else
            {
                // 没有目标时根据移动方向旋转
                if (Projectile.velocity != Vector2.Zero)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                }
            }

            // 帧动画
            int frameSpeed = 8;
            Projectile.frameCounter++;
            
            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            // 发光效果 - 绿色光源
            Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 0.5f);
            
            // 粒子效果
            if (Main.rand.NextBool(6))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GreenTorch, 0, 0, 100, default, 1.2f).noGravity = true;
            }
        }
    }
}