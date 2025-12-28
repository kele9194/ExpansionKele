using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExpansionKele.Content.Customs.ECShield
{
    // 自定义UI元素，显示在玩家上方
    // ... existing code ...
// ... existing code ...
internal class PlayerAboveUIElement : UIElement
{
    private int _playerIndex;
    private Vector2 _dragOffset;
    private bool _isDragging;
    private static Vector2 _position = new Vector2(-1, -1); // 使用(-1, -1)表示使用默认位置

    public PlayerAboveUIElement(int playerIndex = -1)
    {
        this._playerIndex = playerIndex == -1 ? Main.myPlayer : playerIndex;
        Width.Set(100f, 0f);
        Height.Set(30f, 0f);
        
        // 如果位置是默认值，则使用原来的位置
        if (_position.X == -1 && _position.Y == -1)
        {
            // 设置为默认位置（基于玩家位置的偏移）
            Left.Set(0f, 0f);
            Top.Set(0f, 0f);
        }
        else
        {
            // 使用之前保存的位置
            Left.Set(_position.X, 0f);
            Top.Set(_position.Y, 0f);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Player player = _playerIndex == -1 ? Main.LocalPlayer : Main.player[_playerIndex];
        if (player == null || player.active == false)
            return;

        // 如果在拖动状态中，使用鼠标位置作为参考
        if (_isDragging)
        {
            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
            if (_isDragging)
            {
                _position = mousePos - _dragOffset;
                Left.Set(_position.X, 0f);
                Top.Set(_position.Y, 0f);
            }
        }
        else
        {
            // 当不在拖动状态时，更新位置为相对于玩家的原始位置
            if (_position.X == -1 && _position.Y == -1)
            {
                // 保持原始的相对位置
                Vector2 originalPos = player.Center - Main.screenPosition + new Vector2(-100, -85);
                Left.Set(originalPos.X, 0f);
                Top.Set(originalPos.Y, 0f);
            }
        }

        // 获取当前UI元素的屏幕位置用于碰撞检测
        Vector2 currentPos = new Vector2(Left.Pixels, Top.Pixels);
        Rectangle hitbox = new Rectangle((int)(currentPos.X), (int)(currentPos.Y), (int)Width.Pixels, (int)Height.Pixels);
        Vector2 mousePosForHitbox = new Vector2(Main.mouseX, Main.mouseY);

        if (Main.mouseRight && hitbox.Contains((int)mousePosForHitbox.X, (int)mousePosForHitbox.Y))
        {
            if (!_isDragging)
            {
                _isDragging = true;
                _dragOffset = mousePosForHitbox - currentPos;
            }
        }
        else if (_isDragging && !Main.mouseRight)
        {
            _isDragging = false;
        }
    }

    // ... existing code ...
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        int BaseWidth = 100; // 修改为100
        int BaseHeight = 18; // 修改为30
        Player player = _playerIndex == -1 ? Main.LocalPlayer : Main.player[_playerIndex];
        if (player == null || player.active == false)
            return;

        // 获取玩家的ECShieldSystem组件
        ECShieldSystem ecShield = player.GetModPlayer<ECShieldSystem>();
        if(ecShield.ShieldActive){

        // 根据是否被拖动来决定位置
        Vector2 screenPos;
        if (_position.X == -1 && _position.Y == -1)
        {
            // 使用原始的相对于玩家的位置
            screenPos = player.Center - Main.screenPosition + new Vector2(-100 - BaseWidth/2, -85 - BaseHeight/2);
        }
        else
        {
            // 使用拖动后的位置
            screenPos = new Vector2(Left.Pixels, Top.Pixels);
        }

        // 使用默认纹理绘制一个简单的图像并放大10倍
        // ... existing code ...
        // 使用默认纹理绘制一个简单的图像并放大10倍
        // ... existing code ...
        // 使用默认纹理绘制一个简单的图像并放大10倍
        Texture2D texture = ModContent.Request<Texture2D>("ExpansionKele/Content/Customs/ECShield/Defense").Value;
        

        spriteBatch.Draw(
            texture,
            screenPos,
            null,
            Color.Blue, // 使用黄色以便于看到
            0f,
            new Vector2(texture.Width/2, texture.Height/2), // 原点设置为纹理的中心底部
            0.75f, // 放大2倍
            SpriteEffects.None,
            0f
        );

        // 计算进度条填充宽度（根据护盾值计算填充比例）
        int barWidth = BaseWidth; // 进度条总宽度
        int barHeight = BaseHeight; // 进度条高度
        
        // 修复整数除法问题，使用浮点数计算
        float fillRatio = ecShield.MaxShield > 0 ? ecShield.CurrentShield / ecShield.MaxShield : 0f;
        int filledWidth = (int)(fillRatio * barWidth); // 已填充的宽度

        // 绘制进度条背景
        Rectangle barBackground = new Rectangle((int)screenPos.X +texture.Width/2, (int)screenPos.Y - barHeight/2 , barWidth, barHeight);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, barBackground, Color.DarkGray);

        // 绘制进度条填充部分
        if(filledWidth > 0)
        {
            Rectangle barFill = new Rectangle((int)screenPos.X +texture.Width/2, (int)screenPos.Y - barHeight/2 , filledWidth, barHeight);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, barFill, Color.Green);
        }

        string shieldText = $"{ecShield.CurrentShield:F0} / {ecShield.MaxShield:F0}"; // 修改为整数格式
        Vector2 textSize = FontAssets.MouseText.Value.MeasureString(shieldText);
        Vector2 textPosition = new Vector2(
            (int)screenPos.X + texture.Width / 2 + barWidth / 2 - textSize.X / 2, // 水平居中
            (int)screenPos.Y - barHeight / 2 // 在进度条上方30像素
        );

        // 绘制文字阴影以提高可读性
        spriteBatch.DrawString(FontAssets.MouseText.Value, shieldText, textPosition + new Vector2(1, 1), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        // 绘制主文字
        spriteBatch.DrawString(FontAssets.MouseText.Value, shieldText, textPosition, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        
        }
        
    }

    }


    // UI状态，管理显示在玩家上方的UI元素
    internal class PlayerAboveUIState : UIState
    {
        private PlayerAboveUIElement playerAboveElement;

        public override void OnInitialize()
        {
            playerAboveElement = new PlayerAboveUIElement();
            Append(playerAboveElement);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 确保UI始终可见，不进行额外的条件检查
            base.Draw(spriteBatch);
        }
    }

    // UI系统，用于管理始终显示在玩家上方的UI元素
    [Autoload(Side = ModSide.Client)]
    public class PlayerAboveUISystem : ModSystem
    {
        internal PlayerAboveUIState PlayerAboveUI;
        internal UserInterface PlayerAboveUserInterface;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                PlayerAboveUI = new PlayerAboveUIState();
                PlayerAboveUI.Activate();

                PlayerAboveUserInterface = new UserInterface();
                PlayerAboveUserInterface.SetState(PlayerAboveUI);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            PlayerAboveUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 在所有其他UI层之后插入，确保显示在最上方
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "ExpansionKele: Player Above UI",
                    delegate
                    {
                        PlayerAboveUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}