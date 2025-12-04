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
    public class ElectricBallProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            
            // 受重力影响
            Projectile.aiStyle = -1; // 不使用默认AI
        }

        // ... existing code ...
        public override void AI()
        {
            // 添加发光粒子效果
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // 添加光照效果
            Lighting.AddLight(Projectile.Center, 0f, 0.3f, 0.8f);

            // 应用重力
            Projectile.velocity.Y += 0.1f;
            
            // 设置旋转方向跟随运动方向
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            // 发射后30帧触发电流释放
            if (Projectile.timeLeft <= 300 - 30)
            {
                ReleaseElectricity();
            }
        }

        private void ReleaseElectricity()
        {
            const int maxTargets = 3;
            const float range = 600f;
            
            List<NPC> targets = new List<NPC>();
            
            // 查找附近的敌人
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance <= range)
                    {
                        targets.Add(npc);
                    }
                }
            }
            
            // 随机选择最多3个目标
            while (targets.Count > maxTargets)
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
                        beamsPerTarget = 8;
                        break;
                    case 2:
                        beamsPerTarget = 5;
                        break;
                    case 3:
                        beamsPerTarget = 4;
                        break;
                }
                
                // 向选中的目标发射电流
                foreach (NPC target in targets)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    float distance = direction.Length();
                    direction.Normalize();
                    
                    for (int i = 0; i < beamsPerTarget; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                                                target.Center, Vector2.Zero, 
                                                ModContent.ProjectileType<ElectricBallistaShockProjectile>(), 
                                                Projectile.damage, 
                                                0f, 
                                                Projectile.owner);
                        // 创建曲折的闪电路径
                        Vector2 startPos = Projectile.Center;
                        Vector2 endPos = target.Center;
                        
                        // 创建带有多段弯曲的闪电路径
                        int segments = Main.rand.Next(3, 7); // 随机分段数
                        float segmentLength = distance / segments;
                        
                        // 创建闪电路径点
                        List<Vector2> lightningPoints = new List<Vector2>();
                        lightningPoints.Add(startPos);
                        
                        Vector2 currentPos = startPos;
                        Vector2 targetDirection = endPos - startPos;
                        targetDirection.Normalize();
                        
                        for (int j = 1; j < segments; j++)
                        {
                            // 在每段中间加入随机偏移，形成曲折效果
                            Vector2 straightPoint = startPos + targetDirection * (segmentLength * j);
                            Vector2 offset = new Vector2(
                                Main.rand.NextFloat(-segmentLength/2, segmentLength/2),
                                Main.rand.NextFloat(-segmentLength/2, segmentLength/2)
                            );
                            
                            // 确保偏移不会太偏离原方向
                            float progress = (float)j / segments;
                            float maxOffset = segmentLength * 0.7f * (1 - Math.Abs(progress - 0.5f) * 2);
                            offset = Vector2.Clamp(offset, 
                                new Vector2(-maxOffset, -maxOffset), 
                                new Vector2(maxOffset, maxOffset));
                            
                            lightningPoints.Add(straightPoint + offset);
                        }
                        
                        lightningPoints.Add(endPos);
                        
                        // 绘制闪电路径上的粒子效果
                        for (int k = 0; k < lightningPoints.Count - 1; k++)
                        {
                            Vector2 segmentStart = lightningPoints[k];
                            Vector2 segmentEnd = lightningPoints[k + 1];
                            Vector2 segmentDir = segmentEnd - segmentStart;
                            float segmentDist = segmentDir.Length();
                            segmentDir.Normalize();
                            
                            // 在每段上创建更多粒子点以增强视觉效果
                            for (int l = 0; l < (int)(segmentDist / 5); l++)
                            {
                                Vector2 position = segmentStart + segmentDir * (l * 5);
                                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, Vector2.Zero, 100, default, Main.rand.NextFloat(0.8f, 1.5f));
                                dust.noGravity = true; // 电火花不受重力影响
                            }
                        }
                        
                        // 在目标位置生成新的抛射体造成伤害
                        
                    }
                }
            }
            else
            {
                // 没有目标时，向随机方向发射闪电
                const int randomBeams = 12;
                for (int i = 0; i < randomBeams; i++)
                {
                    // 随机方向
                    Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                    Vector2 endPos = Projectile.Center + randomDirection * Main.rand.NextFloat(range * 0.5f, range);
                    
                    float distance = Vector2.Distance(Projectile.Center, endPos);
                    int segments = Main.rand.Next(3, 7);
                    float segmentLength = distance / segments;
                    
                    List<Vector2> lightningPoints = new List<Vector2>();
                    lightningPoints.Add(Projectile.Center);
                    
                    Vector2 direction = endPos - Projectile.Center;
                    direction.Normalize();
                    
                    for (int j = 1; j < segments; j++)
                    {
                        Vector2 straightPoint = Projectile.Center + direction * (segmentLength * j);
                        Vector2 offset = new Vector2(
                            Main.rand.NextFloat(-segmentLength/2, segmentLength/2),
                            Main.rand.NextFloat(-segmentLength/2, segmentLength/2)
                        );
                        
                        float progress = (float)j / segments;
                        float maxOffset = segmentLength * 0.7f * (1 - Math.Abs(progress - 0.5f) * 2);
                        offset = Vector2.Clamp(offset, 
                            new Vector2(-maxOffset, -maxOffset), 
                            new Vector2(maxOffset, maxOffset));
                        
                        lightningPoints.Add(straightPoint + offset);
                    }
                    
                    lightningPoints.Add(endPos);
                    
                    for (int k = 0; k < lightningPoints.Count - 1; k++)
                    {
                        Vector2 segmentStart = lightningPoints[k];
                        Vector2 segmentEnd = lightningPoints[k + 1];
                        Vector2 segmentDir = segmentEnd - segmentStart;
                        float segmentDist = segmentDir.Length();
                        segmentDir.Normalize();
                        
                        for (int l = 0; l < (int)(segmentDist / 5); l++)
                        {
                            Vector2 position = segmentStart + segmentDir * (l * 5);
                            Dust dust = Dust.NewDustPerfect(position, DustID.Electric, Vector2.Zero, 100, default, Main.rand.NextFloat(0.8f, 1.5f));
                            dust.noGravity = true;
                        }
                    }
                }
            }
            
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
            
            // 创建爆炸粒子效果
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            
            // 弹幕完成任务，销毁自己
            Projectile.Kill();
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
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }
        }
    }
}