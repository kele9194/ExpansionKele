// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using ReLogic.Graphics;
// using System.Collections.Generic;
// using Terraria;
// using Terraria.GameContent;
// using Terraria.Localization;
// using Terraria.ModLoader;
// using Terraria.ModLoader.UI;
// using Terraria.UI;

// namespace ExpansionKele.Content.Customs
// {
//     // 饰品充能接口
//     public interface IChargeableAccessories
//     {
//         float GetCurrentCharge();
//         float GetMaxCharge();
//     }

//     // 自定义UI元素，显示玩家饰品的充能状态
//     internal class AccessoriesChargeBarElement : UIElement
//     {
//         private int _playerIndex;
//         private Vector2 _dragOffset;
//         private bool _isDragging;
//         private static Vector2 _position = new Vector2(-1, -1); // 使用(-1, -1)表示使用默认位置

//         public AccessoriesChargeBarElement(int playerIndex = -1)
//         {
//             this._playerIndex = playerIndex == -1 ? Main.myPlayer : playerIndex;
//             Width.Set(50f, 0f);  // 宽度改为50
//             Height.Set(10f, 0f); // 高度改为10
            
//             // 如果位置是默认值，则使用原来的位置
//             if (_position.X == -1 && _position.Y == -1)
//             {
//                 // 设置为默认位置（基于玩家位置的偏移）
//                 Left.Set(0f, 0f);
//                 Top.Set(0f, 0f);
//             }
//             else
//             {
//                 // 使用之前保存的位置
//                 Left.Set(_position.X, 0f);
//                 Top.Set(_position.Y, 0f);
//             }
//         }

//         public override void Update(GameTime gameTime)
//         {
//             base.Update(gameTime);
//             Player player = _playerIndex == -1 ? Main.LocalPlayer : Main.player[_playerIndex];
//             if (player == null || player.active == false)
//                 return;

//             // 如果在拖动状态中，使用鼠标位置作为参考
//             if (_isDragging)
//             {
//                 Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
//                 if (_isDragging)
//                 {
//                     _position = mousePos - _dragOffset;
//                     Left.Set(_position.X, 0f);
//                     Top.Set(_position.Y, 0f);
//                 }
//             }
//             else
//             {
//                 // 当不在拖动状态时，更新位置为相对于玩家的原始位置
//                 if (_position.X == -1 && _position.Y == -1)
//                 {
//                     // 保持原始的相对位置 - 位于玩家下方靠近饰品栏处
//                     Vector2 originalPos = player.Center - Main.screenPosition + new Vector2(0, 80);
//                     Left.Set(originalPos.X, 0f);
//                     Top.Set(originalPos.Y, 0f);
//                 }
//             }

//             // 获取当前UI元素的屏幕位置用于碰撞检测
//             Vector2 currentPos = new Vector2(Left.Pixels, Top.Pixels);
//             Rectangle hitbox = new Rectangle((int)(currentPos.X), (int)(currentPos.Y), (int)Width.Pixels, (int)Height.Pixels);
//             Vector2 mousePosForHitbox = new Vector2(Main.mouseX, Main.mouseY);

//             if (Main.mouseRight && hitbox.Contains((int)mousePosForHitbox.X, (int)mousePosForHitbox.Y))
//             {
//                 if (!_isDragging)
//                 {
//                     _isDragging = true;
//                     _dragOffset = mousePosForHitbox - currentPos;
//                 }
//             }
//             else if (_isDragging && !Main.mouseRight)
//             {
//                 _isDragging = false;
//             }
//         }

//         protected override void DrawSelf(SpriteBatch spriteBatch)
//         {
//             int BarWidth = 50;   // 宽度改为50
//             int BarHeight = 10;  // 高度改为10
//             Player player = _playerIndex == -1 ? Main.LocalPlayer : Main.player[_playerIndex];
//             if (player == null || player.active == false)
//                 return;

