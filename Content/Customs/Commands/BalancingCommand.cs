using ExpansionKele.Content.Items.OtherItem;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static ExpansionKele.Content.Customs.Commands.RuntimeItemModificationSystem;
using static ExpansionKele.ExpansionKeleConfig;

namespace ExpansionKele.Content.Customs.Commands
{
    [Autoload(Side = ModSide.Both)]
    public class BalancingCommand : ModCommand
    {
        public override string Command => "balance";

        public override CommandType Type
            => CommandType.Chat;

        public override string Usage => "/balance global set <float> or /balance clear";

        public override string Description => "Set global damage multiplier for all items when holding balance item, or clear to reset to 1.0";

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
                // 检查配置是否允许全局倍率修改
                if (!Instance.EnableGlobalDamageMultiplierModification)
                {
                    SendErrorMessage(caller, "Global damage multiplier modification is disabled in the configuration.");
                    return;
                }

                // 清除全局伤害倍率设置，将其重置为1.0
                BalancingSystem.ClearGlobalDamageMultiplier();
                
                // 通知所有玩家
                string message = "Server: Global damage multiplier has been reset to 1.0x";
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
                    caller.Reply("Successfully reset global damage multiplier to 1.0x", Color.Green);
                }
                return;
            }

            // 检查参数数量是否足够用于set操作
            if (args.Length < 3)
            {
                ShowUsage(caller);
                return;
            }

            // 检查是否为global命令
            if (target != "global")
            {
                ShowUsage(caller);
                return;
            }

            // 检查是否为set操作
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

            // 检查配置是否允许全局倍率修改
            if (!Instance.EnableGlobalDamageMultiplierModification)
            {
                SendErrorMessage(caller, "Global damage multiplier modification is disabled in the configuration.");
                return;
            }

            // 允许所有玩家执行此命令（移除权限检查）
            bool hasPermission = true; // 总是允许执行命令
            
            if (hasPermission)
            {
                // 设置全局伤害倍数
                BalancingSystem.SetGlobalDamageMultiplier(value);
                
                // 通知所有玩家
                string message = $"Server: Global damage multiplier has been set to {value:F2}x";
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
                    caller.Reply($"Successfully set global damage multiplier to {value:F2}x", Color.Green);
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
                Console.WriteLine("Usage: /balance global set <float>");
                Console.WriteLine("       /balance clear");
                Console.WriteLine("Sets global damage multiplier for all items.");
                Console.WriteLine("clear: Resets global damage multiplier to 1.0x");
                Console.WriteLine("Example: /balance global set 1.5");
                Console.WriteLine("Example: /balance clear");
            }
            else
            {
                caller.Reply("Usage: /balance global set <float>", Color.Yellow);
                caller.Reply("       /balance clear", Color.Yellow);
                caller.Reply("Sets global damage multiplier for all items.", Color.Gray);
                caller.Reply("clear: Resets global damage multiplier to 1.0x", Color.Gray);
                caller.Reply("Example: /balance global set 1.5", Color.Gray);
                caller.Reply("Example: /balance clear", Color.Gray);
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

    /// <summary>
    /// 管理平衡天平效果的全局物品组件
    /// </summary>
    public class BalancingGlobalItem : GlobalItem
    {

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            // 对于武器，应用全局伤害倍率
            if (item.damage > 0)
            {
                if (ExpansionKeleConfig.Instance.EnableGlobalDamageMultiplierModification)
                {
                    damage *= BalancingSystem.GlobalDamageMultiplier;
                }
            }
        }
    }

    public enum BalancingMessageType : byte
    {
        SyncGlobalMultiplier
    }

    /// <summary>
    /// 全局平衡系统，用于管理全局伤害倍数
    /// </summary>
    [Autoload(Side = ModSide.Both)]
    public class BalancingSystem : ModSystem
    {
        public static float GlobalDamageMultiplier { get; private set; } = 1.0f;
        
        // 用于保存和加载全局伤害倍数
        private static float _savedMultiplier = 1.0f;

        public static void SetGlobalDamageMultiplier(float multiplier)
        {
            GlobalDamageMultiplier = Math.Max(0, multiplier); // 确保值不小于0
            _savedMultiplier = GlobalDamageMultiplier; // 保存当前设置
        }

        public static void ClearGlobalDamageMultiplier()
        {
            GlobalDamageMultiplier = 1.0f; // 重置为默认值
            _savedMultiplier = GlobalDamageMultiplier; // 保存当前设置
        }

        public override void OnWorldLoad()
        {
            // 恢复之前保存的倍数值
            GlobalDamageMultiplier = _savedMultiplier;
        }

        public override void OnWorldUnload()
        {
            // 保存当前倍数值
            _savedMultiplier = GlobalDamageMultiplier;
        }
        
        // 添加保存数据的方法
        public override void SaveWorldData(TagCompound tag)
        {
            tag["GlobalDamageMultiplier"] = _savedMultiplier;
        }
        
        // 添加加载数据的方法
        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("GlobalDamageMultiplier"))
            {
                _savedMultiplier = tag.GetFloat("GlobalDamageMultiplier");
            }
            else
            {
                _savedMultiplier = 1.0f; // 默认值
            }
            
            GlobalDamageMultiplier = _savedMultiplier;
        }
    }
}