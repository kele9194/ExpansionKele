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
    // 自定义UI元素，显示在玩家手持物品的充能状态
    public interface IChargeableItem
    {
        float GetCurrentCharge();
        float GetMaxCharge();
    }

    internal class ItemChargeBarElement : UIElement
    {
        private Vector2 _dragOffset;
        private bool _isDragging;
        private static Vector2 _position = new Vector2(-1, -1); // 使用(-1, -1)表示使用默认位置

        public ItemChargeBarElement()
        {
            Width.Set(50f, 0f);  // 宽度改为50
            Height.Set(10f, 0f); // 高度改为10
            
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
            Player player = Main.LocalPlayer;
            if (player == null || player.active == false)
                return;

            // 获取玩家手持物品的充能信息
            float currentCharge = GetCurrentCharge(player);
            float maxCharge = GetMaxCharge(player);

            // 如果没有充能接口或者最大充能为0，则不进行UI更新
            if (maxCharge <= 0)
            {
                return; // 直接返回，不更新UI元素
            }

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
                    // 保持原始的相对位置 - 位于玩家右侧靠近手持物品处
                    Vector2 originalPos = player.Center - Main.screenPosition + new Vector2(60, -20);
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

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            int BarWidth = 50;   // 宽度改为50
            int BarHeight = 10;  // 高度改为10
            Player player = Main.LocalPlayer;
            if (player == null || player.active == false)
                return;

            // 获取玩家手持物品的充能信息（预留接口，后续可以补充具体逻辑）
            float currentCharge = GetCurrentCharge(player);
            float maxCharge = GetMaxCharge(player);
            
            if(maxCharge > 0){
                // 根据是否被拖动来决定位置
                Vector2 screenPos;
                if (_position.X == -1 && _position.Y == -1)
                {
                    // 使用原始的相对于玩家的位置
                    screenPos = player.Center - Main.screenPosition + new Vector2(-80, -150);
                }
                else
                {
                    // 使用拖动后的位置
                    screenPos = new Vector2(Left.Pixels, Top.Pixels);
                }

                // 计算进度条填充宽度（根据充能值计算填充比例）
                float fillRatio = maxCharge > 0 ? currentCharge / maxCharge : 0f;
                int filledWidth = (int)(fillRatio * BarWidth); // 已填充的宽度

                // 绘制进度条背景
                Rectangle barBackground = new Rectangle((int)screenPos.X - BarWidth/2, (int)screenPos.Y, BarWidth, BarHeight);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, barBackground, Color.DarkGray * 0.5f); // 添加透明度

                // 绘制进度条填充部分（绿色代表充能）
                if(filledWidth > 0)
                {
                    // 从左侧开始向右填充
                    Rectangle barFill = new Rectangle(
                        (int)screenPos.X - BarWidth/2, 
                        (int)screenPos.Y, 
                        filledWidth, 
                        BarHeight
                    );
                    
                    // 根据填充比例调整颜色，从浅绿到深绿，并添加透明度
                    Color fillColor = new Color(0, (byte)(100 + 155 * fillRatio), 0, 180); // 第四个参数是alpha通道，控制透明度
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, barFill, fillColor * 0.6f); // 添加透明度
                }

                // 绘制边框
                Utils.DrawBorderString(spriteBatch, "", screenPos, Color.White * 0.7f, 1f); // 添加透明度
                
                // 绘制充能数值文本
                string chargeText = $"{currentCharge:F0} / {maxCharge:F0}";
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(chargeText);
                Vector2 textPosition = new Vector2(
                    (int)screenPos.X - BarWidth/2, // 改为与充能条左侧对齐
                    (int)screenPos.Y + BarHeight / 2 + 5 // 在进度条下方
                );

                // 绘制文字阴影以提高可读性
                spriteBatch.DrawString(FontAssets.MouseText.Value, chargeText, textPosition + new Vector2(1, 1), Color.Black, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                // 绘制主文字，根据充能状态改变颜色：0/max为红色，max/max为绿色
                Color textColor = currentCharge <= 0 ? Color.Red : (currentCharge >= maxCharge ? Color.Green : Color.White);
                spriteBatch.DrawString(FontAssets.MouseText.Value, chargeText, textPosition, textColor, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
            }
            
            // 使用统一的位置进行拖动和悬停检测
            Vector2 currentPos = new Vector2(Left.Pixels, Top.Pixels);
            // 如果没有设置自定义位置，则使用相对玩家的位置
            if (_position.X == -1 && _position.Y == -1)
            {
                Vector2 defaultPos = player.Center - Main.screenPosition + new Vector2(-80, -150);
                currentPos = defaultPos;
            }

            // 只有在有充能物品时才处理交互
            if (maxCharge > 0)
            {
                // 统一拖动和悬停检测范围
                Rectangle interactionBox = new Rectangle((int)(currentPos.X - 25), (int)(currentPos.Y - 10), (int)Width.Pixels + 50, (int)Height.Pixels + 30);
                Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
                bool isMouseInInteractionArea = interactionBox.Contains((int)mousePos.X, (int)mousePos.Y);

                if (Main.mouseRight && isMouseInInteractionArea)
                {
                    if (!_isDragging)
                    {
                        _isDragging = true;
                        _dragOffset = mousePos - currentPos;
                    }
                }
                else if (_isDragging && !Main.mouseRight)
                {
                    _isDragging = false;
                }

                if (isMouseInInteractionArea) {
                    UICommon.TooltipMouseText(Language.GetTextValue("Mods.ExpansionKele.Customs.ItemChargeUIText"));
                }
            }
        }

        // 预留方法：获取当前充能值，后续可以根据实际需求修改
        private float GetCurrentCharge(Player player)
        {
            // 检查玩家手持物品是否实现了充能接口
            var heldItem = player.HeldItem;
            if (heldItem?.ModItem is IChargeableItem chargeableItem)
            {
                return chargeableItem.GetCurrentCharge();
            }
            return 0f;
        }

        // 预留方法：获取最大充能值，后续可以根据实际需求修改
        private float GetMaxCharge(Player player)
        {
            // 检查玩家手持物品是否实现了充能接口
            var heldItem = player.HeldItem;
            if (heldItem?.ModItem is IChargeableItem chargeableItem)
            {
                return chargeableItem.GetMaxCharge();
            }
            return 0f;
        }
    }

    // UI状态，管理显示在玩家手持物品旁的充能条UI元素
    internal class ItemChargeBarState : UIState
    {
        private ItemChargeBarElement itemChargeBarElement;

        public override void OnInitialize()
        {
            // 只为本地玩家创建充能条元素
            itemChargeBarElement = new ItemChargeBarElement();
            Append(itemChargeBarElement);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 确保UI始终可见，不进行额外的条件检查
            base.Draw(spriteBatch);
        }
    }

    // UI系统，用于管理始终显示在玩家手持物品旁的充能条UI元素
    [Autoload(Side = ModSide.Client)]
    public class ItemChargeBarSystem : ModSystem
    {
        internal ItemChargeBarState ItemChargeBar;
        internal UserInterface ItemChargeBarUserInterface;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                ItemChargeBar = new ItemChargeBarState();
                ItemChargeBar.Activate();

                ItemChargeBarUserInterface = new UserInterface();
                ItemChargeBarUserInterface.SetState(ItemChargeBar);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            ItemChargeBarUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 在所有其他UI层之后插入，确保显示在最上方
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "ExpansionKele: Item Charge Bar",
                    delegate
                    {
                        ItemChargeBarUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}