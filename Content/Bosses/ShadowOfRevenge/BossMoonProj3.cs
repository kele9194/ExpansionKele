using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Bosses.ShadowOfRevenge
{
    public class BossMoonProj3 : ModProjectile
    {
        public override string LocalizationCategory => "Bosses.ShadowOfRevenge.Projectiles";
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        // 直接设置Projectile造成防御损伤
        if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
        {
            calamity.Call("SetDefenseDamageProjectile", Projectile, true);
        }
    }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Projectile.ai[0]: 计时器
            // Projectile.ai[1]: 是否已锁定玩家
            // Projectile.localAI[0]: 初始速度长度

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = Projectile.velocity.Length();
            }

            Projectile.ai[0]++;

            if (Projectile.ai[0] >= 10f && Projectile.ai[1] == 0f)
            {
                // 开始追踪玩家
                Player target = null;
                float minDistance = float.MaxValue;

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead)
                    {
                        float distance = Vector2.Distance(Projectile.Center, player.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            target = player;
                        }
                    }
                }

                if (target != null)
                {
                    Vector2 direction = target.Center - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = direction * Projectile.localAI[0];
                    Projectile.ai[1] = 1f; // 标记为已锁定
                    
                    // 当距离玩家50像素时停止追踪
                    if (minDistance <= 50f)
                    {
                        Projectile.ai[1] = 2f; // 标记为停止追踪
                    }
                }
            }
            else if (Projectile.ai[1] == 1f)
            {
                // 检查是否应该停止追踪
                Player target = null;
                float minDistance = float.MaxValue;

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead)
                    {
                        float distance = Vector2.Distance(Projectile.Center, player.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            target = player;
                        }
                    }
                }

                if (target != null && minDistance <= 50f)
                {
                    Projectile.ai[1] = 2f; // 停止追踪
                }
            }
            // 如果Projectile.ai[1] == 2f，则继续直线飞行

            // 旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}