using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Bosses.ShadowOfRevenge
{
    public class BossMoonProj2 : ModProjectile
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
            // Projectile.ai[0]: 0 = 向目标点移动, 1 = 减速, 2 = 返回Boss
            // Projectile.ai[1]: 目标点的X坐标或者返回时的计时器
            // Projectile.localAI[0]: 目标点的Y坐标
            // Projectile.localAI[1]: 初始速度长度

            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = Projectile.velocity.Length();
            }

            if (Projectile.ai[0] == 0f)
            {
                // 向目标点移动
                if (Vector2.Distance(Projectile.Center, new Vector2(Projectile.ai[1], Projectile.localAI[0])) < 20f)
                {
                    // 到达目标点，开始减速
                    Projectile.ai[0] = 1f;
                    Projectile.ai[1] = 0f; // 用作减速计时器
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                // 减速阶段
                Projectile.ai[1]++;
                float progress = Projectile.ai[1] / 60f; // 60帧内减速
                
                if (progress >= 1f)
                {
                    // 减速完成，开始返回Boss
                    Projectile.ai[0] = 2f;
                    
                    // 寻找Boss
                    NPC owner = null;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<ShadowOfRevenge>())
                        {
                            owner = Main.npc[i];
                            break;
                        }
                    }

                    if (owner != null && owner.active)
                    {
                        // 设置返回方向
                        Vector2 returnDirection = owner.Center - Projectile.Center;
                        returnDirection.Normalize();
                        Projectile.velocity = returnDirection * Projectile.localAI[1];
                    }
                    else
                    {
                        // 没找到Boss则销毁
                        Projectile.Kill();
                    }
                }
                else
                {
                    // 减速
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MathHelper.Lerp(Projectile.localAI[1], 0f, progress);
                }
            }
            else if (Projectile.ai[0] == 2f)
            {
                // 返回Boss阶段
                NPC owner = null;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<ShadowOfRevenge>())
                    {
                        owner = Main.npc[i];
                        break;
                    }
                }

                if (owner == null || !owner.active)
                {
                    Projectile.Kill();
                    return;
                }

                // 检查是否接近Boss
                if (Vector2.Distance(Projectile.Center, owner.Center) < 30f)
                {
                    Projectile.Kill();
                    return;
                }

                // 调整方向朝向Boss
                Vector2 directionToOwner = owner.Center - Projectile.Center;
                directionToOwner.Normalize();
                Vector2 newVelocity = directionToOwner * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, newVelocity, 0.1f);
            }

            // 旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}