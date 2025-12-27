using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 使用 GlobalItem 来修改所有武器的伤害
    /// </summary>
    public class HandHeldGlobalItem : GlobalItem
    {
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if(ExpansionKeleConfig.Instance.EnableGlobalDamageMultiplierModification)
            {
                // 应用mod特定或原版物品的伤害倍率
                if (item.ModItem != null)
                {
                    // 这是mod物品，检查是否有该mod的特定倍率
                    string modName = item.ModItem.Mod.Name;
                    if (HandHeldSystem.ModDamageMultipliers.ContainsKey(modName))
                    {
                        damage *= HandHeldSystem.ModDamageMultipliers[modName];
                    }
                }
                else
                {
                    // 这是原版物品，应用原版物品倍率
                    damage *= HandHeldSystem.VanillaDamageMultiplier;
                }
            }
        }
    }
}