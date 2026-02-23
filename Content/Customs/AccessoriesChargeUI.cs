using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 可充能饰品接口
    /// 定义了所有支持充能系统的饰品必须实现的基本方法
    /// </summary>
    public interface IChargeableAccessories
    {
        /// <summary>
        /// 获取当前充能值
        /// </summary>
        /// <returns>当前充能数值</returns>
        float GetCurrentCharge();

        /// <summary>
        /// 获取最大充能值
        /// </summary>
        /// <returns>最大充能数值</returns>
        float GetMaxCharge();

        /// <summary>
        /// 获取饰品显示名称
        /// </summary>
        /// <returns>用于UI显示的饰品名称</returns>
        string GetAccessoryName();
    }

    /// <summary>
    /// 饰品充能饼图UI元素
    /// 负责渲染可充能饰品的可视化界面，支持拖拽和交互功能
    /// </summary>
    internal class AccessoriesChargePieElement : UIElement
    {
        #region 私有字段

        /// <summary>
        /// 拖拽偏移量，用于计算拖拽时的相对位置
        /// </summary>
        private Vector2 _dragOffset;

        /// <summary>
        /// 拖拽状态标识
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// UI元素当前位置缓存 (-1,-1表示使用默认位置)
        /// </summary>
        private static Vector2 _position = new Vector2(-1, -1);

        /// <summary>
        /// 饼图半径（像素）
        /// </summary>
        private const int PIE_RADIUS = 12;

        /// <summary>
        /// 饼图间水平间距（像素）
        /// </summary>
        private const int PIE_SPACING = 8;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化饰品充能饼图UI元素
        /// </summary>
        public AccessoriesChargePieElement()
        {
            // 设置UI元素尺寸
            Width.Set(150f, 0f);   // 宽度适应多个小型饼图
            Height.Set(70f, 0f);   // 高度容纳上下文字内容

            // 初始化位置设置
            InitializePosition();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 每帧更新UI元素状态
        /// 处理拖拽逻辑、位置更新和碰撞检测
        /// </summary>
        /// <param name="gameTime">游戏时间信息</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // 获取本地玩家实例
            Player player = Main.LocalPlayer;
            if (player == null || player.active == false)
                return;

            // 获取充能饰品数据
            var modPlayer = player.GetModPlayer<ChargeableAccessoriesPlayer>();
            var accessories = modPlayer.ChargeableAccessories;

            // 无充能饰品时不执行更新逻辑
            if (accessories.Count <= 0)
            {
                return;
            }

            // 处理拖拽逻辑
            HandleDragOperation();

            // 处理鼠标右键点击检测
            HandleMouseInteraction();
        }

        #endregion

        #region 绘制相关方法

        /// <summary>
        /// 绘制UI元素的主要内容
        /// 包括饼图、文字标签和交互效果
        /// </summary>
        /// <param name="spriteBatch">精灵批处理对象</param>
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            if (player == null || player.active == false)
                return;

            // 获取充能饰品列表
            var modPlayer = player.GetModPlayer<ChargeableAccessoriesPlayer>();
            var accessories = modPlayer.ChargeableAccessories;

            // 有充能饰品时进行绘制
            if (accessories.Count > 0)
            {
                // 确定绘制起始位置
                Vector2 screenPos = DetermineDrawPosition(player);

                // 绘制所有饰品的充能饼图
                for (int i = 0; i < accessories.Count; i++)
                {
                    var accessory = accessories[i];
                    DrawAccessoryPie(accessory, i, screenPos, spriteBatch);
                }
            }
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 初始化UI元素位置
        /// </summary>
        private void InitializePosition()
        {
            if (_position.X == -1 && _position.Y == -1)
            {
                // 使用默认位置
                Left.Set(0f, 0f);
                Top.Set(0f, 0f);
            }
            else
            {
                // 使用已保存的位置
                Left.Set(_position.X, 0f);
                Top.Set(_position.Y, 0f);
            }
        }

        /// <summary>
        /// 处理拖拽操作逻辑
        /// </summary>
        private void HandleDragOperation()
        {
            if (_isDragging)
            {
                // 更新拖拽位置
                Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
                _position = mousePos - _dragOffset;
                Left.Set(_position.X, 0f);
                Top.Set(_position.Y, 0f);
            }
            else
            {
                // 重置到默认位置
                if (_position.X == -1 && _position.Y == -1)
                {
                    Vector2 originalPos = Main.LocalPlayer.Center - Main.screenPosition + new Vector2(-150, -50);
                    Left.Set(originalPos.X, 0f);
                    Top.Set(originalPos.Y, 0f);
                }
            }
        }

        /// <summary>
        /// 处理鼠标交互检测
        /// </summary>
        private void HandleMouseInteraction()
        {
            Vector2 currentPos = new Vector2(Left.Pixels, Top.Pixels);
            Rectangle hitbox = new Rectangle((int)currentPos.X, (int)currentPos.Y, (int)Width.Pixels, (int)Height.Pixels);
            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);

            // 右键点击开始拖拽
            if (Main.mouseRight && hitbox.Contains((int)mousePos.X, (int)mousePos.Y))
            {
                if (!_isDragging)
                {
                    _isDragging = true;
                    _dragOffset = mousePos - currentPos;
                }
            }
            // 松开右键结束拖拽
            else if (_isDragging && !Main.mouseRight)
            {
                _isDragging = false;
            }
        }

        /// <summary>
        /// 确定绘制起始位置
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>屏幕坐标位置</returns>
        private Vector2 DetermineDrawPosition(Player player)
        {
            if (_position.X == -1 && _position.Y == -1)
            {
                // 使用相对于玩家的默认位置
                return player.Center - Main.screenPosition + new Vector2(-150, -50);
            }
            else
            {
                // 使用拖拽后的位置
                return new Vector2(Left.Pixels, Top.Pixels);
            }
        }

        /// <summary>
        /// 绘制单个饰品的充能饼图
        /// 包含背景圆、充能扇形、名称和数值显示
        /// </summary>
        /// <param name="accessory">饰品实例</param>
        /// <param name="index">饰品索引（用于水平排列）</param>
        /// <param name="screenPos">屏幕起始位置</param>
        /// <param name="spriteBatch">精灵批处理对象</param>
        private void DrawAccessoryPie(IChargeableAccessories accessory, int index, Vector2 screenPos, SpriteBatch spriteBatch)
        {
            float currentCharge = accessory.GetCurrentCharge();
            float maxCharge = accessory.GetMaxCharge();
            string accessoryName = accessory.GetAccessoryName();

            if (maxCharge > 0)
            {
                // 计算饼图中心位置
                Vector2 pieCenter = new Vector2(
                    screenPos.X + index * (PIE_RADIUS * 2 + PIE_SPACING),
                    screenPos.Y + 35 // 垂直居中
                );

                // 绘制背景圆（半透明灰色）
                DrawCircle(spriteBatch, pieCenter, PIE_RADIUS, Color.DarkGray * 0.15f);

                // 绘制充能扇形
                float fillRatio = currentCharge / maxCharge;
                if (fillRatio > 0)
                {
                    DrawPieChartFromTop(spriteBatch, pieCenter, PIE_RADIUS, fillRatio, GetChargeColor(fillRatio) * 0.5f);
                }

                // 绘制饰品名称（上方）
                DrawAccessoryName(accessoryName, pieCenter, currentCharge, maxCharge, spriteBatch);

                // 绘制充能数值（下方）
                DrawChargeValue(currentCharge, maxCharge, pieCenter, spriteBatch);
            }
        }

        /// <summary>
        /// 绘制饰品名称文本
        /// </summary>
        /// <param name="accessoryName">饰品名称</param>
        /// <param name="pieCenter">饼图中心位置</param>
        /// <param name="currentCharge">当前充能值</param>
        /// <param name="maxCharge">最大充能值</param>
        /// <param name="spriteBatch">精灵批处理对象</param>
        private void DrawAccessoryName(string accessoryName, Vector2 pieCenter, float currentCharge, float maxCharge, SpriteBatch spriteBatch)
        {
            Vector2 namePosition = new Vector2(
                pieCenter.X,
                pieCenter.Y - PIE_RADIUS - 3
            );

            // 文字阴影效果
            spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                accessoryName,
                namePosition + new Vector2(1, 1),
                Color.Black * 0.8f,
                0f,
                FontAssets.MouseText.Value.MeasureString(accessoryName) * 0.5f,
                0.5f,
                SpriteEffects.None,
                0f
            );

            // 主文字（根据充能状态变色）
            Color nameColor = currentCharge <= 0 ? Color.Red : 
                             (currentCharge >= maxCharge ? Color.LimeGreen : Color.White);
            
            spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                accessoryName,
                namePosition,
                nameColor,
                0f,
                FontAssets.MouseText.Value.MeasureString(accessoryName) * 0.5f,
                0.5f,
                SpriteEffects.None,
                0f
            );
        }

        /// <summary>
        /// 绘制充能数值文本
        /// </summary>
        /// <param name="currentCharge">当前充能值</param>
        /// <param name="maxCharge">最大充能值</param>
        /// <param name="pieCenter">饼图中心位置</param>
        /// <param name="spriteBatch">精灵批处理对象</param>
        private void DrawChargeValue(float currentCharge, float maxCharge, Vector2 pieCenter, SpriteBatch spriteBatch)
        {
            string chargeText = $"{currentCharge:F0}/{maxCharge:F0}";
            Vector2 chargePosition = new Vector2(
                pieCenter.X,
                pieCenter.Y + PIE_RADIUS + 10
            );

            // 数值阴影效果
            spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                chargeText,
                chargePosition + new Vector2(1, 1),
                Color.Black * 0.8f,
                0f,
                FontAssets.MouseText.Value.MeasureString(chargeText) * 0.5f,
                0.5f,
                SpriteEffects.None,
                0f
            );

            // 主要数值（根据充能状态变色）
            Color chargeColor = currentCharge <= 0 ? Color.Red :
                               (currentCharge >= maxCharge ? Color.LimeGreen : Color.White);
                               
            spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                chargeText,
                chargePosition,
                chargeColor,
                0f,
                FontAssets.MouseText.Value.MeasureString(chargeText) * 0.5f,
                0.5f,
                SpriteEffects.None,
                0f
            );
        }

        /// <summary>
        /// 绘制圆形（点阵优化版本）
        /// 使用多个点模拟圆形轮廓，提高视觉质量
        /// </summary>
        /// <param name="spriteBatch">精灵批处理对象</param>
        /// <param name="center">圆心坐标</param>
        /// <param name="radius">半径</param>
        /// <param name="color">颜色</param>
        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int step = System.Math.Max(1, 360 / 72); // 72个点构成圆形

            for (int i = 0; i < 360; i += step)
            {
                float angle = MathHelper.ToRadians(i);
                Vector2 point = center + new Vector2(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle)
                ) * radius;

                // 使用3x3像素点增强圆形视觉效果
                Rectangle rect = new Rectangle((int)point.X - 1, (int)point.Y - 1, 3, 3);
                spriteBatch.Draw(pixel, rect, color);
            }
        }

        /// <summary>
        /// 绘制饼状图扇形（从顶部开始顺时针绘制）
        /// 使用点阵填充算法创建平滑的扇形效果
        /// </summary>
        /// <param name="spriteBatch">精灵批处理对象</param>
        /// <param name="center">圆心坐标</param>
        /// <param name="radius">半径</param>
        /// <param name="fillRatio">填充比例 (0.0-1.0)</param>
        /// <param name="color">填充颜色</param>
        private void DrawPieChartFromTop(SpriteBatch spriteBatch, Vector2 center, float radius, float fillRatio, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int totalSegments = (int)(360 * fillRatio);
            int step = System.Math.Max(1, 360 / 72); // 角度步长

            // 从-90度（正上方）开始绘制扇形
            for (int i = -90; i <= (-90 + totalSegments); i += step)
            {
                float angle = MathHelper.ToRadians(i);

                // 径向填充：从中心向外绘制同心圆点
                for (int r = 1; r <= radius; r++)
                {
                    Vector2 point = center + new Vector2(
                        (float)System.Math.Cos(angle),
                        (float)System.Math.Sin(angle)
                    ) * r;

                    Rectangle rect = new Rectangle((int)point.X, (int)point.Y, 1, 1);
                    spriteBatch.Draw(pixel, rect, color);
                }
            }

            // 绘制边缘轮廓点，确保边界清晰
            if (totalSegments > 0)
            {
                float finalAngle = MathHelper.ToRadians(-90 + totalSegments);
                for (int r = 1; r <= radius; r++)
                {
                    Vector2 edgePoint = center + new Vector2(
                        (float)System.Math.Cos(finalAngle),
                        (float)System.Math.Sin(finalAngle)
                    ) * r;

                    Rectangle rect = new Rectangle((int)edgePoint.X, (int)edgePoint.Y, 1, 1);
                    spriteBatch.Draw(pixel, rect, color);
                }
            }
        }

        /// <summary>
        /// 绘制直线（优化版本）
        /// 支持任意角度和厚度的直线绘制
        /// </summary>
        /// <param name="spriteBatch">精灵批处理对象</param>
        /// <param name="start">起点坐标</param>
        /// <param name="end">终点坐标</param>
        /// <param name="color">线条颜色</param>
        /// <param name="thickness">线条厚度（像素）</param>
        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 direction = end - start;
            float length = direction.Length();

            // 特殊情况：零长度线段
            if (length <= 0.001f)
            {
                Rectangle pointRect = new Rectangle((int)start.X, (int)start.Y, thickness, thickness);
                spriteBatch.Draw(pixel, pointRect, color);
                return;
            }

            // 计算旋转角度
            direction.Normalize();
            float rotation = (float)System.Math.Atan2(direction.Y, direction.X);

            // 绘制旋转矩形实现直线效果
            Rectangle rect = new Rectangle(
                (int)start.X,
                (int)start.Y,
                (int)System.Math.Ceiling(length),
                thickness
            );
            
            Vector2 origin = new Vector2(0, thickness / 2f);
            spriteBatch.Draw(pixel, rect, null, color, rotation, origin, SpriteEffects.None, 0);
        }

        /// <summary>
        /// 根据充能比例获取对应的颜色
        /// 实现颜色渐变效果以直观显示充能状态
        /// </summary>
        /// <param name="fillRatio">充能比例 (0.0-1.0)</param>
        /// <returns>对应的颜色值</returns>
        private Color GetChargeColor(float fillRatio)
        {
            if (fillRatio <= 0.25f)
                return Color.Red;          // 低充能 - 红色警告
            else if (fillRatio <= 0.5f)
                return Color.Orange;       // 中等充能 - 橙色注意
            else if (fillRatio <= 0.75f)
                return Color.Yellow;       // 较高充能 - 黄色良好
            else
                return Color.LimeGreen;    // 高充能 - 绿色优秀
        }

        #endregion
    }

    /// <summary>
    /// 饰品充能UI状态管理器
    /// 负责初始化和管理充能饼图UI元素的生命周期
    /// </summary>
    internal class AccessoriesChargePieState : UIState
    {
        #region 私有字段

        /// <summary>
        /// 充能饼图UI元素实例
        /// </summary>
        private AccessoriesChargePieElement accessoriesChargePieElement;

        #endregion

        #region 生命周期方法

        /// <summary>
        /// UI状态初始化
        /// 创建并添加充能饼图元素到UI树中
        /// </summary>
        public override void OnInitialize()
        {
            accessoriesChargePieElement = new AccessoriesChargePieElement();
            Append(accessoriesChargePieElement);
        }

        /// <summary>
        /// UI绘制方法
        /// 委托给基类处理实际绘制逻辑
        /// </summary>
        /// <param name="spriteBatch">精灵批处理对象</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        #endregion
    }

    /// <summary>
    /// 饰品充能UI系统
    /// 管理整个充能UI系统的加载、更新和渲染流程
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class AccessoriesChargePieSystem : ModSystem
    {
        #region 内部字段

        /// <summary>
        /// 充能饼图UI状态实例
        /// </summary>
        internal AccessoriesChargePieState AccessoriesChargePie;

        /// <summary>
        /// 用户界面管理器
        /// </summary>
        internal UserInterface AccessoriesChargePieUserInterface;

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 系统加载初始化
        /// 创建UI状态和用户界面实例
        /// </summary>
        public override void Load()
        {
            // 仅在客户端运行
            if (!Main.dedServ)
            {
                AccessoriesChargePie = new AccessoriesChargePieState();
                AccessoriesChargePie.Activate();

                AccessoriesChargePieUserInterface = new UserInterface();
                AccessoriesChargePieUserInterface.SetState(AccessoriesChargePie);
            }
        }

        /// <summary>
        /// UI更新方法
        /// 每帧更新用户界面状态
        /// </summary>
        /// <param name="gameTime">游戏时间信息</param>
        public override void UpdateUI(GameTime gameTime)
        {
            AccessoriesChargePieUserInterface?.Update(gameTime);
        }

        /// <summary>
        /// 修改游戏界面层级
        /// 将充能UI插入到鼠标文本层之上，确保正确显示顺序
        /// </summary>
        /// <param name="layers">游戏界面层列表</param>
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "ExpansionKele: Accessories Charge Pie",
                    delegate
                    {
                        AccessoriesChargePieUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        #endregion
    }

    /// <summary>
    /// 可充能饰品玩家数据管理器
    /// 负责跟踪和管理玩家装备的所有可充能饰品
    /// </summary>
    public class ChargeableAccessoriesPlayer : ModPlayer
    {
        #region 属性

        /// <summary>
        /// 当前玩家装备的所有可充能饰品列表
        /// </summary>
        public List<IChargeableAccessories> ChargeableAccessories { get; private set; } = new List<IChargeableAccessories>();

        #endregion

        #region 公共方法

        /// <summary>
        /// 注册新的可充能饰品
        /// 防止重复注册同一饰品实例
        /// </summary>
        /// <param name="accessory">要注册的饰品实例</param>
        public void RegisterAccessory(IChargeableAccessories accessory)
        {
            if (!ChargeableAccessories.Contains(accessory))
            {
                ChargeableAccessories.Add(accessory);
            }
        }

        /// <summary>
        /// 注销指定的可充能饰品
        /// </summary>
        /// <param name="accessory">要注销的饰品实例</param>
        public void UnregisterAccessory(IChargeableAccessories accessory)
        {
            ChargeableAccessories.Remove(accessory);
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 重置效果周期
        /// 每帧清理旧数据，等待饰品重新注册当前状态
        /// </summary>
        public override void ResetEffects()
        {
            base.ResetEffects();
            ChargeableAccessories.Clear();
        }

        #endregion
    }
}