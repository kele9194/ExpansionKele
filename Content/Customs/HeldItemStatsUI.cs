using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 手持物品使用时间诊断窗口。按 P 键开关，可拖动。
    /// </summary>
    public class HeldItemStatsState : UIState
    {
        private const float LineHeight = 19f;
        private const int LineCount = 9;
        private const float PanelWidth = 360f;
        private const float PanelPadding = 8f;

        private const float LabelX = 10f;
        private const float UseTimeX = 100f;
        private const float AnimTimeX = 238f;

        private static readonly Color TitleColor = new Color(130, 205, 255);
        private static readonly Color HeaderColor = new Color(190, 205, 235);
        private static readonly Color LabelColor = new Color(165, 175, 200);
        private static readonly Color BodyColor = new Color(220, 220, 220);
        private static readonly Color HintColor = new Color(150, 225, 150);
        private static readonly Color WarnColor = new Color(255, 170, 95);

        private UIPanel _panel;
        private readonly UIText[] _labels = new UIText[LineCount];
        private readonly UIText[] _useTexts = new UIText[LineCount];
        private readonly UIText[] _animTexts = new UIText[LineCount];

        private bool _dragging;
        private int _dragBiasX;
        private int _dragBiasY;
        private bool _needRefresh;

        /// <summary>下次 Update 时强制刷新一行文字（开窗瞬间内容不至于空一帧）。</summary>
        public void MarkDirty()
        {
            _needRefresh = true;
        }

        public override void OnInitialize()
        {
            _panel = new UIPanel();
            _panel.Width.Set(PanelWidth, 0f);
            _panel.Height.Set(PanelPadding * 2f + LineHeight * LineCount, 0f);
            _panel.BackgroundColor = new Color(40, 52, 88) * 0.88f;
            _panel.BorderColor = new Color(90, 130, 210) * 0.9f;

            // 用归一化锚点而不是像素，换分辨率不用重调
            HeldItemStatsConfig config = ModContent.GetInstance<HeldItemStatsConfig>();
            _panel.HAlign = config.PanelHAlign;
            _panel.VAlign = config.PanelVAlign;

            Append(_panel);

            // 行数固定、只改文字：避免像参考实现那样每帧 RemoveAllChildren + new UIText
            for (int i = 0; i < LineCount; i++)
            {
                _labels[i] = CreateCell(LabelX, i);
                _useTexts[i] = CreateCell(UseTimeX, i);
                _animTexts[i] = CreateCell(AnimTimeX, i);
            }
        }

        private UIText CreateCell(float x, int row)
        {
            UIText text = new UIText(string.Empty, 0.78f, false);
            text.Left.Set(x, 0f);
            text.Top.Set(PanelPadding + row * LineHeight, 0f);
            text.TextColor = BodyColor;
            _panel.Append(text);
            return text;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            HandleDrag();

            if (_needRefresh || Main.GameUpdateCount % 6u == 0u)
            {
                _needRefresh = false;
                Refresh();
            }
        }

        private void HandleDrag()
        {
            HeldItemStatsConfig config = ModContent.GetInstance<HeldItemStatsConfig>();

            if (!_dragging)
            {
                if (!config.Draggable || !Main.mouseLeft || !_panel.IsMouseHovering)
                    return;

                if (Main.screenWidth <= 0 || Main.screenHeight <= 0)
                    return;

                // HAlign 是按「父容器宽 - 元素宽」算的，bias 必须用同一套基准，
                // 否则拖动时鼠标和面板之间会缓慢漂移
                float usableWidth = Math.Max(1f, Main.screenWidth - _panel.Width.Pixels);
                float usableHeight = Math.Max(1f, Main.screenHeight - _panel.Height.Pixels);

                _dragging = true;
                _dragBiasX = Main.mouseX - (int)(usableWidth * _panel.HAlign);
                _dragBiasY = Main.mouseY - (int)(usableHeight * _panel.VAlign);
                return;
            }

            if (!Main.mouseLeft)
            {
                _dragging = false;
                SavePosition(config);
                return;
            }

            if (Main.screenWidth <= 0 || Main.screenHeight <= 0)
                return;

            float width = Math.Max(1f, Main.screenWidth - _panel.Width.Pixels);
            float height = Math.Max(1f, Main.screenHeight - _panel.Height.Pixels);

            _panel.HAlign = MathHelper.Clamp((Main.mouseX - _dragBiasX) / width, 0f, 1f);
            _panel.VAlign = MathHelper.Clamp((Main.mouseY - _dragBiasY) / height, 0f, 1f);
            _panel.Recalculate();
        }

        /// <summary>拖动结束时把位置写回配置并落盘，下次进游戏保持在原处。</summary>
        private void SavePosition(HeldItemStatsConfig config)
        {
            if (Math.Abs(config.PanelHAlign - _panel.HAlign) < 0.001f
                && Math.Abs(config.PanelVAlign - _panel.VAlign) < 0.001f)
                return;

            config.PanelHAlign = _panel.HAlign;
            config.PanelVAlign = _panel.VAlign;
            config.SaveChanges(silent: true);
        }

        private void Refresh()
        {
            HeldItemStatsData stats = HeldItemStatsCalculator.Compute(Main.LocalPlayer);

            if (!stats.HasItem)
            {
                SetRow(0, "无手持物品", WarnColor, string.Empty, string.Empty, WarnColor);

                for (int i = 1; i < LineCount; i++)
                    SetRow(i, string.Empty, LabelColor, string.Empty, string.Empty, BodyColor);

                return;
            }

            SetRow(0, stats.ItemName, TitleColor, string.Empty, string.Empty, BodyColor);
            SetRow(1, string.Empty, LabelColor, "使用时间", "动画时间", HeaderColor);
            SetRow(2, "当前", LabelColor, $"{stats.ActualUseTime} 帧", $"{stats.ActualAnimation} 帧", BodyColor);
            SetRow(3, "原始", LabelColor, $"{stats.RawUseTime}", $"{stats.RawAnimation}", BodyColor);

            // 使用时间的阈值只在该物品受攻速影响时才有意义；动画时间始终受攻速影响
            string useKeep;
            string useNext;

            if (stats.AttackSpeedIgnored)
            {
                useKeep = "—";
                useNext = "—";
            }
            else if (!stats.SpeedAffectsUseTime)
            {
                useKeep = "不受影响";
                useNext = "不受影响";
            }
            else
            {
                useKeep = FormatThreshold(stats.KeepAdditiveSpeed);
                useNext = stats.HasNextLevel ? FormatThreshold(stats.NextAdditiveSpeed) : "已是最快";
            }

            string animKeep;
            string animNext;

            if (stats.AttackSpeedIgnored)
            {
                animKeep = "—";
                animNext = "—";
            }
            else
            {
                animKeep = FormatThreshold(stats.KeepAnimationAdditiveSpeed);
                animNext = stats.HasNextAnimationLevel ? FormatThreshold(stats.NextAnimationAdditiveSpeed) : "已是最快";
            }

            // 进度色：当前加算攻速落在 [维持门槛, 下一级门槛) 区间内，越接近下一档越绿
            Color useColor = HintColor;
            Color animColor = HintColor;

            if (ModContent.GetInstance<HeldItemStatsConfig>().ProgressColor)
            {
                if (stats.SpeedAffectsUseTime && !stats.AttackSpeedIgnored)
                    useColor = ProgressColor(stats.AdditiveSpeed, stats.KeepAdditiveSpeed, stats.NextAdditiveSpeed);

                if (!stats.AttackSpeedIgnored)
                    animColor = ProgressColor(stats.AdditiveSpeed, stats.KeepAnimationAdditiveSpeed, stats.NextAnimationAdditiveSpeed);
            }

            SetRow(4, "维持", LabelColor, useKeep, animKeep, useColor, animColor);
            SetRow(5, "下一级", LabelColor, useNext, animNext, useColor, animColor);
            SetRow(6, "加算攻速", LabelColor, HeldItemStatsCalculator.Percent2(stats.AdditiveSpeed), string.Empty, BodyColor);
            SetRow(7, "乘算", LabelColor, DescribeMultipliers(stats), string.Empty, BodyColor);
            SetRow(8, "最终攻速", LabelColor, $"{HeldItemStatsCalculator.Percent2(stats.FinalSpeed)}（{stats.FinalSpeed:0.000}×）", string.Empty, BodyColor);
        }

        /// <summary>
        /// 把乘算部分列出来（武器系数 / 使用速度 / 使用时间 / 动画倍率），值为 1 的项省略。
        /// </summary>
        private static string DescribeMultipliers(HeldItemStatsData stats)
        {
            string result = string.Empty;

            if (Math.Abs(stats.AttackSpeedFactor - 1f) > 0.0001f)
                result += $"武器×{stats.AttackSpeedFactor:0.##} ";

            if (Math.Abs(stats.SpeedHookMult - 1f) > 0.0001f)
                result += $"速度×{stats.SpeedHookMult:0.##} ";

            if (Math.Abs(stats.TimeHookMult - 1f) > 0.0001f)
                result += $"时间×{stats.TimeHookMult:0.##} ";

            if (Math.Abs(stats.AnimationHookMult - 1f) > 0.0001f)
                result += $"动画×{stats.AnimationHookMult:0.##} ";

            return result.Length == 0 ? "无" : result.TrimEnd();
        }

        /// <summary>
        /// 阈值单元格。<paramref name="additiveSpeed"/> 已经是量化到 0.01% 的最小可行值，
        /// 所以用「≥」而不是「&gt;」。
        /// </summary>
        private static string FormatThreshold(float additiveSpeed)
        {
            if (float.IsPositiveInfinity(additiveSpeed))
                return "—";

            if (additiveSpeed <= 1.00005f)
                return "无需加成";

            return $"≥ {HeldItemStatsCalculator.Percent2(additiveSpeed)}";
        }

        /// <summary>进度色：当前值在 [维持门槛, 下一级门槛) 里越靠右越绿。</summary>
        private static Color ProgressColor(float current, float keepThreshold, float nextThreshold)
        {
            float span = nextThreshold - keepThreshold;

            if (span <= 0.0001f)
                return HintColor;

            float t = MathHelper.Clamp((current - keepThreshold) / span, 0f, 1f);

            return Color.Lerp(new Color(255, 175, 90), new Color(140, 235, 130), t);
        }

        private void SetRow(int row, string label, Color labelColor, string useValue, string animValue, Color valueColor, Color? animColor = null)
        {
            if (row < 0 || row >= LineCount)
                return;

            SetCell(_labels[row], label, labelColor);
            SetCell(_useTexts[row], useValue, valueColor);
            SetCell(_animTexts[row], animValue, animColor ?? valueColor);
        }

        private static void SetCell(UIText cell, string value, Color color)
        {
            if (cell.Text != value)
                cell.SetText(value);

            cell.TextColor = color;
        }
    }

    /// <summary>
    /// 管理诊断窗口的 <see cref="UserInterface"/> 与开关状态。
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class HeldItemStatsUISystem : ModSystem
    {
        private UserInterface _statsInterface;
        private HeldItemStatsState _statsState;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _statsState = new HeldItemStatsState();
            _statsState.Activate();

            _statsInterface = new UserInterface();
        }

        public override void Unload()
        {
            _statsInterface = null;
            _statsState = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _statsInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.dedServ)
                return;

            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

            if (mouseTextIndex == -1)
                return;

            layers.Insert(
                mouseTextIndex,
                new LegacyGameInterfaceLayer(
                    "ExpansionKele: Held Item Stats",
                    delegate
                    {
                        _statsInterface?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI));
        }

        /// <summary>开 / 关诊断窗口。</summary>
        public void Toggle()
        {
            if (_statsInterface is null || _statsState is null)
                return;

            if (_statsInterface.CurrentState is null)
            {
                _statsState.MarkDirty();
                _statsInterface.SetState(_statsState);
                SoundEngine.PlaySound(SoundID.MenuOpen);
            }
            else
            {
                _statsInterface.SetState(null);
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
        }
    }

    /// <summary>
    /// 按 P 键开关 <see cref="HeldItemStatsState"/> 窗口。
    /// </summary>
    public class HeldItemStatsPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Main.dedServ)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            if (Main.drawingPlayerChat || Main.editSign || Main.editChest || Main.gameMenu)
                return;

            if (!(ExpansionKele.HeldItemStatsKeyBind?.JustPressed ?? false))
                return;

            ModContent.GetInstance<HeldItemStatsUISystem>()?.Toggle();
        }
    }
}
