using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class RedemptionBulletProjectile : ModProjectile
    {
        private static Asset<Texture2D> _cachedTexture;

        public override void Load()
        {
            // 预加载纹理资源
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }

        public override void Unload()
        {
            // 清理资源引用
            _cachedTexture = null;
        }
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
            Texture2D texture = _cachedTexture.Value;
            
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
            // 50% 概率消耗血量槽回复生命值
            if (Main.rand.NextBool(2))
            {
                player.GetModPlayer<RedemptionHealLimiter>().TryUseHealSlot();
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
        // 血量槽当前储存的生命值
        public int healSlot = 0;
        // 血量槽最大容量
        public const int MaxHealSlot = 30;
        // 上次回复血量槽的时间
        private int lastHealSlotRegen = 0;
        // 回复间隔（帧数）
        private const int RegenRate = 6;

        public override void PreUpdate()
        {
            // 每隔一定时间自动回复血量槽
            int currentFrame = (int)Main.GameUpdateCount;
            if (currentFrame - lastHealSlotRegen >= RegenRate)
            {
                if (healSlot < MaxHealSlot)
                {
                    healSlot++;
                }
                lastHealSlotRegen = currentFrame;
            }
        }

        /// <summary>
        /// 尝试使用血量槽回复生命值
        /// </summary>
        public void TryUseHealSlot()
        {
            // 如果血量槽有储存的生命值，则消耗1点并回复1点生命值
            if (healSlot > 0)
            {
                healSlot--;
                Player.Heal(1);
            }
        }
    }
}