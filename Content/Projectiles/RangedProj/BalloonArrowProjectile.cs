using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class BalloonArrowProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.arrow = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates=1;
        }

        public override void AI()
        {
            // 添加箭矢的旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;//箭矢朝下
            Projectile.velocity.Y += 0.02f;
            
            // 添加尾部粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 当击中敌人时生成气球牵引投射物
            SpawnBalloonProjectile(target);
            
            // 减少水平速度，使击退效果更明显地表现为向上
            target.velocity.X *= 0.5f;
        }

        public override void OnKill(int timeLeft)
        {
            // 击中物块或时间结束时也生成粒子效果
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }

        private void SpawnBalloonProjectile(NPC target)
        {
            // 生成一个气球牵引投射物来将敌人拉起
            Projectile balloonProjectile = Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<BalloonTetherProjectile>(),
                0,
                0,
                Projectile.owner
            );
            
            // 将目标与气球关联
            if (balloonProjectile.ModProjectile is BalloonTetherProjectile balloonModProjectile)
            {
                balloonModProjectile.attachedNPC = target.whoAmI;
            }
        }
    }

    // ... existing code ...
    public class BalloonTetherProjectile : ModProjectile
    {
        public int attachedNPC = -1; // 关联的敌人ID
        public float balloonForce = 0.4f; // 气球的基准上升力
        public int timer = 0; // 计时器，用于控制持续时间
        public Vector2 offset = new Vector2(0, -40); // 气球相对于NPC的位置偏移

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1; // 自定义AI
            Projectile.damage = 0;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 90; // 存在时间 (3秒)
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            timer++;
            
            // 如果没有关联敌人或者敌人已经死亡，则移除气球
            if (attachedNPC == -1 || attachedNPC >= Main.maxNPCs || !Main.npc[attachedNPC].active)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[attachedNPC];
            
            // 更新气球位置，保持在NPC上方
            Projectile.Center = target.Center + offset;
            
            // 根据目标碰撞体积计算实际施加的力
            // 公式：基础力 * 1000 / 碰撞体积
            float actualForce = balloonForce * 1000f / (target.width * target.height);
            
            // 只给敌人施加向上的力，气球只是视觉效果跟随敌人
            target.velocity.Y -= actualForce;
            
            // 限制最大的上升速度
            float maxSpeed = -6f;
            if (target.velocity.Y < maxSpeed)
            {
                target.velocity.Y = maxSpeed;
            }
            
            // 添加红色粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Terraria.ID.DustID.RedTorch, 0f, 0f, 100, default, 1f);
            }
            
            // 调整偏移量，使气球逐渐上升到NPC正上方
            if (offset.Y < -30)
            {
                offset.Y += 2f;
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // 自定义绘制，如果需要的话
            return true;
        }
    }
// ... existing code ...
}