using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 阻挡标记 GlobalNPC。
    /// 在被阻挡的敌人头顶绘制红色标记（BlockingTag 贴图）。
    /// 标记位置 = NPC中心 + 向上偏移(半个碰撞箱高度 + 20像素)
    /// </summary>
    public class BlockingMarkerNPC : GlobalNPC
    {
        /// <summary>
        /// 标记贴图（延迟加载）
        /// </summary>
        private static Texture2D _blockingTag;

        /// <summary>
        /// 标记额外向上偏移量（px）
        /// </summary>
        private const float MARKER_EXTRA_OFFSET = 20f;

        /// <summary>
        /// 获取标记贴图（懒加载）
        /// </summary>
        private static Texture2D BlockingTag
        {
            get
            {
                if (_blockingTag == null || _blockingTag.IsDisposed)
                {
                    _blockingTag = ModContent.Request<Texture2D>("ExpansionKele/Content/Customs/BlockingTag", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                return _blockingTag;
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // 只对活跃且可追击的敌人生效
            if (!npc.active || !npc.chaseable || npc.friendly || npc.dontTakeDamage)
                return;

            // 检查是否有任意玩家将此NPC纳入阻挡名单
            bool isBlocked = false;
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                Player player = Main.player[p];
                if (player != null && player.active && !player.dead)
                {
                    var blockingPlayer = player.GetModPlayer<BlockingPlayer>();
                    if (blockingPlayer.BlockingCap > 0 && blockingPlayer.IsInBlockingList(npc))
                    {
                        isBlocked = true;
                        break;
                    }
                }
            }

            if (!isBlocked)
                return;

            // 获取贴图
            Texture2D tex = BlockingTag;
            if (tex == null)
                return;

            // 计算标记位置：
            // NPC中心 → 向上偏移半个碰撞箱高度 → 再向上偏移20像素
            Vector2 markerWorldPos = npc.Center + new Vector2(0, -npc.height / 2f - MARKER_EXTRA_OFFSET);
            Vector2 markerScreenPos = markerWorldPos - screenPos;

            // 以贴图中心为原点绘制
            spriteBatch.Draw(
                tex,
                markerScreenPos,
                null,
                Color.White,
                0f,
                new Vector2(tex.Width / 2f, tex.Height / 2f),
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}