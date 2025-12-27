using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.IO;
using Terraria.DataStructures;
using System.Linq;

namespace ExpansionKele.Content.Customs.Commands
{
    /// <summary>
    /// 物品属性修改数据
    /// </summary>
    public class ItemPropertyModification
    {
        public enum ModificationType
        {
            Multiply,
            Add,
            SetValue
        }

        public ModificationType Type;
        public float Value;
        public string Description;

        public ItemPropertyModification(ModificationType type, float value, string description)
        {
            Type = type;
            Value = value;
            Description = description;
        }
    }

    /// <summary>
    /// 管理物品属性修改的系统，支持在游戏过程中动态修改物品属性
    /// </summary>
    public class RuntimeItemModificationSystem : ModSystem
    {
        /// <summary>
        /// 存储物品netID到其修改列表的映射
        /// </summary>
        public static Dictionary<int, List<ItemPropertyModification>> ItemModifications = new Dictionary<int, List<ItemPropertyModification>>();
        
        /// <summary>
        /// 存储玩家持有的已修改物品
        /// </summary>
        public static Dictionary<string, Dictionary<int, List<ItemPropertyModification>>> PlayerSpecificModifications = new Dictionary<string, Dictionary<int, List<ItemPropertyModification>>>();

        public override void OnWorldLoad()
        {
            ItemModifications.Clear();
            PlayerSpecificModifications.Clear();
            LoadModifications();
        }

        public override void OnWorldUnload()
        {
            SaveModifications();
            ItemModifications.Clear();
            PlayerSpecificModifications.Clear();
        }

        /// <summary>
        /// 为指定物品添加伤害修改（全局修改，影响所有该类型物品）
        /// </summary>
        /// <param name="itemNetID">物品的netID</param>
        /// <param name="modType">修改类型</param>
        /// <param name="value">修改值</param>
        /// <param name="description">修改描述</param>
        public static void AddGlobalDamageModification(int itemNetID, ItemPropertyModification.ModificationType modType, float value, string description)
        {
            if (!ItemModifications.ContainsKey(itemNetID))
            {
                ItemModifications[itemNetID] = new List<ItemPropertyModification>();
            }

            ItemModifications[itemNetID].Add(new ItemPropertyModification(modType, value, description));
        }

        // ... existing code ...
        /// <param name="player">目标玩家</param>
        /// <param name="itemNetID">物品的netID</param>
        /// <param name="modType">修改类型</param>
        /// <param name="value">修改值</param>
        /// <param name="description">修改描述</param>
        public static void AddPlayerSpecificDamageModification(Player player, int itemNetID, ItemPropertyModification.ModificationType modType, float value, string description)
        {
            string playerKey = GetPlayerKey(player);
            
            if (!PlayerSpecificModifications.ContainsKey(playerKey))
            {
                PlayerSpecificModifications[playerKey] = new Dictionary<int, List<ItemPropertyModification>>();
            }

            // 每次新修改都覆盖旧修改，而不是累积
            PlayerSpecificModifications[playerKey][itemNetID] = new List<ItemPropertyModification>();
            PlayerSpecificModifications[playerKey][itemNetID].Add(new ItemPropertyModification(modType, value, description));
        }
// ... existing code ...

        /// <summary>
        /// 移除指定物品的所有全局修改
        /// </summary>
        /// <param name="itemNetID">物品的netID</param>
        public static void ClearGlobalModifications(int itemNetID)
        {
            if (ItemModifications.ContainsKey(itemNetID))
            {
                ItemModifications.Remove(itemNetID);
            }
        }

        /// <summary>
        /// 移除指定玩家特定物品的所有修改
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="itemNetID">物品的netID</param>
        public static void ClearPlayerSpecificModifications(Player player, int itemNetID)
        {
            string playerKey = GetPlayerKey(player);
            
            if (PlayerSpecificModifications.ContainsKey(playerKey) && 
                PlayerSpecificModifications[playerKey].ContainsKey(itemNetID))
            {
                PlayerSpecificModifications[playerKey].Remove(itemNetID);
            }
        }

        /// <summary>
        /// 清除指定玩家的所有修改
        /// </summary>
        /// <param name="player">目标玩家</param>
        public static void ClearAllPlayerSpecificModifications(Player player)
        {
            string playerKey = GetPlayerKey(player);
            
            if (PlayerSpecificModifications.ContainsKey(playerKey))
            {
                PlayerSpecificModifications[playerKey].Clear();
            }
        }

        /// <summary>
        /// 获取指定物品的全局修改列表
        /// </summary>
        /// <param name="itemNetID">物品的netID</param>
        /// <returns>修改列表</returns>
        public static List<ItemPropertyModification> GetGlobalModifications(int itemNetID)
        {
            if (ItemModifications.ContainsKey(itemNetID))
            {
                return ItemModifications[itemNetID];
            }

            return new List<ItemPropertyModification>();
        }

        /// <summary>
        /// 获取指定玩家特定物品的修改列表
        /// </summary>
        /// <param name="player">目标玩家</param>
        /// <param name="itemNetID">物品的netID</param>
        /// <returns>修改列表</returns>
        public static List<ItemPropertyModification> GetPlayerSpecificModifications(Player player, int itemNetID)
        {
            string playerKey = GetPlayerKey(player);
            
            if (PlayerSpecificModifications.ContainsKey(playerKey) && 
                PlayerSpecificModifications[playerKey].ContainsKey(itemNetID))
            {
                return PlayerSpecificModifications[playerKey][itemNetID];
            }

            return new List<ItemPropertyModification>();
        }

