using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace ExpansionKele.Content.Projectiles
{
    /// <summary>
    /// 望月召唤物主控弹幕 - 占用召唤栏并控制其他月亮
    /// </summary>
    public class FullMoonMinionController : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月守护控制器");
            
            // 设置为仆从类型
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.friendly = true; // 友善弹幕
            Projectile.minion = true; // 标记为仆从
            Projectile.minionSlots = 2f; // 占用2个召唤栏
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 18000; // 存在时间
            Projectile.ignoreWater = true; // 忽略水
            Projectile.usesLocalNPCImmunity = false; // 不使用本地NPC无敌帧
            Projectile.hide = true; // 隐藏弹幕
        }

        public override bool? CanCutTiles()
        {
            return false; // 不能切割草
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // 检查是否应该继续存在
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<Content.Buff.FullMoonMinionBuff>());
                return;
            }
            
            // 检查玩家是否拥有对应的buff
            if (player.HasBuff(ModContent.BuffType<Content.Buff.FullMoonMinionBuff>()))
            {
                Projectile.timeLeft = 18000; // 重置存在时间
            }
            else
            {
                // 如果没有buff，销毁所有相关弹幕
                DestroyAllMoons();
                return;
            }
            
            // 保持在玩家中心
            Projectile.Center = player.Center;
            Projectile.velocity = Vector2.Zero; // 速度设为0，确保不移动
            
            // 确保6个月亮存在
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FullMoonMinion>()] <= 0)
            {
                // 如果月亮不存在，生成6个月亮，使用存储在ai[0]中的距离层级
                CreateMoons((int)Projectile.ai[0]);
            }
        }
        
        // 创建月亮
        private void CreateMoons(int distanceLevel)
        {
            Player player = Main.player[Projectile.owner];
            const int moonCount = 6;
            const float baseDistance = 80f;
            float distance = baseDistance + (distanceLevel * baseDistance);
            
            for (int i = 0; i < moonCount; i++)
            {
                // 计算初始角度
                float angle = MathHelper.TwoPi / moonCount * i;
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * distance;
                
                // 生成弹幕并设置AI参数 (ai[0]为索引, ai[1]为距离层级)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, 
                    ModContent.ProjectileType<FullMoonMinion>(), Projectile.damage, Projectile.knockBack, Projectile.owner, i, distanceLevel);
            }
        }
        
        // 销毁所有月亮
        private void DestroyAllMoons()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<FullMoonMinion>() && proj.owner == Projectile.owner)
                {
                    proj.Kill();
                }
            }
        }
        
        // 更新月亮距离到目标距离
        public void UpdateMoonDistanceToTarget(int targetDistanceLevel)
        {
            // 保存目标距离层级到主控弹幕的ai[0]中
            Projectile.ai[0] = targetDistanceLevel;
            
            // 先销毁现有月亮
            DestroyAllMoons();
            
            // 创建新月亮
            CreateMoons(targetDistanceLevel);
        }
        
        // 当主控弹幕被销毁时，也销毁所有月亮
        public override void OnKill(int timeLeft)
        {
            DestroyAllMoons();
        }
    }
}