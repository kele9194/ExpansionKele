using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 通用Glowmask辅助类，用于简化物品和弹幕的发光遮罩实现
    /// </summary>
    public static class GlowmaskHelper
    {
        // 缓存已加载的纹理资源
        private static Dictionary<string, Asset<Texture2D>> _textureCache = new Dictionary<string, Asset<Texture2D>>();

        /// <summary>
        /// 获取指定路径的glowmask纹理
        /// </summary>
        /// <param name="texturePath">基础纹理路径</param>
        /// <param name="suffix">后缀，默认为"_Glowmask"</param>
        /// <returns>纹理资源，如果不存在则返回null</returns>
        public static Asset<Texture2D> GetGlowmaskTexture(string texturePath, string suffix = "_Glowmask")
        {
            string glowmaskPath = texturePath + suffix;
            
            // 检查缓存
            if (_textureCache.ContainsKey(glowmaskPath))
            {
                return _textureCache[glowmaskPath];
            }

            // 尝试加载纹理
            try
            {
                var texture = ModContent.Request<Texture2D>(glowmaskPath, AssetRequestMode.DoNotLoad);
                if (texture.IsLoaded)
                {
                    _textureCache[glowmaskPath] = texture;
                    return texture;
                }
            }
            catch
            {
                // 纹理不存在，缓存null避免重复尝试
                _textureCache[glowmaskPath] = null;
            }

            return null;
        }

        /// <summary>
        /// 绘制物品的世界glowmask
        /// </summary>
        /// <param name="item">物品实例</param>
        /// <param name="spriteBatch">精灵批处理</param>
        /// <param name="lightColor">光照颜色</param>
        /// <param name="alphaColor">透明度颜色</param>
        /// <param name="rotation">旋转角度</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="whoAmI">物品ID</param>
        /// <param name="offsetY">Y轴偏移</param>
        public static void DrawItemGlowmaskInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, 
            float rotation, float scale, int whoAmI, float offsetY = 2f)
        {
            string texturePath = item.ModItem.Texture;
            var glowmaskTexture = GetGlowmaskTexture(texturePath);

            if (glowmaskTexture?.Value != null)
            {
                spriteBatch.Draw(
                    glowmaskTexture.Value,
                    new Vector2(
                        item.position.X - Main.screenPosition.X + item.width * 0.5f,
                        item.position.Y - Main.screenPosition.Y + item.height - glowmaskTexture.Value.Height * 0.5f + offsetY
                    ),
                    new Rectangle(0, 0, glowmaskTexture.Value.Width, glowmaskTexture.Value.Height),
                    Color.White,
                    rotation,
                    glowmaskTexture.Value.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// 绘制物品的背包glowmask
        /// </summary>
        /// <param name="item">物品实例</param>
        /// <param name="spriteBatch">精灵批处理</param>
        /// <param name="position">绘制位置</param>
        /// <param name="frame">矩形框架</param>
        /// <param name="drawColor">绘制颜色</param>
        /// <param name="itemColor">物品颜色</param>
        /// <param name="origin">原点</param>
        /// <param name="scale">缩放比例</param>
        public static void DrawItemGlowmaskInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, 
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            string texturePath = item.ModItem.Texture;
            var glowmaskTexture = GetGlowmaskTexture(texturePath);

            if (glowmaskTexture?.Value != null)
            {
                spriteBatch.Draw(
                    glowmaskTexture.Value,
                    position,
                    frame,
                    Color.White,
                    0f,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// 绘制弹幕的glowmask
        /// </summary>
        /// <param name="projectile">弹幕实例</param>
        /// <param name="lightColor">光照颜色</param>
        public static void DrawProjectileGlowmask(Projectile projectile, Color lightColor)
        {
            string texturePath = projectile.ModProjectile.Texture;
            var glowmaskTexture = GetGlowmaskTexture(texturePath);

            if (glowmaskTexture?.Value != null)
            {
                Main.EntitySpriteDraw(
                    glowmaskTexture.Value,
                    projectile.Center - Main.screenPosition,
                    null,
                    Color.White,
                    projectile.rotation,
                    glowmaskTexture.Value.Size() * 0.5f,
                    projectile.scale,
                    SpriteEffects.None,
                    0
                );
            }
        }

        /// <summary>
        /// 清理缓存资源
        /// </summary>
        public static void ClearCache()
        {
            _textureCache.Clear();
        }

        /// <summary>
        /// 手动预加载指定物品的glowmask纹理
        /// </summary>
        /// <param name="modItem">模组物品</param>
        public static void PreloadItemGlowmask(ModItem modItem)
        {
            GetGlowmaskTexture(modItem.Texture);
        }

        /// <summary>
        /// 手动预加载指定弹幕的glowmask纹理
        /// </summary>
        /// <param name="modProjectile">模组弹幕</param>
        public static void PreloadProjectileGlowmask(ModProjectile modProjectile)
        {
            GetGlowmaskTexture(modProjectile.Texture);
        }
    }
}