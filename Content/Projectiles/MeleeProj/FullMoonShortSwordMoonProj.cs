using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;
using System.IO;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class FullMoonShortSwordMoonProj : ModProjectile
    {
        // 弹幕状态枚举
        private enum State
        {
            Straight,        // 前10帧直线飞行
            Tracking,        // 追踪敌人阶段
            PostHit,         // 击中后直线飞行阶段
            ReturnToPlayer   // 返回玩家阶段
        }

        private State CurrentState = State.Straight; // 当前弹幕状态
        private Vector2 InitialVelocity;            // 初始速度
        private int straightTimer = 0;              // 直线飞行计时器
        private const int StraightDuration = 10;    // 直线飞行帧数
        private int postHitTimer = 0;               // 击中后计时器
        private const int PostHitDuration = 30;     // 击中后继续前进的帧数
        private Vector2 postHitDirection;           // 击中后的前进方向
        private int _penetrateCount = 0;         // 是否已击中目标
        private List<int> _hitNPCs = new List<int>();
        public int MaxTimeLeft=360;
        private int originalDamage; // 存储原始伤害值
        private bool damageReduced = false; // 标记伤害是否已减少

        public int MaxPenetrate =1;

        // 添加网络同步标识
        private const int STATE_SYNC_INDEX = 0;      // AI[0] 用于状态同步
        private const int PENETRATE_COUNT_INDEX = 1; // AI[1] 用于穿透计数同步

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;              // 无限穿透
            Projectile.timeLeft = MaxTimeLeft;              // 总生存时间为6秒
            Projectile.tileCollide = true;          // 与地形碰撞
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
            Projectile.extraUpdates = 1;            // 每帧更新两次
            Projectile.usesLocalNPCImmunity = true; // 使用本地无敌帧
            Projectile.localNPCHitCooldown = 30;     // 本地无敌帧为8
            Projectile.netUpdate = true;
            Projectile.netImportant = true;         // 重要的网络弹幕
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 初始化网络同步数据
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.ai[STATE_SYNC_INDEX] = (int)CurrentState;
                Projectile.ai[PENETRATE_COUNT_INDEX] = _penetrateCount;
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            // 从网络数据同步状态
            SyncNetworkData();
            
            // 记录初始速度
            if (InitialVelocity == Vector2.Zero)
            {
                InitialVelocity = Projectile.velocity;
            }
            
            // 记录初始伤害
            if (originalDamage == 0)
            {
                originalDamage = Projectile.damage;
            }
            

            Player player = Main.player[Projectile.owner];
            
            // 执行碰撞检测逻辑（在所有客户端执行）
            HandleCollisionDetection();
            
            // 根据当前状态执行不同的AI逻辑
            switch (CurrentState)
            {
                case State.Straight:
                    StraightAI();
                    break;
                case State.Tracking:
                    TrackingAI(player);
                    break;
                case State.PostHit:
                    PostHitAI();
                    break;
                case State.ReturnToPlayer:
                    ReturnToPlayerAI(player);
                    break;
            }
            
            // 更新网络同步数据
            UpdateNetworkData();
        }
        
        // 网络数据同步方法
        private void SyncNetworkData()
        {
            // 从AI数组读取网络同步的数据
            State newState = (State)(int)Projectile.ai[STATE_SYNC_INDEX];
            int newPenetrateCount = (int)Projectile.ai[PENETRATE_COUNT_INDEX];
            
            // 只有当状态真正改变时才更新（避免不必要的同步）
            if (newState != CurrentState || newPenetrateCount != _penetrateCount)
            {
                CurrentState = newState;
                _penetrateCount = newPenetrateCount;
                
                // 状态改变时的一些特殊处理
                if (CurrentState == State.PostHit && postHitDirection == Vector2.Zero)
                {
                    postHitDirection = Vector2.Normalize(Projectile.velocity);
                    _hitNPCs.Clear();
                }
            }
        }
        
        // 更新网络数据
        private void UpdateNetworkData()
        {
            // 更新AI数组中的同步数据
            Projectile.ai[STATE_SYNC_INDEX] = (int)CurrentState;
            Projectile.ai[PENETRATE_COUNT_INDEX] = _penetrateCount;
        }

        public void HandleCollisionDetection()
        {
            // 移除了Main.myPlayer == Projectile.owner的限制，让所有客户端都能执行碰撞检测
            if(CurrentState == State.Straight || CurrentState == State.Tracking)
            {
                // 检测与NPC的碰撞
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.dontTakeDamage && !npc.friendly && npc.life > 0)
                    {
                        // 检查是否与NPC发生碰撞
                        if (Projectile.getRect().Intersects(npc.getRect()))
                        {
                            // 检查是否已经击中过这个NPC
                            if (!_hitNPCs.Contains(i))
                            {
                                _hitNPCs.Add(i);
                                _penetrateCount++;
                                
                                // 只在拥有者客户端显示调试信息和触发状态变更
                                if (Main.myPlayer == Projectile.owner)
                                {
                                    
                                    if(_penetrateCount >= MaxPenetrate)
                                    {
                                        ChangeState(State.PostHit);
                                    }
                                    
                                    // 触发网络同步
                                    Projectile.netUpdate = true;
                                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 前10帧直线飞行AI
        public void StraightAI()
        {
            // 保持初始方向飞行
            Projectile.velocity = InitialVelocity;
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 检查是否达到最大穿透次数
            if (_penetrateCount >= MaxPenetrate)
            {
                ChangeState(State.PostHit);
                return;
            }
            
            // 增加计时器
            straightTimer++;
            
            // 时间到了就进入追踪阶段
            if (straightTimer >= StraightDuration)
            {
                ChangeState(State.Tracking);
            }
        }

        // 追踪敌人阶段AI
        public void TrackingAI(Player player)
        {
            // 使用HomingProjectileHelper进行追踪
            ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 12f, 400f, 10f);
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            if (_penetrateCount >= MaxPenetrate)
            {
                ChangeState(State.PostHit);
                return;
            }
        }

        // 击中后直线飞行阶段AI
        public void PostHitAI()
        {
            // 沿着击中时的方向继续飞行
            Projectile.velocity = postHitDirection * 8f;
            
            // 增加计时器
            postHitTimer++;
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 在这个阶段无限穿透
            Projectile.penetrate = -1;
            
            // 时间到了就进入返回阶段
            if (postHitTimer >= PostHitDuration)
            {
                ChangeState(State.ReturnToPlayer);
            }
        }

        // 返回玩家阶段AI
        public void ReturnToPlayerAI(Player player)
        {
            // 当处于返回阶段且伤害尚未减少时，将伤害设置为原来的66%
            if (!damageReduced) 
            {
                Projectile.damage = (int)(Projectile.damage * 0.66f);
                damageReduced = true;
            }
            
            Vector2 direction = player.Center - Projectile.Center;
            
            // 如果距离玩家很近则销毁弹幕
            if (direction.Length() < 20f)
            {
                Projectile.Kill();
                return;
            }

            direction.Normalize();
            Projectile.velocity = direction * 12f; // 返回速度
            
            // 返回阶段无限穿透
            Projectile.penetrate = -1;
            Projectile.tileCollide = false; // 返回阶段忽略地形碰撞
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        // 统一的状态变更方法
        private void ChangeState(State newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                
                // 状态特定的初始化
                switch (newState)
                {
                    case State.PostHit:
                        postHitDirection = Vector2.Normalize(Projectile.velocity);
                        _hitNPCs.Clear();
                        Projectile.penetrate = -1;
                        break;
                    case State.ReturnToPlayer:
                        Projectile.tileCollide = false;
                        break;
                }
                
                // 触发网络同步
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.netUpdate = true;
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
        }

        // 碰撞到地形时的处理
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 在追踪阶段碰到地形后进入返回阶段
            if (CurrentState == State.Tracking)
            {
                ChangeState(State.ReturnToPlayer);
                return false; // 不销毁弹幕
            }
            
            // 在击中后直线飞行阶段碰到地形也进入返回阶段
            if (CurrentState == State.PostHit)
            {
                ChangeState(State.ReturnToPlayer);
                return false;
            }
            
            return base.OnTileCollide(oldVelocity);
        }
        
        // 网络接收处理
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            // 接收额外的AI数据（如果需要）
            CurrentState = (State)reader.ReadInt32();
            _penetrateCount = reader.ReadInt32();
        }
        
        public override void SendExtraAI(BinaryWriter writer)
        {
            // 发送额外的AI数据（如果需要）
            writer.Write((int)CurrentState);
            writer.Write(_penetrateCount);
        }
    }
}