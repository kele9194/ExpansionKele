using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 手持物品攻速仪表的客户端配置。
    /// </summary>
    /// <remarks>
    /// 面板位置用归一化锚点（0~1）而不是像素，这样换分辨率不需要重调。
    /// 拖动面板结束时会把当前位置写回这里并落盘。
    /// </remarks>
    public class HeldItemStatsConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        /// <summary>面板横向锚点（拖动面板后自动记录，不必手动调）。</summary>
        [DefaultValue(0.98f)]
        [Range(0f, 1f)]
        [Slider]
        public float PanelHAlign { get; set; } = 0.98f;

        /// <summary>面板纵向锚点（拖动面板后自动记录，不必手动调）。</summary>
        [DefaultValue(0.2f)]
        [Range(0f, 1f)]
        [Slider]
        public float PanelVAlign { get; set; } = 0.2f;

        /// <summary>是否允许按住左键拖动面板。</summary>
        [DefaultValue(true)]
        public bool Draggable { get; set; } = true;

        /// <summary>
        /// 是否用颜色表示「离下一档还差多少」：越接近下一档越绿。
        /// </summary>
        [DefaultValue(true)]
        public bool ProgressColor { get; set; } = true;
    }
}
