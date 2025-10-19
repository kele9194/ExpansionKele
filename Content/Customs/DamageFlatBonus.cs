
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ExpansionKele.Content.Items.Accessories
{
public class DamageFlatBonusPlayer : ModPlayer
    {
        public int DamageFlatBonus = 0;

        public override void ResetEffects()
        {
            DamageFlatBonus = 0;
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
                damage.Flat += DamageFlatBonus;
        }
    }
    // ... existing code ...
public class DamageFlatBonusRanger : ModPlayer
    {
        public int DamageFlatBonus = 0;

        public override void ResetEffects()
        {
            DamageFlatBonus = 0;
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (item.DamageType == DamageClass.Ranged || item.DamageType.CountsAsClass(DamageClass.Ranged))
            {
                damage.Flat += DamageFlatBonus;
            }
        }
    }
// ... existing code ...
}