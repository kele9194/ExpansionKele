using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static ExpansionKele.Content.Customs.Commands.RuntimeItemModificationSystem;

namespace ExpansionKele.Content.Customs.Commands
{
    [Autoload(Side = ModSide.Both)]
    public class ItemModificationCommand : ModCommand
    {
        public override string Command => "modi";

        public override CommandType Type
            => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            // 检查是否是clearmodify子命令
            if (args.Length > 0 && args[0].ToLower() == "clear")
            {
                HandleClearCommand(caller, args);
                return;
            }

            // 检查参数数量
            if (args.Length < 3)
            {
                ShowUsage(caller);
                return;
            }

            Player player = caller.Player;
            if (player == null)
            {
                SendErrorMessage(caller, "This command can only be used by players.");
                return;
            }

            string target = args[0].ToLower();
            string operation = args[1].ToLower();
            string valueStr = args[2];

            // 只处理手持物品
            if (target != "h" && target != "held")
            {
                ShowUsage(caller);
                return;
            }

            // 解析修改类型
            ItemPropertyModification.ModificationType modType;
            switch (operation)
            {
                case "multiply":
                case "mul":
                    modType = ItemPropertyModification.ModificationType.Multiply;
                    break;
                case "add":
                    modType = ItemPropertyModification.ModificationType.Add;
                    break;
                case "set":
                case "setvalue":
                    modType = ItemPropertyModification.ModificationType.SetValue;
                    break;
                default:
                    SendErrorMessage(caller, "Invalid operation. Use: multiply, add, or set");
                    return;
            }

            // 解析数值
            if (!float.TryParse(valueStr, out float value))
            {
                SendErrorMessage(caller, "Invalid value. Please enter a valid number.");
                return;
            }

            // 修改手中物品
            Item heldItem = player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
            {
                SendErrorMessage(caller, "You are not holding any item.");
                return;
            }

            AddPlayerSpecificDamageModification(player, heldItem.netID, modType, value, "Command modification");
            
            string operationDesc = GetOperationDescription(modType, value);
            string successMessage = $"Successfully modified held item [{Lang.GetItemNameValue(heldItem.netID)}]: {operationDesc}";
            caller.Reply(successMessage, Color.Green);
        }

        private void HandleClearCommand(CommandCaller caller, string[] args)
        {
            Player player = caller.Player;
            if (player == null)
            {
                SendErrorMessage(caller, "This command can only be used by players.");
                return;
            }

            // 检查是否有all参数
            if (args.Length > 1 && args[1].ToLower() == "all")
            {
                // 清除玩家所有物品的修改
                RuntimeItemModificationSystem.ClearAllPlayerSpecificModifications(player);
                caller.Reply("Successfully cleared modifications for all items in your inventory", Color.Green);
                return;
            }

            Item heldItem = player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
            {
                SendErrorMessage(caller, "You are not holding any item.");
                return;
            }

            // 清除该物品的所有玩家特定修改
            RuntimeItemModificationSystem.ClearPlayerSpecificModifications(player, heldItem.netID);
            
            string successMessage = $"Successfully cleared modifications for held item [{Lang.GetItemNameValue(heldItem.netID)}]";
            caller.Reply(successMessage, Color.Green);
        }

        private void ShowUsage(CommandCaller caller)
        {
            if (caller.CommandType == CommandType.Console)
            {
                Console.WriteLine("Usage: modi <h> <operation> <value>");
                Console.WriteLine("       modi clear [all]");
                Console.WriteLine("Operations:");
                Console.WriteLine("  multiply/mul <value>   - Multiply damage by value");
                Console.WriteLine("  add <value>            - Add value to damage");
                Console.WriteLine("  set <value>            - Set damage to value");
                Console.WriteLine("Subcommands:");
                Console.WriteLine("  clear                  - Clear all modifications for held item");
                Console.WriteLine("  clear all              - Clear all modifications for all items in inventory");
                Console.WriteLine("Example:");
                Console.WriteLine("  modi h add 15");
                Console.WriteLine("  modi clear");
                Console.WriteLine("  modi clear all");
            }
            else
            {
                caller.Reply("Usage: /modi <h> <operation> <value> or /modi clear [all]", Color.Yellow);
                caller.Reply("Operations: multiply/mul, add, set", Color.Gray);
                caller.Reply("Subcommands: clear (held item), clear all (inventory)", Color.Gray);
                caller.Reply("Examples: /modi h add 15  or  /modi clear  or  /modi clear all", Color.Gray);
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

        private string GetOperationDescription(ItemPropertyModification.ModificationType modType, float value)
        {
            switch (modType)
            {
                case ItemPropertyModification.ModificationType.Multiply:
                    return $"Multiply damage by {value}";
                case ItemPropertyModification.ModificationType.Add:
                    return $"Add {value} damage";
                case ItemPropertyModification.ModificationType.SetValue:
                    return $"Set damage to {value}";
                default:
                    return "";
            }
        }
    }
}