        /// <summary>
        /// 应用修改到物品伤害
        /// </summary>
        /// <param name="item">要修改的物品</param>
        /// <param name="player">物品持有者</param>
        /// <param name="originalDamage">原始伤害</param>
        /// <returns>修改后的伤害</returns>
        public static int ApplyDamageModifications(Item item, Player player, int originalDamage)
        {
            int finalDamage = originalDamage;
            
            // 应用全局修改
            if (ItemModifications.ContainsKey(item.netID))
            {
                foreach (var modification in ItemModifications[item.netID])
                {
                    finalDamage = ApplyModification(finalDamage, modification);
                }
            }
            
            // 应用玩家特定修改
            if (player != null)
            {
                string playerKey = GetPlayerKey(player);
                if (PlayerSpecificModifications.ContainsKey(playerKey) && 
                    PlayerSpecificModifications[playerKey].ContainsKey(item.netID))
                {
                    foreach (var modification in PlayerSpecificModifications[playerKey][item.netID])
                    {
                        finalDamage = ApplyModification(finalDamage, modification);
                    }
                }
            }

            return finalDamage;
        }

        /// <summary>
        /// 应用单个修改
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="modification">修改信息</param>
        /// <returns>修改后的值</returns>
        private static int ApplyModification(int value, ItemPropertyModification modification)
        {
            switch (modification.Type)
            {
                case ItemPropertyModification.ModificationType.Multiply:
                    return (int)(value * modification.Value);
                case ItemPropertyModification.ModificationType.Add:
                    return value + (int)modification.Value;
                case ItemPropertyModification.ModificationType.SetValue:
                    return (int)modification.Value;
                default:
                    return value;
            }
        }

        /// <summary>
        /// 获取玩家唯一标识符
        /// </summary>
        /// <param name="player">玩家对象</param>
        /// <returns>玩家标识符</returns>
        private static string GetPlayerKey(Player player)
        {
            return $"{player.name}_{player.whoAmI}";
        }

        public override void Unload()
        {
            SaveModifications();
            ItemModifications.Clear();
            PlayerSpecificModifications.Clear();
        }

        #region 保存和加载修改数据
        private const string SAVE_FILENAME = "item_modifications.dat";

        private void SaveModifications()
        {
            string path = Path.Combine(Main.WorldPath, SAVE_FILENAME);
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    // 保存全局修改
                    writer.Write(ItemModifications.Count);
                    foreach (var kvp in ItemModifications)
                    {
                        writer.Write(kvp.Key); // item netID
                        writer.Write(kvp.Value.Count); // modification count
                        foreach (var mod in kvp.Value)
                        {
                            writer.Write((int)mod.Type);
                            writer.Write(mod.Value);
                            writer.Write(mod.Description ?? "");
                        }
                    }

                    // 保存玩家特定修改
                    writer.Write(PlayerSpecificModifications.Count);
                    foreach (var playerKvp in PlayerSpecificModifications)
                    {
                        writer.Write(playerKvp.Key); // player key
                        writer.Write(playerKvp.Value.Count); // items count
                        foreach (var itemKvp in playerKvp.Value)
                        {
                            writer.Write(itemKvp.Key); // item netID
                            writer.Write(itemKvp.Value.Count); // modification count
                            foreach (var mod in itemKvp.Value)
                            {
                                writer.Write((int)mod.Type);
                                writer.Write(mod.Value);
                                writer.Write(mod.Description ?? "");
                            }
                        }
                    }
                }
            }
            catch
            {
                // 忽略保存错误
            }
        }

        private void LoadModifications()
        {
            string path = Path.Combine(Main.WorldPath, SAVE_FILENAME);
            if (!File.Exists(path)) return;

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    ItemModifications.Clear();
                    PlayerSpecificModifications.Clear();

                    // 加载全局修改
                    int globalItemCount = reader.ReadInt32();
                    for (int i = 0; i < globalItemCount; i++)
                    {
                        int itemNetID = reader.ReadInt32();
                        int modCount = reader.ReadInt32();
                        var modifications = new List<ItemPropertyModification>();

                        for (int j = 0; j < modCount; j++)
                        {
                            ItemPropertyModification.ModificationType type = (ItemPropertyModification.ModificationType)reader.ReadInt32();
                            float value = reader.ReadSingle();
                            string description = reader.ReadString();
                            modifications.Add(new ItemPropertyModification(type, value, description));
                        }

                        ItemModifications[itemNetID] = modifications;
                    }

                    // 加载玩家特定修改
                    int playerCount = reader.ReadInt32();
                    for (int i = 0; i < playerCount; i++)
                    {
                        string playerKey = reader.ReadString();
                        int itemCount = reader.ReadInt32();
                        var playerMods = new Dictionary<int, List<ItemPropertyModification>>();

                        for (int j = 0; j < itemCount; j++)
                        {
                            int itemNetID = reader.ReadInt32();
                            int modCount = reader.ReadInt32();
                            var modifications = new List<ItemPropertyModification>();

                            for (int k = 0; k < modCount; k++)
                            {
                                ItemPropertyModification.ModificationType type = (ItemPropertyModification.ModificationType)reader.ReadInt32();
                                float value = reader.ReadSingle();
                                string description = reader.ReadString();
                                modifications.Add(new ItemPropertyModification(type, value, description));
                            }

                            playerMods[itemNetID] = modifications;
                        }

                        PlayerSpecificModifications[playerKey] = playerMods;
                    }
                }
            }
            catch
            {
                // 忽略加载错误
            }
        }
        #endregion
    }
}