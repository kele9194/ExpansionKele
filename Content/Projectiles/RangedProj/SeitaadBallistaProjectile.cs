using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class SeitaadBallistaProjectile : ModProjectile
    {
        // 基本属性常量
        private const int PROJECTILE_WIDTH = 16;
        private const int PROJECTILE_HEIGHT = 16;
        private const int PENETRATE_COUNT = 1;
        private const int TIME_LEFT = 300;
        private const int EXTRA_UPDATES = 1;
        
        // 粒子效果常量
        private const int DUST_TYPE = DustID.Electric;
        private const int DUST_ALPHA = 100;
        private const float DUST_SCALE_NORMAL = 1f;
        private const float DUST_SCALE_LARGE = 1.5f;
        private const float DUST_SCALE_EXTRA = 1.2f;
        private const int PARTICLE_COUNT_ON_HIT = 10;
        private const int PARTICLE_COUNT_ON_EXPLODE = 20;
        
        // 光照效果常量
        private const float LIGHT_RED = 0f;
        private const float LIGHT_GREEN = 0.3f;
        private const float LIGHT_BLUE = 0.8f;
        
        // 物理效果常量
        private const float GRAVITY_FORCE = 0.1f;
        private const int ELECTRICITY_TRIGGER_TIME = 30;
        
        // 电流释放相关常量
        private const int MAX_TARGETS = 3;
        private const float TARGET_RANGE = 600f;
        private const int EXTRA_BEAMS_PER_TARGET = 4;
        private const float RANDOM_TARGET_RADIUS = 100f;
        private const int RANDOM_BEAMS_COUNT = 12;
        private const float MIN_RANDOM_RANGE_FACTOR = 0.5f;
        private const float MAX_RANDOM_RANGE_FACTOR = 1.0f;
        
        // 闪电束相关常量
        private const int MIN_SEGMENTS = 3;
        private const int MAX_SEGMENTS = 7;
        private const float DAMAGE_PERCENTAGE = 0.4f;
        private const float SEGMENT_OFFSET_FACTOR = 0.5f;
        private const float SEGMENT_MAX_OFFSET_FACTOR = 0.7f;
        private const float ENEMY_DETECTION_RANGE = 20f;
        private const int BEAMS_TARGET_1 = 6;
        private const int BEAMS_TARGET_2 = 4;
        private const int BEAMS_TARGET_3 = 3;
        private const float DUST_SPACING = 5f;
        private const float MIN_DUST_SCALE = 0.6f;
        private const float MAX_DUST_SCALE = 1.1f;
        
        public override void SetDefaults()
        {
            Projectile.width = PROJECTILE_WIDTH;
            Projectile.height = PROJECTILE_HEIGHT;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = PENETRATE_COUNT;
            Projectile.timeLeft = TIME_LEFT;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = EXTRA_UPDATES;
            
            // 受重力影响
            Projectile.aiStyle = -1; // 不使用默认AI
        }

        public override void AI()
        {
            // 添加发光粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DUST_TYPE, 0f, 0f, DUST_ALPHA, default, DUST_SCALE_NORMAL);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // 添加光照效果
            Lighting.AddLight(Projectile.Center, LIGHT_RED, LIGHT_GREEN, LIGHT_BLUE);

            // 应用重力
            Projectile.velocity.Y += GRAVITY_FORCE;
            
            // 设置旋转方向跟随运动方向
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            if (Projectile.ai[0] == 0)
            {
                // 获取弹幕所属玩家的暴击率
                Player player = Main.player[Projectile.owner];
                int critChance = 0;
                if (Projectile.DamageType == DamageClass.Melee) {
                    critChance = player.GetWeaponCrit(player.HeldItem);
                } else if (Projectile.DamageType == DamageClass.Ranged) {
                    critChance = player.GetWeaponCrit(player.HeldItem);
                } else if (Projectile.DamageType == DamageClass.Magic) {
                    critChance = player.GetWeaponCrit(player.HeldItem);
                } else if (Projectile.DamageType == DamageClass.Throwing) {
                    critChance = player.GetWeaponCrit(player.HeldItem);
                }
                
                // 将暴击率转换为伤害加成 (例如: 10%暴击率提供10%伤害加成)
                float damageMultiplier = 1f + (critChance / 100f);
                Projectile.damage = (int)(Projectile.damage * damageMultiplier);
                
                // 标记已处理过伤害加成
                Projectile.ai[0] = 1;
            }
            
            // 发射后30帧触发电流释放
            if (Projectile.timeLeft <= TIME_LEFT - ELECTRICITY_TRIGGER_TIME)
            {
                ReleaseElectricity();
            }
        }

        
        // ... existing code ...
        // ... existing code ...
        private void ReleaseElectricity()
        {
            List<NPC> targets = new List<NPC>();
            
            // 查找附近的敌人
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance <= TARGET_RANGE)
                    {
                        targets.Add(npc);
                    }
                }
            }
            
            // 随机选择最多3个目标
            while (targets.Count > MAX_TARGETS)
            {
                targets.RemoveAt(Main.rand.Next(targets.Count));
            }
            
            // 获取弹幕所属玩家的暴击率
            Player player = Main.player[Projectile.owner];
            int critChance = 0;
            if (Projectile.DamageType == DamageClass.Melee) {
                critChance = player.GetWeaponCrit(player.HeldItem);
            } else if (Projectile.DamageType == DamageClass.Ranged) {
                critChance = player.GetWeaponCrit(player.HeldItem);
            } else if (Projectile.DamageType == DamageClass.Magic) {
                critChance = player.GetWeaponCrit(player.HeldItem);
            } else if (Projectile.DamageType == DamageClass.Throwing) {
                critChance = player.GetWeaponCrit(player.HeldItem);
            }
            
            if (targets.Count > 0)
            {
                // 根据目标数量确定每个目标的闪电数量
                int beamsPerTarget = 0;
                switch (targets.Count)
                {
                    case 1:
                        beamsPerTarget = BEAMS_TARGET_1;
                        break;
                    case 2:
                        beamsPerTarget = BEAMS_TARGET_2;
                        break;
                    case 3:
                        beamsPerTarget = BEAMS_TARGET_3;
                        break;
                }
                
                // 向选中的目标发射电流
                foreach (NPC target in targets)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    float distance = direction.Length();
                    direction.Normalize();
                    
                    // 发射指向目标的闪电
                    for (int i = 0; i < beamsPerTarget; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                                                target.Center, Vector2.Zero, 
                                                ModContent.ProjectileType<SeitaadBallistaShockProjectile>(), 
                                                Projectile.damage, 
                                                0f, 
                                                Projectile.owner);
                        CreateLightningBeam(Projectile.Center, target.Center, critChance, direction);
                    }
                    
                    // 发射指向目标周围随机位置的额外闪电
                    for (int i = 0; i < EXTRA_BEAMS_PER_TARGET; i++)
                    {
                        // 在目标周围100像素半径内随机生成位置
                        Vector2 randomOffset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(RANDOM_TARGET_RADIUS);
                        Vector2 randomEndPos = target.Center + randomOffset;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                                                randomEndPos, Vector2.Zero, 
                                                ModContent.ProjectileType<SeitaadBallistaShockProjectile>(), 
                                                Projectile.damage, 
                                                0f, 
                                                Projectile.owner);
                        CreateLightningBeam(Projectile.Center, randomEndPos, critChance, Vector2.Normalize(randomEndPos - Projectile.Center));
                    }
                }
            }
            else
            {
                // 没有目标时，向随机方向发射闪电
                for (int i = 0; i < RANDOM_BEAMS_COUNT; i++)
                {
                    // 随机方向
                    Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                    Vector2 endPos = Projectile.Center + randomDirection * Main.rand.NextFloat(TARGET_RANGE * MIN_RANDOM_RANGE_FACTOR, TARGET_RANGE * MAX_RANDOM_RANGE_FACTOR);
                    CreateLightningBeam(Projectile.Center, endPos, critChance, randomDirection);
                }
            }
            
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
            
            // 创建爆炸粒子效果
            for (int i = 0; i < PARTICLE_COUNT_ON_EXPLODE; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DUST_TYPE, 0f, 0f, DUST_ALPHA, default, DUST_SCALE_LARGE);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            
            // 弹幕完成任务，销毁自己
            Projectile.Kill();
        }
        
        // 创建闪电束的通用方法
        private void CreateLightningBeam(Vector2 start, Vector2 end, int critChance, Vector2 generalDirection)
        {
            // 计算距离和方向
            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0) return; // 避免除零错误
            direction.Normalize();
            
            // 创建带有多段弯曲的闪电路径
            int segments = Main.rand.Next(MIN_SEGMENTS, MAX_SEGMENTS); // 随机分段数
            float segmentLength = distance / segments;
            
            // 创建闪电路径点
            List<Vector2> lightningPoints = new List<Vector2>();
            lightningPoints.Add(start);
            
            Vector2 targetDirection = end - start;
            targetDirection.Normalize();
            
            for (int j = 1; j < segments; j++)
            {
                // 在每段中间加入随机偏移，形成曲折效果
                Vector2 straightPoint = start + targetDirection * (segmentLength * j);
                Vector2 offset = new Vector2(
                    Main.rand.NextFloat(-segmentLength * SEGMENT_OFFSET_FACTOR, segmentLength * SEGMENT_OFFSET_FACTOR),
                    Main.rand.NextFloat(-segmentLength * SEGMENT_OFFSET_FACTOR, segmentLength * SEGMENT_OFFSET_FACTOR)
                );
                
                // 确保偏移不会太偏离原方向
                float progress = (float)j / segments;
                float maxOffset = segmentLength * SEGMENT_MAX_OFFSET_FACTOR * (1 - Math.Abs(progress - 0.5f) * 2);
                offset = Vector2.Clamp(offset, 
                    new Vector2(-maxOffset, -maxOffset), 
                    new Vector2(maxOffset, maxOffset));
                
                lightningPoints.Add(straightPoint + offset);
            }
            
            lightningPoints.Add(end);
            
            // 绘制闪电路径上的粒子效果
            for (int k = 0; k < lightningPoints.Count - 1; k++)
            {
                Vector2 segmentStart = lightningPoints[k];
                Vector2 segmentEnd = lightningPoints[k + 1];
                Vector2 segmentDir = segmentEnd - segmentStart;
                float segmentDist = segmentDir.Length();
                segmentDir.Normalize();
                
                // 在每段上创建更多粒子点以增强视觉效果
                for (int l = 0; l < (int)(segmentDist / DUST_SPACING); l++)
                {
                    Vector2 position = segmentStart + segmentDir * (l * DUST_SPACING);
                    Dust dust = Dust.NewDustPerfect(position, DUST_TYPE, Vector2.Zero, DUST_ALPHA, default, Main.rand.NextFloat(MIN_DUST_SCALE, MAX_DUST_SCALE));
                    dust.noGravity = true; // 电火花不受重力影响
                }
            }
            
            // // 检查是否有敌人在闪电路径附近
            // for (int i = 0; i < Main.maxNPCs; i++)
            // {
            //     NPC npc = Main.npc[i];
            //     if (npc.active && !npc.friendly && !npc.dontTakeDamage)
            //     {
            //         // 检查NPC是否在闪电路径点附近
            //         foreach (Vector2 point in lightningPoints)
            //         {
            //             float distToLightning = Vector2.Distance(npc.Center, point);
            //             if (distToLightning <= ENEMY_DETECTION_RANGE) // 20像素范围内的敌人受到伤害
            //             {
            //                 // 判断是否暴击
            //                 bool isCrit = Main.rand.Next(1, 101) <= critChance;
                            
            //                 // 对目标造成伤害（武器伤害的40%）
            //                 int electricDamage = (int)(Projectile.damage * DAMAGE_PERCENTAGE);
            //                 // 暴击时不再增加伤害，而是让目标速度归零
                            
            //                 NPC.HitInfo hitInfo = new NPC.HitInfo()
            //                 {
            //                     DamageType = DamageClass.Ranged,
            //                     Damage = electricDamage,
            //                     Knockback = 0,
            //                     HitDirection = (int)Math.Sign(generalDirection.X),
            //                     Crit = isCrit
            //                 };
            //                 npc.StrikeNPC(hitInfo);
                            
            //                 // 如果暴击，使目标速度归零
            //                 if (isCrit)
            //                 {
            //                     npc.velocity *= 0;
            //                 }
                            
            //                 break; // 每道闪电只伤害一个敌人
            //             }
            //         }
            //     }
            // }
        }
// ... existing code ...

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return base.OnTileCollide(oldVelocity);
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // 击中时产生电火花效果
            for (int i = 0; i < PARTICLE_COUNT_ON_HIT; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DUST_TYPE, 0f, 0f, DUST_ALPHA, default, DUST_SCALE_EXTRA);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
        }
    }
}