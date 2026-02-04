using ExpansionKele.Content.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.RangedProj
{
    public class DotZeroFiveSniperCrosshair : ModProjectile
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
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = false; // 无伤害
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2; // 极短的存在时间，通过AI不断续命
            Projectile.penetrate = -1; // 无限穿透
            Projectile.alpha=128;
            Projectile.light=2.0f;
        }

        public override void AI()
        {
            // 让瞄准镜始终跟随鼠标
            Projectile.Center = Main.MouseWorld;
            
            // 续命，保持存在
            Projectile.timeLeft = 2;
            
            // 玩家不再持有该武器时销毁瞄准镜
            if (Main.player[Projectile.owner].HeldItem.type != ModContent.ItemType<DotZeroFiveSniperRifle>())
            {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理
            Texture2D texture =_cachedTexture.Value;
            
            // 计算缩放后的尺寸 (默认2倍大小)
            float scaleMultiplier = 2f;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            
            // 绘制瞄准镜，放大2倍
            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                Color.White*0.7f,
                0f,
                origin,
                scaleMultiplier,
                SpriteEffects.None,
                0
            );
            
            return false; // 我们已经手动绘制了，不需要默认绘制
        }
    }
}