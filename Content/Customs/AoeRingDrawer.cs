using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 范围指示圈的统一绘制入口。
    /// <para/>
    /// 贴图是一张<b>白色</b>径向羽化圆（<c>Content/Customs/AoeRing.png</c>），
    /// 靠 <paramref name="color"/> 染色，所以任意芯片都能共用同一张图，
    /// 不必再为每个范围技复制一份同形状不同颜色的 PNG。
    /// <para/>
    /// 只负责"画"，判定逻辑（圆形碰撞、命中帧数）由各自的弹幕负责 ——
    /// 两者尺寸必须对齐，调用方应传入与 <c>Projectile.width / 2</c> 相同的半径。
    /// </summary>
    public static class AoeRingDrawer
    {
        private const string TexturePath = "ExpansionKele/Content/Customs/AoeRing";

        private static Asset<Texture2D> _texture;

        /// <summary>白色羽化圆贴图（首次访问时加载并缓存）。</summary>
        private static Texture2D Texture => (_texture ??= ModContent.Request<Texture2D>(TexturePath)).Value;

        /// <summary>
        /// 以 <paramref name="screenCenter"/> 为圆心画一个半径 <paramref name="radius"/> 的范围圈。
        /// <para/>
        /// 动画与 RAc 的 <c>ArtilleristAoe</c> 一致：
        /// 尺寸从 0.9 倍扩散到 1.1 倍，不透明度按进度的平方淡出（前两帧最亮）。
        /// 贴图是白色，所以 <paramref name="color"/> 就是最终颜色。
        /// </summary>
        /// <param name="screenCenter">屏幕坐标下的圆心（世界坐标需先减 <see cref="Main.screenPosition"/>）。</param>
        /// <param name="radius">半径（像素），应与弹幕判定半径一致。</param>
        /// <param name="color">染色，贴图为白色因此这里是最终颜色。</param>
        /// <param name="progress">播放进度 0→1（用 <c>1f - timeLeft / Lifetime</c> 计算）。</param>
        public static void Draw(Vector2 screenCenter, float radius, Color color, float progress)
        {
            if (Main.dedServ || radius <= 0f)
                return;

            progress = MathHelper.Clamp(progress, 0f, 1f);

            float alpha = (1f - progress) * (1f - progress);
            if (alpha <= 0f)
                return;

            Texture2D texture = Texture;

            Vector2 origin = texture.Size() / 2f;
            float scale = radius * 2f / texture.Width * (0.9f + 0.2f * progress);

            Main.EntitySpriteDraw(
                texture,
                screenCenter,
                null,
                color * alpha,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0);
        }
    }
}
