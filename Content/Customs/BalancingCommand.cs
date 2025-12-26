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
using static ExpansionKele.Content.Customs.RuntimeItemModificationSystem;
using static ExpansionKele.ExpansionKeleConfig;

namespace ExpansionKele.Content.Customs
{
    [Autoload(Side = ModSide.Both)]
    public class BalancingCommand : ModCommand
    {
        public override string Command => "balance";

        public override CommandType Type
            => CommandType.Chat;

        public override string Usage => "/balance global set <float>";

        public override string Description => "Set global damage multiplier for all items when holding balance item";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;
            if (player == null)
            {
                SendErrorMessage(caller, "This command can only be used by players.");
                return;
            }

            // 检查参数数量
            if (args.Length < 3)
            {
                ShowUsage(caller);
                return;
            }

            string target = args[0].ToLower();
            string operation = args[1].ToLower();
            string valueStr = args[2];

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

            // 检查权限（仅服务器管理员或单人游戏中的玩家可以执行此命令）
            if (caller.CommandType == CommandType.Console || Main.netMode == NetmodeID.SinglePlayer)
            {
                // 设置全局伤害倍数
                BalancingSystem.SetGlobalDamageMultiplier(value);
                
                // 同步到所有客户端
                if (Main.netMode == NetmodeID.Server)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)BalancingMessageType.SyncGlobalMultiplier);
                    packet.Write(value);
                    packet.Send();
                }
                
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
                SendErrorMessage(caller, "Insufficient permissions. Only server admins can execute this command.");
            }
        }

        private void ShowUsage(CommandCaller caller)
        {
            if (caller.CommandType == CommandType.Console)
            {
                Console.WriteLine("Usage: /balance global set <float>");
                Console.WriteLine("Sets global damage multiplier for all items.");
                Console.WriteLine("Example: /balance global set 1.5");
            }
            else
            {
                caller.Reply("Usage: /balance global set <float>", Color.Yellow);
                caller.Reply("Sets global damage multiplier for all items.", Color.Gray);
                caller.Reply("Example: /balance global set 1.5", Color.Gray);
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
    /// 管理平衡天平效果的玩家组件
    /// </summary>
    public class BalancingPlayer : ModPlayer
    {
        public override void ResetEffects()
        {
            // 如果启用了全局倍率修改，则不需要重置任何与平衡物品相关的状态
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            // 应用全局伤害倍率（从BalancingSystem获取）
            // 如果启用了全局倍率修改，则应用全局倍率
            if (ExpansionKeleConfig.Instance.EnableGlobalDamageMultiplierModification)
            {
                damage *= BalancingSystem.GlobalDamageMultiplier;
            }
        }

       
        public override void PostUpdateEquips()
        {
            // 如果启用了全局倍率修改，不需要进行任何特殊处理
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
        
        // ... existing code ...
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((byte)BalancingMessageType.SyncGlobalMultiplier);
            writer.Write(GlobalDamageMultiplier);
        }
        
        public override void NetReceive(BinaryReader reader)
        {
            BalancingMessageType msgType = (BalancingMessageType)reader.ReadByte();
            switch (msgType)
            {
                case BalancingMessageType.SyncGlobalMultiplier:
                    float multiplier = reader.ReadSingle();
                    GlobalDamageMultiplier = multiplier;
                    _savedMultiplier = multiplier;
                    break;
            }
        }
    }
}