//             // 获取玩家饰品的充能信息（按顺序遍历饰品栏）
//             var accessories = GetChargeableAccessories(player);
            
//             if(accessories.Count > 0){
//                 // 根据是否被拖动来决定位置
//                 Vector2 screenPos;
//                 if (_position.X == -1 && _position.Y == -1)
//                 {
//                     // 使用原始的相对于玩家的位置
//                     screenPos = player.Center - Main.screenPosition + new Vector2(0, 80);
//                 }
//                 else
//                 {
//                     // 使用拖动后的位置
//                     screenPos = new Vector2(Left.Pixels, Top.Pixels);
//                 }

//                 // 绘制多个充能条，每个饰品一个
//                 for (int i = 0; i < accessories.Count; i++)
//                 {
//                     var accessory = accessories[i];
//                     float currentCharge = accessory.GetCurrentCharge();
//                     float maxCharge = accessory.GetMaxCharge();
                    
//                     if(maxCharge > 0){
//                         // 计算进度条填充宽度（根据充能值计算填充比例）
//                         float fillRatio = maxCharge > 0 ? currentCharge / maxCharge : 0f;
//                         int filledWidth = (int)(fillRatio * BarWidth); // 已填充的宽度

//                         // 绘制进度条背景
//                         Rectangle barBackground = new Rectangle((int)screenPos.X - BarWidth/2, (int)screenPos.Y + i * 20, BarWidth, BarHeight);
//                         spriteBatch.Draw(TextureAssets.MagicPixel.Value, barBackground, Color.DarkGray * 0.5f); // 添加透明度

//                         // 绘制进度条填充部分（蓝色代表充能）
//                         if(filledWidth > 0)
//                         {
//                             // 从左侧开始向右填充
//                             Rectangle barFill = new Rectangle(
//                                 (int)screenPos.X - BarWidth/2, 
//                                 (int)screenPos.Y + i * 20, 
//                                 filledWidth, 
//                                 BarHeight
//                             );
                            
//                             // 根据填充比例调整颜色，从浅蓝到深蓝，并添加透明度
//                             Color fillColor = new Color(0, (byte)(100 + 155 * fillRatio), (byte)(100 + 155 * fillRatio), 180); // 第四个参数是alpha通道，控制透明度
//                             spriteBatch.Draw(TextureAssets.MagicPixel.Value, barFill, fillColor * 0.6f); // 添加透明度
//                         }

//                         // 绘制充能数值文本
//                         string chargeText = $"{currentCharge:F0} / {maxCharge:F0}";
//                         Vector2 textSize = FontAssets.MouseText.Value.MeasureString(chargeText);
//                         Vector2 textPosition = new Vector2(
//                             (int)screenPos.X - BarWidth/2, // 改为与充能条左侧对齐
//                             (int)screenPos.Y + i * 20 + BarHeight / 2 + 5 // 在进度条下方
//                         );

//                         // 绘制文字阴影以提高可读性
//                         spriteBatch.DrawString(FontAssets.MouseText.Value, chargeText, textPosition + new Vector2(1, 1), Color.Black, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
//                         // 绘制主文字
//                         spriteBatch.DrawString(FontAssets.MouseText.Value, chargeText, textPosition, Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
//                     }
//                 }
//             }

//             // 使用统一的位置进行拖动和悬停检测
//             Vector2 currentPos = new Vector2(Left.Pixels, Top.Pixels);
//             // 如果没有设置自定义位置，则使用相对玩家的位置
//             if (_position.X == -1 && _position.Y == -1)
//             {
//                 Vector2 defaultPos = player.Center - Main.screenPosition + new Vector2(0, 80);
//                 currentPos = defaultPos;
//             }

//             // 统一拖动和悬停检测范围
//             int totalHeight = accessories.Count > 0 ? accessories.Count * 20 + 30 : 40;
//             Rectangle interactionBox = new Rectangle((int)(currentPos.X - 25), (int)(currentPos.Y - 10), (int)Width.Pixels + 50, totalHeight);
//             Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
//             bool isMouseInInteractionArea = interactionBox.Contains((int)mousePos.X, (int)mousePos.Y);

