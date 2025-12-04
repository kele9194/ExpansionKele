using System;
using System.Collections.Generic;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.GenericProj
{


public class EMPPulseProjectile : ModProjectile
    {
        // 添加一个字段用于标记该弹幕是否已经连锁过
        public bool HasChained = false;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // 添加电弧粒子效果
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // 添加光照效果
            Lighting.AddLight(Projectile.Center, 0f, 0.5f, 1f);

            // 设置旋转方向跟随运动方向
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 直接让目标速度变为0
            target.velocity *= 0;

            // 只有当弹幕未连锁过时才尝试连锁
            if (!HasChained)
            {
                ChainToNearbyEnemies(target);
            }
        }

        private void ChainToNearbyEnemies(NPC initialTarget)
        {
            const int maxChainTargets = 3; // 最多连锁3个目标
            const float chainRange = 300f; // 连锁范围
            
            List<NPC> chainedNPCs = new List<NPC>();
            chainedNPCs.Add(initialTarget); // 添加初始目标
            
            NPC currentTarget = initialTarget;
            
            // 进行连锁跳跃
            for (int i = 0; i < maxChainTargets; i++)
            {
                NPC nextTarget = FindNearestValidEnemy(currentTarget, chainedNPCs, chainRange);
                
                if (nextTarget != null)
                {
                    // 发射新的弹幕到下一个目标
                    Vector2 direction = nextTarget.Center - currentTarget.Center;
                    direction.Normalize();
                    
                    // 创建新的弹幕
                    int projIndex = Projectile.NewProjectile(Projectile.GetSource_FromThis(), currentTarget.Center, direction * 8f,
                        ModContent.ProjectileType<EMPPulseProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    
                    // 立即标记新弹幕已连锁，防止继续分裂
                    if (projIndex >= 0 && projIndex < Main.projectile.Length)
                    {
                        Projectile newProj = Main.projectile[projIndex];
                        if (newProj.active && newProj.ModProjectile is EMPPulseProjectile empProj)
                        {
                            empProj.HasChained = true;
                        }
                    }
                    
                    // 在两个NPC之间创建电弧视觉效果
                    float distance = Vector2.Distance(currentTarget.Center, nextTarget.Center);
                    
                    // 创建电弧粒子效果
                    for (int j = 0; j < (int)(distance / 10); j++)
                    {
                        Vector2 position = currentTarget.Center + direction * (j * 10);
                        Dust.NewDustPerfect(position, DustID.Electric, Vector2.Zero, 100, default, 1f);
                    }
                    
                    // 直接让新目标速度变为0
                    nextTarget.velocity *= 0;
                    chainedNPCs.Add(nextTarget);
                    currentTarget = nextTarget;
                }
                else
                {
                    break; // 没有找到更多目标则退出循环
                }
            }
            
            // 标记当前弹幕已连锁
            HasChained = true;
        }

        private NPC FindNearestValidEnemy(NPC sourceNPC, List<NPC> excludeList, float maxRange)
        {
            NPC nearestNPC = null;
            float nearestDistance = maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                
                // 检查NPC是否有效且不在排除列表中
                if (npc.active && !npc.friendly && !excludeList.Contains(npc))
                {
                    float distance = Vector2.Distance(sourceNPC.Center, npc.Center);
                    
                    // 检查是否在范围内且距离最近
                    if (distance <= nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestNPC = npc;
                    }
                }
            }

            return nearestNPC;
        }

        public override void OnKill(int timeLeft)
        {
            // 击中时产生电火花效果
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
            
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item94, Projectile.position);
        }
    }
}