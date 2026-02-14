using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Projectiles.MeleeProj;
using System;
using ExpansionKele.Content.Items.Weapons.Melee;

namespace ExpansionKele.Content.Projectiles.MeleeProj
{
    public class ShroomiteSwordProjectile : EnergySwordProjectile
    {
        protected override float TextureScaleMultiplier => 1.2f; // 蓝色刀光大小为0.9f

        protected override Color backDarkColor => new Color(0x2E, 0x5E, 0xFF); // 深蓝色
        protected override Color middleMediumColor => new Color(0x66, 0xB2, 0xFF); // 中蓝色
        protected override Color frontLightColor => new Color(0xAD, 0xD6, 0xFF); // 浅蓝色

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            base.AI();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            
            // 检查敌人是否已经被标记
            if (!target.HasBuff(ModContent.BuffType<Buff.MushroomSwordMark>()))
            {
                // 如果敌人没有被标记，在敌人位置生成2个悬浮蘑菇
                for (int i = 0; i < 2; i++)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        Vector2.Zero, // 静止的悬浮效果
                        ModContent.ProjectileType<FloatingMushroomProjectile>(),
                        (int)(Projectile.damage * 0.4f), // 较低的伤害
                        Projectile.knockBack * 0.2f, // 较小的击退
                        Projectile.owner
                    );
                }
            }
            
            // 标记敌人5秒（300帧）
            target.AddBuff(ModContent.BuffType<Buff.MushroomSwordMark>(), 600);
        }



        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            
            // 死亡时也生成悬浮蘑菇
            SpawnFloatingMushrooms();
        }

        private void SpawnFloatingMushrooms()
        {
            Player player = Main.player[Projectile.owner];
            
            // 统计当前被标记的敌人数量（最多10个）
            int markedEnemyCount = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.HasBuff(ModContent.BuffType<Buff.MushroomSwordMark>()))
                {
                    markedEnemyCount++;
                }
            }
            // 限制最大标记数量为10
            markedEnemyCount = Math.Min(markedEnemyCount, ShroomiteSword.shroomiteSwordMaxCharge);
            
            // 基础生成2-4个蘑菇 + 标记敌人数量的额外蘑菇
            int mushroomCount = Main.rand.Next(2, 5) + markedEnemyCount; // 2-4个基础蘑菇 + 标记敌人数量的额外蘑菇
            
            for (int i = 0; i < mushroomCount; i++)
            {
                Vector2 positionOffset = Main.rand.NextVector2Circular(150f, 150f); // 在150像素范围内随机位置
                Vector2 spawnPosition = player.Center + positionOffset;
                
                // 确保生成位置在合理范围内
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    Vector2.Zero, // 静止的悬浮效果
                    ModContent.ProjectileType<FloatingMushroomProjectile>(),
                    (int)(Projectile.damage * 0.3f), // 较低的伤害
                    Projectile.knockBack * 0.2f, // 较小的击退
                    Projectile.owner
                );
            }
        }
    }
}