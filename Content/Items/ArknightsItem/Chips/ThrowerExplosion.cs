using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 投掷手延迟爆炸抛射体。
    /// 使用 ai[0] 作为延迟 ticks 数。
    /// - 延迟阶段：显示警告烟雾，标记爆炸落点
    /// - 激活阶段：完整爆炸效果（音效+粒子），然后 alpha 淡出
    /// </summary>
    public class ThrowerExplosion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;           // 直径48px，半径24px（1.5格）
            Projectile.height = 48;
            Projectile.friendly = false;     // 延迟期间不造成伤害
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;          // 延迟期间完全透明
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;  // 每个敌人只命中一次
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            if (Projectile.ai[0] > 0f)
            {
                // ⏳ 延迟阶段：递减计时器，同时显示警告提示
                Projectile.ai[0]--;

                // 每 5 tick 产生一次烟雾，标记即将爆炸的位置
                if (Main.rand.NextBool(5))
                {
                    Dust warning = Dust.NewDustDirect(
                        Projectile.Center - new Vector2(4, 4),
                        8, 8,
                        DustID.Smoke,
                        0f, -1f,
                        80,
                        Color.LightGray,
                        0.6f
                    );
                    warning.noGravity = true;
                }

                // 延迟结束 → 激活爆炸！
                if (Projectile.ai[0] <= 0f)
                {
                    ActivateExplosion();
                }
            }
            else
            {
                // 💥 爆炸后的淡出效果（参考炮手：alpha += 13 / tick）
                Projectile.alpha += 13;
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }
        }

        /// <summary>
        /// 激活爆炸：播放音效、生成粒子、开启伤害
        /// </summary>
        private void ActivateExplosion()
        {
            Projectile.friendly = true;
            Projectile.alpha = 0;

            // 爆炸音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 爆炸火焰粒子（参考炮手）
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center - new Vector2(8, 8),
                    16, 16,
                    DustID.Torch,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100,
                    Color.Orange,
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                dust.noGravity = true;
            }

            // 烟雾粒子
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center - new Vector2(12, 12),
                    24, 24,
                    DustID.Smoke,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    100,
                    Color.LightBlue,
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                dust.noGravity = true;
            }

            // 小范围冲击波火花
            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = Main.rand.NextVector2Circular(1f, 1f);
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center + dir * 20f - new Vector2(2, 2),
                    4, 4,
                    DustID.YellowStarDust,
                    dir.X * 2f,
                    dir.Y * 2f,
                    50,
                    Color.Blue,
                    0.8f
                );
                dust.noGravity = true;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 设置总存活时间 = 延迟 + 20 ticks 显示时间（匹配 alpha += 13 的淡出节奏）
            Projectile.timeLeft = (int)Projectile.ai[0] + 20;
        }
    }
}