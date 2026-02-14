using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    public class NoMoreRovingProjectile : ModProjectile
    {
        private bool deployed = false; // 标记地雷是否已部署
        private NPC targetToExclude = null; // 要排除的敌人（触发爆炸的那个）
        private int existTime = 0; // 存在时间计数器
        
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("不再漫游地雷");
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            // 增加存在时间
            existTime++;
            float damageMultiplier = 1f + (existTime * 0.0015f);
            Projectile.damage = (int)(Projectile.originalDamage * damageMultiplier);
            
            
            // 添加紫色粒子效果（仅在移动时）
            if (!deployed && Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 100, default(Color), 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            
            // 如果地雷已经减速到几乎停止，则进入部署状态
            if (Projectile.velocity.Length() < 0.1f && !deployed)
            {
                // 进入部署状态
                deployed = true;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false; // 部署后不再与物块碰撞
                
                // 改变地雷的碰撞箱大小，使其更容易被敌人碰到
                Projectile.width = 30;
                Projectile.height = 30;
                Projectile.netUpdate = true; // 同步更新
            }
            else if (!deployed)
            {
                // 减速过程
                Projectile.velocity *= 0.96f;
            }
            
            // 地雷旋转效果
            Projectile.rotation += 0.1f * Projectile.direction;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 与物块碰撞时不销毁，继续减速（仅在未部署时）
            if (!deployed)
            {
                Projectile.velocity = oldVelocity * 0.8f; // 弹性碰撞
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 只有在部署状态下碰到敌人时才引爆
            if (deployed)
            {
                // 记录触发爆炸的敌人，避免其受到重复伤害
                targetToExclude = target;
                
                // 对范围内的敌人造成AOE伤害
                Explode();
            }
        }

        private void Explode()
        {
            // 添加爆炸粒子效果
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 100, default(Color), 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // 计算伤害加成（每帧0.15%）
            
            // 对范围内的敌人造成伤害（排除触发爆炸的那个敌人）
            foreach (NPC npc in Main.npc)
            {
                // 排除触发爆炸的敌人，避免重复伤害
                if (npc != targetToExclude && npc.active && !npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < 144f) // AOE范围
                    {
                        NPC.HitInfo hitInfo = new NPC.HitInfo();
                        hitInfo.Knockback = Projectile.knockBack;
                        hitInfo.Damage = Projectile.damage;
                        hitInfo.HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1;
                        npc.StrikeNPC(hitInfo);
                    }
                }
            }

            // 销毁地雷
            Projectile.Kill();
        }
        
        public override bool? CanHitNPC(NPC target)
        {
            // 只有在部署状态下才能碰撞敌人并造成伤害
            return deployed ? (bool?)null : false;
        }

        public override bool CanHitPvp(Player target)
        {
            // 只有在部署状态下才能造成PvP伤害
            return deployed;
        }
    }
}