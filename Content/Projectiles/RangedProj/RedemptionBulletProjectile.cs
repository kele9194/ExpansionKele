using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class RedemptionBulletProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            
            AIType = ProjectileID.Bullet;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            // 50% 概率恢复生命值
            if (Main.rand.NextBool(2))
            {
                // 使用 RedemptionHealLimiter 来限制每秒恢复的生命值
                player.GetModPlayer<RedemptionHealLimiter>().Heal(1, player);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }

    public class RedemptionHealLimiter : ModPlayer
{
    // 记录上次恢复生命值的时间（游戏帧数）
    private int lastHealFrame = 0;
    private const int HealCooldown = 15; // 冷却时间15帧

    public override void ResetEffects()
    {
        // 不再需要基于秒的重置逻辑
    }

    /// <summary>
    /// 处理救赎弹的生命恢复，限制每次恢复之间有15帧冷却时间
    /// </summary>
    /// <param name="amount">尝试恢复的生命值</param>
    /// <param name="player">要恢复生命的玩家</param>
    public void Heal(int amount, Player player)
    {
        // 检查是否已经过了冷却时间
        int currentFrame = (int)Main.GameUpdateCount;
        if (currentFrame - lastHealFrame >= HealCooldown)
        {
            player.Heal(amount);
            lastHealFrame = currentFrame;
        }
    }
}
}