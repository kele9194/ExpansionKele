using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using System.Collections.Generic;

namespace ExpansionKele.Content.Projectiles
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
        // Deleted:private bool initialized = false;           // 是否已完成初始化
        private int straightTimer = 0;              // 直线飞行计时器
        private const int StraightDuration = 10;    // 直线飞行帧数
        private int postHitTimer = 0;               // 击中后计时器
        private const int PostHitDuration = 30;     // 击中后继续前进的帧数
        private Vector2 postHitDirection;           // 击中后的前进方向
        private int _penetrateCount = 0;         // 是否已击中目标
        private List<int> _hitNPCs = new List<int>();
        private int _lifeTimer = 0;
        public int MaxTimeLeft=360;
        private int originalDamage; // 存储原始伤害值
        private bool damageReduced = false; // 标记伤害是否已减少

        public int MaxPenetrate =1;

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
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 记录初始速度
            InitialVelocity = Projectile.velocity;
            // 记录初始伤害
            originalDamage = Projectile.damage;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            _lifeTimer=(int)Projectile.ai[0];

            Player player = Main.player[Projectile.owner];
            
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
        }

        // 前10帧直线飞行AI
        private void StraightAI()
        {
            // 保持初始方向飞行
            Projectile.velocity = InitialVelocity;
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // 检查是否达到最大穿透次数
            if (_penetrateCount >= MaxPenetrate)
            {
                CurrentState = State.PostHit;
                postHitDirection = Vector2.Normalize(Projectile.velocity); // 使用当前速度方向
                _hitNPCs.Clear();
                Projectile.penetrate = -1; // 允许无限穿透返回
                return;
            }
            
            // 增加计时器
            straightTimer++;
            
            // 时间到了就进入追踪阶段
            if (straightTimer >= StraightDuration)
            {
                CurrentState = State.Tracking;
            }
        }

        // 追踪敌人阶段AI
        private void TrackingAI(Player player)
        {
            // 使用HomingProjectileHelper进行追踪
            ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 12f, 400f, 10f);
            
            // 设置旋转角度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (_penetrateCount >= MaxPenetrate)
            {
                CurrentState = State.PostHit;
                postHitDirection = Vector2.Normalize(Projectile.velocity); // 使用当前速度方向
                _hitNPCs.Clear();
                Projectile.penetrate = -1; // 允许无限穿透返回
                return;
            }
        }

        // 击中后直线飞行阶段AI
        private void PostHitAI()
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
                CurrentState = State.ReturnToPlayer;
            }
        }

        // 返回玩家阶段AI
        private void ReturnToPlayerAI(Player player)
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

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            _hitNPCs.Add(target.whoAmI);
            _penetrateCount++;
        }


        // 碰撞到地形时的处理
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 在追踪阶段碰到地形后进入返回阶段
            if (CurrentState == State.Tracking)
            {
                CurrentState = State.ReturnToPlayer;
                return false; // 不销毁弹幕
            }
            
            // 在击中后直线飞行阶段碰到地形也进入返回阶段
            if (CurrentState == State.PostHit)
            {
                CurrentState = State.ReturnToPlayer;
                return false;
            }
            
            return base.OnTileCollide(oldVelocity);
        }
    }
}