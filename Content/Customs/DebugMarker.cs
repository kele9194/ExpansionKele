using System;
using System.Collections.Generic;
using ExpansionKele.Content.Items.OtherItem;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 调试输出工具类，提供类似Main.NewText的调试标记功能
    /// 只有当玩家收藏了NewTexter物品时才会显示调试信息
    /// </summary>
    public static class DebugMarker
    {
        private static bool? _isDebugEnabled = null;
        private static Dictionary<string, int> _lastPrintTicks = new Dictionary<string, int>();
        private const int COOLDOWN_TICKS = 60; // 冷却时间：60 tick (1秒)

        /// <summary>
        /// 检查调试模式是否启用（通过检测玩家是否收藏了NewTexter物品）
        /// </summary>
        public static bool IsDebugEnabled
        {
            get
            {
                if (_isDebugEnabled == null)
                {
                    _isDebugEnabled = CheckDebugStatus();
                }
                return _isDebugEnabled.Value;
            }
        }

        /// <summary>
        /// 重置调试状态缓存（用于物品收藏状态改变时）
        /// </summary>
        public static void ResetDebugCache()
        {
            _isDebugEnabled = null;
        }

        /// <summary>
        /// 检查所有在线玩家是否有人收藏了NewTexter物品
        /// </summary>
        private static bool CheckDebugStatus()
        {
            foreach (Player player in Main.player)
            {
                if (player != null && player.active && !player.dead)
                {
                    foreach (Item item in player.inventory)
                    {
                        if (item != null && !item.IsAir && 
                            item.type == ModContent.ItemType<NewTexter>() &&
                            item.favorited)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 生成标记的唯一键
        /// </summary>
        private static string GenerateMarkKey(string callerFilePath, int callerLineNumber)
        {
            return $"{callerFilePath}:{callerLineNumber}";
        }

        /// <summary>
        /// 检查是否应该打印（考虑冷却时间）
        /// </summary>
        private static bool ShouldPrint(string markKey, bool forcePrint)
        {
            int currentTick = (int)Main.GameUpdateCount;
            
            if (forcePrint)
            {
                return true;
            }

            if (!_lastPrintTicks.ContainsKey(markKey))
            {
                _lastPrintTicks[markKey] = currentTick;
                return true;
            }

            int lastTick = _lastPrintTicks[markKey];
            if (currentTick - lastTick >= COOLDOWN_TICKS)
            {
                _lastPrintTicks[markKey] = currentTick;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 基础形式：输出所在文件和代码行数
        /// </summary>
        /// <param name="forcePrint">是否强制每次都打印（忽略冷却）</param>
        /// <param name="callerFilePath">调用者的文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用者的代码行数（自动填充）</param>
        public static void Mark(
            bool forcePrint = false,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (!IsDebugEnabled) return;

            string markKey = GenerateMarkKey(callerFilePath, callerLineNumber);
            if (!ShouldPrint(markKey, forcePrint)) return;

            string fileName = System.IO.Path.GetFileName(callerFilePath);
            string modeTag = forcePrint ? "[FORCE]" : "[COOLED]";
            string message = $"[DEBUG MARK]{modeTag} File: {fileName}, Line: {callerLineNumber}";
            
            Main.NewText(message, Microsoft.Xna.Framework.Color.Yellow);
        }

        /// <summary>
        /// 重载版本：输出一个参数值，同时显示文件和行数
        /// </summary>
        /// <param name="value">要输出的值</param>
        /// <param name="forcePrint">是否强制每次都打印（忽略冷却）</param>
        /// <param name="callerFilePath">调用者的文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用者的代码行数（自动填充）</param>
        public static void Mark(object value,
            bool forcePrint = false,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (!IsDebugEnabled) return;

            string markKey = GenerateMarkKey(callerFilePath, callerLineNumber);
            if (!ShouldPrint(markKey, forcePrint)) return;

            string fileName = System.IO.Path.GetFileName(callerFilePath);
            string modeTag = forcePrint ? "[FORCE]" : "[COOLED]";
            string message = $"[DEBUG MARK]{modeTag} File: {fileName}, Line: {callerLineNumber} | Value: {value?.ToString() ?? "null"}";
            
            Main.NewText(message, Microsoft.Xna.Framework.Color.Cyan);
        }

        /// <summary>
        /// 重载版本：输出一个字符串消息，同时显示文件和行数
        /// </summary>
        /// <param name="message">要输出的消息</param>
        /// <param name="forcePrint">是否强制每次都打印（忽略冷却）</param>
        /// <param name="callerFilePath">调用者的文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用者的代码行数（自动填充）</param>
        public static void Mark(string message,
            bool forcePrint = false,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (!IsDebugEnabled) return;

            string markKey = GenerateMarkKey(callerFilePath, callerLineNumber);
            if (!ShouldPrint(markKey, forcePrint)) return;

            string fileName = System.IO.Path.GetFileName(callerFilePath);
            string modeTag = forcePrint ? "[FORCE]" : "[COOLED]";
            string output = $"[DEBUG MARK]{modeTag} File: {fileName}, Line: {callerLineNumber} | {message}";
            
            Main.NewText(output, Microsoft.Xna.Framework.Color.LimeGreen);
        }

        /// <summary>
        /// 带颜色自定义的输出方法
        /// </summary>
        /// <param name="message">要输出的消息</param>
        /// <param name="color">文字颜色</param>
        /// <param name="forcePrint">是否强制每次都打印（忽略冷却）</param>
        /// <param name="callerFilePath">调用者的文件路径（自动填充）</param>
        /// <param name="callerLineNumber">调用者的代码行数（自动填充）</param>
        public static void MarkColored(string message, Microsoft.Xna.Framework.Color color,
            bool forcePrint = false,
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0)
        {
            if (!IsDebugEnabled) return;

            string markKey = GenerateMarkKey(callerFilePath, callerLineNumber);
            if (!ShouldPrint(markKey, forcePrint)) return;

            string fileName = System.IO.Path.GetFileName(callerFilePath);
            string modeTag = forcePrint ? "[FORCE]" : "[COOLED]";
            string output = $"[DEBUG MARK]{modeTag} File: {fileName}, Line: {callerLineNumber} | {message}";
            
            Main.NewText(output, color);
        }
    }
}