using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace ExpansionKele.Content.Projectiles.SummonProj
{
    /// <summary>
    /// 望月召唤物 - 围绕玩家旋转的月亮
    /// 具有标准的召唤物AI行为
    /// </summary>
    public class FullMoonMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月守护");
            
            // 设置动画帧数
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
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.friendly = true; // 友善弹幕
            Projectile.minion = true; // 标记为仆从
            Projectile.DamageType = DamageClass.Summon; // 明确指定伤害类型
            Projectile.minionSlots = 1f; // 每个月亮占用1个召唤栏
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 18000; // 存在时间
            Projectile.ignoreWater = true; // 忽略水
            Projectile.usesLocalNPCImmunity = true; // 使用本地NPC无敌帧
            Projectile.localNPCHitCooldown = 30; // 本地NPC击中冷却（局部无敌帧）
        }

        public override bool? CanCutTiles()
        {
            return false; // 不能切割草
        }

        public override bool MinionContactDamage()
        {
            return true; // 接触伤害
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

            // 通用行为 - 处理基本的位置和移动逻辑
            GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
            
            // 寻找目标 - 搜索附近的敌人
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            
            // 移动逻辑 - 根据是否有目标来决定移动方式
            Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
            
            // 视觉效果 - 处理动画和光照
            Visuals();
        }

        // 活跃检查 - 确保召唤物正常工作
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Content.Buff.FullMoonMinionBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Content.Buff.FullMoonMinionBuff>()))
            {
                Projectile.timeLeft = 2; // 保持活跃
            }

            return true;
        }

        // 通用行为 - 处理基本的闲置位置计算
        private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
        {
            // 基础闲置位置设置
            Vector2 idlePosition = owner.Center;
            idlePosition.Y -= 48f; // 向上偏移

            // 根据召唤位置偏移，避免重叠
            float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -owner.direction;
            idlePosition.X += minionPositionOffsetX;

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
            float overlapVelocity = 0.04f;
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
        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = 700f;
            targetCenter = Projectile.position;
            foundTarget = false;

            // 处理玩家手动锁定的目标
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                
                if (between < 2000f) // 合理的攻击距离
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            // 自动搜索附近敌人
            if (!foundTarget)
            {
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        bool closeThroughWall = between < 100f; // 近距离穿墙攻击

                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }

            // 根据是否有目标设置友善状态
            Projectile.friendly = foundTarget;
        }

               // 移动逻辑 - 实现智能的移动和攻击行为
        private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            float speed = 24f; // 基础移动速度提升至3倍(原8f)
            float inertia = 10f; // 进一步降低惯性使转向更灵敏

            if (foundTarget)
            {
                // 有目标时：攻击模式
                if (distanceFromTarget > 40f) // 避免过于接近目标
                {
                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
            }
            else
            {
                // 无目标时：返回玩家身边并闲置
                if (distanceToIdlePosition > 600f)
                {
                    // 距离太远时加速 - 返回速度3倍(原12f)
                    speed = 36f;
                    inertia = 20f;
                }
                else
                {
                    // 靠近时保持较高速度用于巡逻 - 巡逻速度4倍(原4f)
                    speed = 16f;
                    inertia = 15f;
                }

                if (distanceToIdlePosition > 20f)
                {
                    // 移动到闲置位置
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                }
                else if (Projectile.velocity == Vector2.Zero)
                {
                    // 静止时给一点微小的速度防止卡住
                    Projectile.velocity.X = -0.15f;
                    Projectile.velocity.Y = -0.05f;
                }
            }
        }

        // 视觉效果 - 处理动画、旋转和光照
        private void Visuals()
        {
            // 根据移动方向倾斜
            Projectile.rotation = Projectile.velocity.X * 0.05f;

            // 帧动画
            int frameSpeed = 5;
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

            // 发光效果
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.78f);
            
            // 粒子效果
            if (Main.rand.NextBool(8))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.BlueTorch, 0, 0, 100, default, 1f).noGravity = true;
            }
        }
    }
}