//             if (Main.mouseRight && isMouseInInteractionArea)
//             {
//                 if (!_isDragging)
//                 {
//                     _isDragging = true;
//                     _dragOffset = mousePos - currentPos;
//                 }
//             }
//             else if (_isDragging && !Main.mouseRight)
//             {
//                 _isDragging = false;
//             }

//             if (isMouseInInteractionArea) {
//                 UICommon.TooltipMouseText(Language.GetTextValue("Mods.ExpansionKele.Customs.AccessoriesChargeUIText"));
//             }
//         }

//         // 获取所有实现了IChargeableAccessories接口的饰品
//         private List<IChargeableAccessories> GetChargeableAccessories(Player player)
//         {
//             List<IChargeableAccessories> chargeableAccessories = new List<IChargeableAccessories>();
            
//             // 遍历玩家的所有饰品栏位
//             for (int i = 3; i < 3 + 10; i++) // 通常饰品槽从索引3开始，共10个槽位
//             {
//                 if (i < player.inventory.Length)
//                 {
//                     var item = player.inventory[i];
//                     if (item?.ModItem is IChargeableAccessories chargeableAccessory)
//                     {
//                         chargeableAccessories.Add(chargeableAccessory);
//                     }
//                 }
//             }
            
//             // 遍历玩家的其他可能饰品槽位（如护盾、翅膀等）
//             for (int i = 13; i < player.miscEquips.Length; i++)
//             {
//                 var item = player.miscEquips[i];
//                 if (item?.ModItem is IChargeableAccessories chargeableAccessory)
//                 {
//                     chargeableAccessories.Add(chargeableAccessory);
//                 }
//             }
            
//             return chargeableAccessories;
//         }
//     }

//     // UI状态，管理显示在玩家饰品栏旁的充能条UI元素
//     internal class AccessoriesChargeBarState : UIState
//     {
//         private AccessoriesChargeBarElement accessoriesChargeBarElement;

//         public override void OnInitialize()
//         {
//             accessoriesChargeBarElement = new AccessoriesChargeBarElement();
//             Append(accessoriesChargeBarElement);
//         }

//         public override void Draw(SpriteBatch spriteBatch)
//         {
//             // 确保UI始终可见，不进行额外的条件检查
//             base.Draw(spriteBatch);
//         }
//     }

//     // UI系统，用于管理始终显示在玩家饰品栏旁的充能条UI元素
//     [Autoload(Side = ModSide.Client)]
//     public class AccessoriesChargeBarSystem : ModSystem
//     {
//         internal AccessoriesChargeBarState AccessoriesChargeBar;
//         internal UserInterface AccessoriesChargeBarUserInterface;

//         public override void Load()
//         {
//             if (!Main.dedServ)
//             {
//                 AccessoriesChargeBar = new AccessoriesChargeBarState();
//                 AccessoriesChargeBar.Activate();

//                 AccessoriesChargeBarUserInterface = new UserInterface();
//                 AccessoriesChargeBarUserInterface.SetState(AccessoriesChargeBar);
//             }
//         }

//         public override void UpdateUI(GameTime gameTime)
//         {
//             AccessoriesChargeBarUserInterface?.Update(gameTime);
//         }

//         public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
//         {
//             // 在所有其他UI层之后插入，确保显示在最上方
//             int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
//             if (mouseTextIndex != -1)
//             {
//                 layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
//                     "ExpansionKele: Accessories Charge Bar",
//                     delegate
//                     {
//                         AccessoriesChargeBarUserInterface.Draw(Main.spriteBatch, new GameTime());
//                         return true;
//                     },
//                     InterfaceScaleType.UI)
//                 );
//             }
//         }
//     }
// }