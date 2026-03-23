
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using ExpansionKele.Content.Customs.Commands;
using ExpansionKele.Content.Customs;

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
         /// <summary>
        /// 修改弹幕击中 NPC 时的伤害
        /// 特别处理召唤物弹幕的乘算伤害加成
        /// 解决 ModifyWeaponDamage 无法为召唤武器的弹幕提供伤害的问题
        /// </summary>
        /// <param name="proj">击中 NPC 的弹幕</param>
        /// <param name="target">被击中的 NPC</param>
        /// <param name="modifiers">伤害修饰符引用</param>
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            SummonDamageHelper.ApplyFlatBonusMultiplierToSummon(proj, ref modifiers, DamageFlatBonus);
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