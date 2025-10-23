using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace ExpansionKele.Content.Projectiles
{
    /// <summary>
    /// 望月召唤物 - 围绕玩家旋转的月亮
    /// </summary>
    public class FullMoonMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("望月守护");
            
            // 设置为仆从类型
            Main.projPet[Projectile.type] = true;
            
            // 设置弹幕ID集合
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // 可以被牺牲
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // 对邪教徒有抗性
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.tileCollide = false; // 不与物块碰撞
            Projectile.friendly = true; // 友善弹幕
            Projectile.minion = true; // 标记为仆从
            Projectile.minionSlots = 0f; // 不占用召唤栏，由主控弹幕占用
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 18000; // 存在时间
            Projectile.ignoreWater = true; // 忽略水
            Projectile.usesLocalNPCImmunity = true; // 使用本地NPC无敌帧
            Projectile.localNPCHitCooldown = 15; // 本地NPC击中冷却
        }

        public override bool? CanCutTiles()
        {
            return false; // 不能切割草
        }

        public override bool MinionContactDamage()
        {
            return true; // 接触伤害
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // 检查是否应该继续存在
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }
            
            // 检查玩家是否拥有对应的buff，没有则销毁
            if (!player.HasBuff(ModContent.BuffType<Content.Buff.FullMoonMinionBuff>()))
            {
                Projectile.Kill();
                return;
            }
            
            // 旋转参数
            const int moonCount = 6; // 6个月亮
            const float baseDistance = 80f; // 基础距离
            const float maxDistance = 640f; // 最大距离
            const float rotationSpeed = 0.05f; // 旋转速度
            
            // 计算当前月亮的索引
            int moonIndex = (int)Projectile.ai[0];
            
            // 计算旋转角度
            float angle = Main.GameUpdateCount * rotationSpeed + (MathHelper.TwoPi / moonCount * moonIndex);
            
            // 计算距离 - 使用ai[1]存储距离层级
            int distanceLevel = (int)Projectile.ai[1];
            float distance = baseDistance + (distanceLevel * baseDistance);
            
            // 确保距离在有效范围内
            if (distance > maxDistance)
            {
                distance = baseDistance; // 超过最大距离时回到基础距离
                Projectile.ai[1] = 0; // 重置距离层级
            }
            
            // 计算目标位置（严格围绕玩家中心旋转）
            Vector2 targetPos = player.Center + angle.ToRotationVector2() * distance;
            
            // 直接设置位置，确保严格围绕玩家旋转
            Projectile.Center = targetPos;
            Projectile.velocity = Vector2.Zero; // 速度设为0，因为我们直接控制位置
            
            // 设置旋转角度
            Projectile.rotation = angle + MathHelper.PiOver2;
            
            // 添加发光效果
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.5f);
            
            // 添加粒子效果
            if (Main.rand.NextBool(10))
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.BlueTorch, 0, 0, 100, default, 1f).noGravity = true;
            }
        }
    }
}