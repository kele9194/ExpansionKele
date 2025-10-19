using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles
{
    public class FullMoonProjectile : ModProjectile
    {
        // 弹幕状态枚举：飞向目标 / 返回玩家
        private enum State
        {
            ToTarget,
            ReturnToPlayer
        }

        private State CurrentState = State.ToTarget; // 当前弹幕状态
        private Vector2 TargetPosition;              // 目标位置（发射时确定）
        private bool initialized = false;            // 是否已完成初始化
        private bool hasReachedTarget = false;       // 是否已到达目标点（用于防止重复触发）
        private int maxTravelTime = 180;             // 最大飞行时间（帧数），超时后自动返回
        private Vector2 initialDirection;            // 初始方向（用于保持发射时刻的鼠标方向）

        public override void SetStaticDefaults()
        {
            // 设置尾迹长度为 10 帧，使用尾迹模式 2（平滑渐变）
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            // 基础属性设置
            Projectile.width = 8;                   // 弹幕宽度
            Projectile.height = 8;                  // 弹幕高度
            Projectile.friendly = true;              // 对玩家友好
            Projectile.penetrate = -1;               // 无限穿透
            Projectile.timeLeft = 420;               // 总生存时间为 420 帧（7 秒）
            Projectile.tileCollide = false;          // 不与地形碰撞
            Projectile.ignoreWater = true;           // 忽略水体阻挡
            Projectile.light = 1f;                   // 发光强度
            Projectile.extraUpdates = 0;             // 每帧只更新一次
            Projectile.usesLocalNPCImmunity = true;  // 使用本地无敌帧系统
            Projectile.localNPCHitCooldown = 5;      // 击中 NPC 后冷却 5 帧再可击中下一个
        }

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            if (player != null && !player.dead)
            {
                // 获取玩家当前武器的基础伤害，并应用近战总加成
                float baseMeleeDamage = player.HeldItem.damage;
                float totalMeleeDamage = player.GetTotalDamage(DamageClass.Melee).ApplyTo(baseMeleeDamage);

                // 设置弹幕伤害为玩家近战伤害的 41%
                Projectile.damage = (int)(totalMeleeDamage * 0.82f);
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 初始化阶段：设定目标位置
            if (!initialized)
            {
                if (Projectile.ai[0] != 0 || Projectile.ai[1] != 0)
                {
                    // 如果 ai 参数有效，则使用其作为目标点
                    TargetPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                }
                else
                {
                    // 否则根据发射时刻的鼠标方向设定目标点
                    initialDirection = player.DirectionTo(Main.MouseWorld);
                    TargetPosition = player.Center + initialDirection * 400f;
                }

                initialized = true;
            }

            // 设置透明度
            if (CurrentState == State.ToTarget)
            {
                Projectile.alpha = 0; // 飞行时完全不透明
            }
            else if (CurrentState == State.ReturnToPlayer)
            {
                Projectile.alpha = 127; // 返回时半透明（50% 透明度）
            }

            // 如果弹幕速度非常小（卡住），强制切换为返回状态
            if (Projectile.velocity.LengthSquared() < 10f)
            {
                CurrentState = State.ReturnToPlayer;
            }

            // 如果飞行时间过长，也切换为返回状态
            if (Projectile.timeLeft < maxTravelTime)
            {
                CurrentState = State.ReturnToPlayer;
            }

            // 根据当前状态执行移动逻辑
            if (CurrentState == State.ToTarget)
            {
                MoveToTarget(player);
            }
            else if (CurrentState == State.ReturnToPlayer)
            {
                MoveToPlayer(player);
            }

            // 添加发光效果
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.8f);
            if (Main.rand.NextBool(3))
            {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                DustID.GoldFlame, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, 
                default(Color), 1.2f);
            dust.noGravity = true;
            }
        }

        // 飞向目标点逻辑
        private void MoveToTarget(Player player)
        {
            Vector2 direction = TargetPosition - Projectile.Center;

            if (direction.Length() < 16f)
            {
                // 首次到达目标点，切换为返回状态
                if (!hasReachedTarget)
                {
                    CurrentState = State.ReturnToPlayer;
                    hasReachedTarget = true;
                }
                return;
            }

            direction.Normalize();
            Projectile.velocity = direction * 30f;
            if (Main.rand.NextBool(2))
                {
             Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
            DustID.BlueFlare, -Projectile.velocity.X * 0.5f, -Projectile.velocity.Y * 0.5f, 
            100, default(Color), 1.5f);
            dust.noGravity = true;
            dust.scale = 1.2f;
            }
        }

        // 返回玩家逻辑
        private void MoveToPlayer(Player player)
        {
            Vector2 direction = player.Center - Projectile.Center;

            if (direction.Length() < 4f)
            {
                // 距离足够近时销毁弹幕
                Projectile.Kill();
                return;
            }

            direction.Normalize();
            Projectile.velocity = direction * 30f;

        }

        // 碰撞到地形时不触发反弹或销毁
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        // 击中敌人时可添加特效或音效（目前为空）
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 可选：在此添加击中特效或音效
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            
            modifiers.ArmorPenetration+=15;
        }
    }
}