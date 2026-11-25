using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExpansionKele.Content.Items.Weapons.Melee
{
    /// <summary>
    /// 画出「塔纳托斯 / 俄耳甫斯」的作用半径。
    /// <para>
    /// 两个阶段共用 <see cref="TomorrowsEdgePlayer.AttackRadius"/>：
    /// 一阶段是 <c>FindTargets</c> 的挑选半径，二阶段是光环与治疗的判定半径。
    /// </para>
    /// <para>
    /// 走 UI 层而不是 <c>PostDrawTiles</c>：<see cref="GameInterfaceLayer.Draw"/> 每次都会自己
    /// Begin/End，不依赖外部 spriteBatch 状态。
    /// （<c>PostDrawTiles</c> 在装了 HighFPSSupport 时 Begin 还没调用，直接抛
    /// <c>Draw was called, but Begin has not yet been called</c>。）
    /// </para>
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class TomorrowsEdgeRangeRing : ModSystem
    {
        private const string LayerName = "ExpansionKele: TomorrowsEdgeRangeRing";

        /// <summary>
        /// 圆周分段数。600 半径下 48 段的弦高只有约 1.3 像素，2px 线宽下看不出多边形。
        /// （不能改用 <c>Utils.DrawLine</c>：它内部按 width 逐点画，一圈要上千次 Draw。）
        /// </summary>
        private const int Segments = 48;

        /// <summary>线宽（像素）。</summary>
        private const float Thickness = 3f;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 插到最前：圈位于所有原版 UI 之下。
            // 拿到的每帧都是 Main 新建的副本（Main.cs:38317），重复 Insert 不会累积。
            layers.Insert(0, new LegacyGameInterfaceLayer(LayerName, DrawRingLayer, InterfaceScaleType.Game));
        }

        private static bool DrawRingLayer()
        {
            Player player = Main.LocalPlayer;
            if (!player.active || player.dead)
            {
                return true;
            }

            TomorrowsEdgePlayer edge = player.GetModPlayer<TomorrowsEdgePlayer>();
            if (!edge.HoldingEdge)
            {
                return true;
            }

            Color color;
            float baseAlpha;
            switch (edge.Stage)
            {
                case TomorrowsEdgeStage.Thanatos:
                    color = new Color(0x26, 0x48, 0xF5);
                    baseAlpha = 0.55f;
                    break;
                case TomorrowsEdgeStage.Orpheus:
                    color = new Color(0xD3, 0xE2, 0xFF);
                    baseAlpha = 0.45f;
                    break;
                default:
                    return true;
            }

            // 缓慢呼吸，避免一条死线。
            float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);

            DrawRing(player.Center, TomorrowsEdgePlayer.AttackRadius, color * (baseAlpha * pulse));
            return true;
        }

        /// <summary>用贴图拼出圆环；每段单独旋转绘制。</summary>
        private static void DrawRing(Vector2 centerWorld, float radius, Color color)
        {
            // 用 BlackTile（16x16，尺寸确定）而不是 MagicPixel：
            // scale 按贴图实际宽度归一化，避免贴图尺寸不是 1x1 时线段被拉出屏幕。
            Texture2D unit = TextureAssets.BlackTile.Value;
            float unitSize = unit.Width;

            Vector2 center = centerWorld - Main.screenPosition;
            float step = MathHelper.TwoPi / Segments;

            for (int i = 0; i < Segments; i++)
            {
                Vector2 from = center + (step * i).ToRotationVector2() * radius;
                Vector2 to = center + (step * (i + 1)).ToRotationVector2() * radius;
                Vector2 edge = to - from;

                Main.EntitySpriteDraw(unit, from, null, color, edge.ToRotation(),
                    new Vector2(0f, unit.Height * 0.5f),
                    new Vector2(edge.Length() / unitSize, Thickness / unitSize),
                    SpriteEffects.None, 0);
            }
        }
    }
}
