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
    public class EMPBallProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            
            // 受重力影响，但比普通抛射体轻
            Projectile.aiStyle = -1; // 不使用默认AI
        }

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
            Lighting.AddLight(Projectile.Center, 0f, 0.5f, 0.2f);

            // 应用微重力（比重力影响小）
            Projectile.velocity.Y += 0.05f;
            
            // 设置旋转方向跟随运动方向
            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            // 发射后30帧触发EMP效果
            if (Projectile.timeLeft <= 300 - 30)
            {
                ReleaseEMP();
            }
        }

        private void ReleaseEMP()
        {
            const float range = 256f;
            
            bool foundTargets = false;
            
            // 查找附近的敌人
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance <= range)
                    {
                        foundTargets = true;
                        break;
                    }
                }
            }
            
            // 如果找到目标或时间足够长（强制触发）
            if (foundTargets || Projectile.timeLeft <= 10)
            {
                // 创建EMP波
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), 
                    Projectile.Center, 
                    Vector2.Zero, 
                    ModContent.ProjectileType<EMPWave>(), 
                    0, // EMP波不造成伤害
                    0, 
                    Projectile.owner
                );
                
                // 播放音效
                SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.5f }, Projectile.Center);
                
                // 创建粒子效果
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, 0f, 0f, 100, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity *= 2f;
                }
                
                // 弹幕完成任务，销毁自己
                Projectile.Kill();
            }
        }

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