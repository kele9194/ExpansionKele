using ExpansionKele.Content.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class AutoAimingMarker : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("自动瞄准标记");
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false; // 无伤害
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2; // 极短的存在时间，通过AI不断续命
            Projectile.penetrate = -1; // 无限穿透
            Projectile.alpha = 128;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            // 续命，保持存在
            Projectile.timeLeft = 2;
            
            // 检查是否应该销毁标记（当持有者不存在或者不是持有指定武器时）
            if (!Main.player[Projectile.owner].active || Main.player[Projectile.owner].dead)
            {
                Projectile.Kill();
                return;
            }
            
            // 当玩家不再持有自动瞄准狙击步枪时销毁标记
            if (Main.player[Projectile.owner].HeldItem.type != ModContent.ItemType<AutoAimingSniperRifle>())
            {
                Projectile.Kill();
            }
            
            // 额外检查：确保标记位置合理
            NPC target = Main.npc[(int)Projectile.ai[0]];
            if (target == null || !target.active)
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            
            // 绘制标记
            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                Color.Red * 0.8f,
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0
            );
            
            return false; // 我们已经手动绘制了，不需要默认绘制
        }
        
        public override void OnKill(int timeLeft)
        {
            // 确保彻底清理
            Projectile.active = false;
        }
    }
}