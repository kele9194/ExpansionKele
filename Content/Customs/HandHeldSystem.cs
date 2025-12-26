using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 管理手持物品伤害倍率的系统，用于管理特定mod物品和原版物品的伤害倍率
    /// </summary>
    [Autoload(Side = ModSide.Both)]
    public class HandHeldSystem : ModSystem
    {
        // 存储各mod的伤害倍率
        public static Dictionary<string, float> ModDamageMultipliers { get; private set; } = new Dictionary<string, float>();
        
        // 存储原版物品的伤害倍率
        public static float VanillaDamageMultiplier { get; private set; } = 1.0f;

        // 用于保存和加载的变量
        private static Dictionary<string, float> _savedModMultipliers = new Dictionary<string, float>();
        private static float _savedVanillaMultiplier = 1.0f;

        public static void SetModDamageMultiplier(string modName, float multiplier)
        {
            ModDamageMultipliers[modName] = Math.Max(0, multiplier); // 确保值不小于0
            _savedModMultipliers[modName] = ModDamageMultipliers[modName]; // 保存当前设置
        }

        public static void SetVanillaDamageMultiplier(float multiplier)
        {
            VanillaDamageMultiplier = Math.Max(0, multiplier); // 确保值不小于0
            _savedVanillaMultiplier = VanillaDamageMultiplier; // 保存当前设置
        }

        public override void OnWorldLoad()
        {
            // 恢复之前保存的倍数值
            ModDamageMultipliers = new Dictionary<string, float>(_savedModMultipliers);
            VanillaDamageMultiplier = _savedVanillaMultiplier;
        }

        public override void OnWorldUnload()
        {
            // 保存当前倍数值
            _savedModMultipliers = new Dictionary<string, float>(ModDamageMultipliers);
            _savedVanillaMultiplier = VanillaDamageMultiplier;
        }

        // 添加保存数据的方法
        public override void SaveWorldData(TagCompound tag)
        {
            // 保存mod倍率
            var modMultipliers = new List<TagCompound>();
            foreach (var kvp in _savedModMultipliers)
            {
                modMultipliers.Add(new TagCompound {
                    {"ModName", kvp.Key},
                    {"Multiplier", kvp.Value}
                });
            }
            tag["ModDamageMultipliers"] = modMultipliers;

            // 保存原版倍率
            tag["VanillaDamageMultiplier"] = _savedVanillaMultiplier;
        }

        // 添加加载数据的方法
        public override void LoadWorldData(TagCompound tag)
        {
            // 加载mod倍率
            _savedModMultipliers.Clear();
            if (tag.ContainsKey("ModDamageMultipliers"))
            {
                var modMultipliers = tag.GetList<TagCompound>("ModDamageMultipliers");
                foreach (var modData in modMultipliers)
                {
                    string modName = modData.GetString("ModName");
                    float multiplier = modData.GetFloat("Multiplier");
                    _savedModMultipliers[modName] = multiplier;
                }
            }
            else
            {
                _savedModMultipliers = new Dictionary<string, float>(); // 默认值
            }

            // 加载原版倍率
            if (tag.ContainsKey("VanillaDamageMultiplier"))
            {
                _savedVanillaMultiplier = tag.GetFloat("VanillaDamageMultiplier");
            }
            else
            {
                _savedVanillaMultiplier = 1.0f; // 默认值
            }

            // 应用保存的值
            ModDamageMultipliers = new Dictionary<string, float>(_savedModMultipliers);
            VanillaDamageMultiplier = _savedVanillaMultiplier;
        }
    }
}