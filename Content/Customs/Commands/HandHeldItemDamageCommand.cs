using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs.Commands
{
    [Autoload(Side = ModSide.Both)]
    public class HandHeldItemDamageCommand : ModCommand
    {
        public override string Command => "HHbalance";

        public override CommandType Type => CommandType.Chat;

        public override string Usage => "/HHbalance h set <float> (for mod-specific items) or /HHbalance van set <float> (for all vanilla items) or /HHbalance clear (to reset all multipliers to 1.0)";

        public override string Description => "Set damage multiplier for mod items based on held item's mod, or all vanilla items, or clear all multipliers to 1.0";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;
            if (player == null)
            {
                SendErrorMessage(caller, "This command can only be used by players.");
                return;
            }

            // 检查参数数量
            if (args.Length < 1)
            {
                ShowUsage(caller);
                return;
            }

            string target = args[0].ToLower();
            string operation = "";
            string valueStr = "";

            if (args.Length >= 3)
            {
                operation = args[1].ToLower();
                valueStr = args[2];
            }

            // Handle clear command
            if (target == "clear")
            {
                // 清除所有倍率设置，将它们重置为1.0
                HandHeldSystem.ClearAllDamageMultipliers();
                
                // 通知所有玩家
                string message = "Server: All damage multipliers have been reset to 1.0x";
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(message, Color.Yellow);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Yellow);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    caller.Reply("Successfully reset all damage multipliers to 1.0x", Color.Green);
                }
                return;
            }

            // 检查是否为set操作（对于非clear命令）
            if (operation != "set")
            {
                ShowUsage(caller);
                return;
            }

            // 解析数值
            if (!float.TryParse(valueStr, out float value))
            {
                SendErrorMessage(caller, "Invalid value. Please enter a valid number.");
                return;
            }

            // 检查数值是否在有效范围内
            if (value < 0)
            {
                SendErrorMessage(caller, "Value must be 0 or greater.");
                return;
            }

            // 检查权限（允许所有玩家执行此命令）
            bool hasPermission = true; // 总是允许执行命令
            
            if (hasPermission)
            {
                if (target == "h")
                {
                    // 获取玩家当前手持的物品
                    Item heldItem = player.HeldItem;
                    
                    if (heldItem == null || heldItem.IsAir)
                    {
                        SendErrorMessage(caller, "You must be holding an item to use this command.");
                        return;
                    }

                    // 检查物品是否来自mod（如果不是mod物品则返回提示）
                    if (heldItem.ModItem == null)
                    {
                        SendErrorMessage(caller, "This command only works with mod items. To modify vanilla items, use '/balance van set <float>'.");
                        return;
                    }

                    // 获取物品所属的mod
                    string modName = heldItem.ModItem.Mod.Name;
                    
                    // 为该mod的所有物品设置伤害倍率
                    HandHeldSystem.SetModDamageMultiplier(modName, value);
                    
                    // 通知所有玩家
                    string message = $"Server: Damage multiplier for all items from mod '{modName}' has been set to {value:F2}x";
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText(message, Color.Yellow);
                    }
                    else if (Main.netMode == NetmodeID.Server)
                    {
                        Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Yellow);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        caller.Reply($"Successfully set damage multiplier for all items from mod '{modName}' to {value:F2}x", Color.Green);
                    }
                }
                else if (target == "van")
                {
                    // 为所有原版物品设置伤害倍率
                    HandHeldSystem.SetVanillaDamageMultiplier(value);
                    
                    // 通知所有玩家
                    string message = $"Server: Damage multiplier for all vanilla items has been set to {value:F2}x";
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Main.NewText(message, Color.Yellow);
                    }
                    else if (Main.netMode == NetmodeID.Server)
                    {
                        Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Yellow);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        caller.Reply($"Successfully set damage multiplier for all vanilla items to {value:F2}x", Color.Green);
                    }
                }
                else
                {
                    ShowUsage(caller);
                    return;
                }
            }
            else
            {
                SendErrorMessage(caller, "Insufficient permissions. Only server host can execute this command.");
            }
        }

        private void ShowUsage(CommandCaller caller)
        {
            if (caller.CommandType == CommandType.Console)
            {
                Console.WriteLine("Usage: /HHbalance h set <float>");
                Console.WriteLine("       /HHbalance van set <float>");
                Console.WriteLine("       /HHbalance clear");
                Console.WriteLine("h: Sets damage multiplier for all items from the mod of the held item.");
                Console.WriteLine("van: Sets damage multiplier for all vanilla items.");
                Console.WriteLine("clear: Resets all damage multipliers to 1.0x");
                Console.WriteLine("Example: /HHbalance h set 1.5 (when holding a mod item)");
                Console.WriteLine("Example: /HHbalance van set 1.5");
                Console.WriteLine("Example: /HHbalance clear");
            }
            else
            {
                caller.Reply("Usage: /HHbalance h set <float>", Color.Yellow);
                caller.Reply("       /HHbalance van set <float>", Color.Yellow);
                caller.Reply("       /HHbalance clear", Color.Yellow);
                caller.Reply("h: Sets damage multiplier for all items from the mod of the held item.", Color.Gray);
                caller.Reply("van: Sets damage multiplier for all vanilla items.", Color.Gray);
                caller.Reply("clear: Resets all damage multipliers to 1.0x", Color.Gray);
                caller.Reply("Example: /HHbalance h set 1.5 (when holding a mod item)", Color.Gray);
                caller.Reply("Example: /HHbalance van set 1.5", Color.Gray);
                caller.Reply("Example: /HHbalance clear", Color.Gray);
            }
        }

        private void SendErrorMessage(CommandCaller caller, string message)
        {
            if (caller.CommandType == CommandType.Console)
            {
                Console.WriteLine($"Error: {message}");
            }
            else
            {
                caller.Reply($"Error: {message}", Color.Red);
            }
        }
    }

    // 删除枚举，因为我们不再需要网络同步
}