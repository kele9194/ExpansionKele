using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele
{
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
    }
}