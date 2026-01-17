using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele
{
    // ... existing code ...
    public static class ReflectionHelper
    {
        public static void ApplyRogueStealth(Player player, float rogueStealthMax)
        {
            if (ExpansionKele.calamity != null)
            {
                var calamityPlayerType = player.GetModPlayer(ExpansionKele.calamity.Find<ModPlayer>("CalamityPlayer"));
                if (calamityPlayerType != null)
                {
                    // 获取 rogueStealthMax 字段
                    FieldInfo rogueStealthMaxField = calamityPlayerType.GetType().GetField("rogueStealthMax", BindingFlags.Public | BindingFlags.Instance);
                    if (rogueStealthMaxField != null)
                    {
                        // 获取当前值并增加 rogueStealthMax
                        float currentValue = (float)rogueStealthMaxField.GetValue(calamityPlayerType);
                        rogueStealthMaxField.SetValue(calamityPlayerType, currentValue + rogueStealthMax);
                    }

                    // 获取 wearingRogueArmor 字段
                    FieldInfo wearingRogueArmorField = calamityPlayerType.GetType().GetField("wearingRogueArmor", BindingFlags.Public | BindingFlags.Instance);
                    if (wearingRogueArmorField != null)
                    {
                        // 设置为 true
                        wearingRogueArmorField.SetValue(calamityPlayerType, true);
                    }
                }
            }
        }

        public static float GetStealthGenStandstill(Player player)
        {
            if (ExpansionKele.calamity != null)
            {
                var calamityPlayerType = player.GetModPlayer(ExpansionKele.calamity.Find<ModPlayer>("CalamityPlayer"));
                if (calamityPlayerType != null)
                {
                    // 获取 stealthGenStandstill 字段
                    FieldInfo stealthGenStandstillField = calamityPlayerType.GetType().GetField("stealthGenStandstill", BindingFlags.Public | BindingFlags.Instance);
                    if (stealthGenStandstillField != null)
                    {
                        return (float)stealthGenStandstillField.GetValue(calamityPlayerType);
                    }
                }
            }
            return 0f;
        }

        public static float GetStealthGenMoving(Player player)
        {
            if (ExpansionKele.calamity != null)
            {
                var calamityPlayerType = player.GetModPlayer(ExpansionKele.calamity.Find<ModPlayer>("CalamityPlayer"));
                if (calamityPlayerType != null)
                {
                    // 获取 stealthGenMoving 字段
                    FieldInfo stealthGenMovingField = calamityPlayerType.GetType().GetField("stealthGenMoving", BindingFlags.Public | BindingFlags.Instance);
                    if (stealthGenMovingField != null)
                    {
                        return (float)stealthGenMovingField.GetValue(calamityPlayerType);
                    }
                }
            }
            return 0f;
        }
        public static void SetStealthGenStandstill(Player player, float value)
        {
            if (ExpansionKele.calamity != null)
            {
                var calamityPlayerType = player.GetModPlayer(ExpansionKele.calamity.Find<ModPlayer>("CalamityPlayer"));
                if (calamityPlayerType != null)
                {
                    // 设置 stealthGenStandstill 字段
                    FieldInfo stealthGenStandstillField = calamityPlayerType.GetType().GetField("stealthGenStandstill", BindingFlags.Public | BindingFlags.Instance);
                    if (stealthGenStandstillField != null)
                    {
                        stealthGenStandstillField.SetValue(calamityPlayerType, value);
                    }
                }
            }
        }

        public static void SetStealthGenMoving(Player player, float value)
        {
            if (ExpansionKele.calamity != null)
            {
                var calamityPlayerType = player.GetModPlayer(ExpansionKele.calamity.Find<ModPlayer>("CalamityPlayer"));
                if (calamityPlayerType != null)
                {
                    // 设置 stealthGenMoving 字段
                    FieldInfo stealthGenMovingField = calamityPlayerType.GetType().GetField("stealthGenMoving", BindingFlags.Public | BindingFlags.Instance);
                    if (stealthGenMovingField != null)
                    {
                        stealthGenMovingField.SetValue(calamityPlayerType, value);
                    }
                }
            }
        }
    }
// ... existing code ...
}