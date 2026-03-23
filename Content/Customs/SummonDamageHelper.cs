// ... existing code ...
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
// ... existing code ...
namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 召唤物弹幕伤害辅助工具类，提供统一的召唤物弹幕伤害修正逻辑
    /// </summary>
    public static class SummonDamageHelper
    {
        /// <summary>
        /// 验证是否为有效的召唤物弹幕
        /// </summary>
        /// <param name="proj">要验证的弹幕</param>
        /// <returns>如果是有效的召唤物弹幕返回 true，否则返回 false</returns>
        public static bool IsValidSummonProjectile(Projectile proj)
        {
            if(proj.DamageType == DamageClass.Summon)
            {
                if(Main.projPet[proj.type]||ProjectileID.Sets.MinionShot[proj.type]||ProjectileID.Sets.SentryShot[proj.type]){
                    return true;
                }
                return false;
            }
            return false;
        }
        public static void ApplyGlobalMultiplierToSummon(Projectile proj, ref NPC.HitModifiers modifiers, float globalMultiplier, bool enabled)
        {
            if (!IsValidSummonProjectile(proj))
            {
                return;
            }

            if (enabled)
            {
                modifiers.FinalDamage *= globalMultiplier;
            }
        }

        

        /// <summary>
        /// 应用固定伤害加成到召唤物弹幕
        /// </summary>
        /// <param name="proj">击中 NPC 的弹幕</param>
        /// <param name="modifiers">伤害修饰符引用</param>
        /// <param name="flatBonus">固定伤害加成值</param>
        public static void ApplyFlatBonusToSummon(Projectile proj, ref NPC.HitModifiers modifiers, int flatBonus)
        {
            if (!IsValidSummonProjectile(proj))
            {
                return;
            }

            if (flatBonus != 0)
            {
                modifiers.FinalDamage.Flat += flatBonus;
            }
        }

        /// <summary>
        /// 应用乘法伤害加成到召唤物弹幕
        /// </summary>
        /// <param name="proj">击中 NPC 的弹幕</param>
        /// <param name="modifiers">伤害修饰符引用</param>
        /// <param name="multiplicativeBonus">乘法伤害加成值</param>
        public static void ApplyMultiplicativeBonusToSummon(Projectile proj, ref NPC.HitModifiers modifiers, float multiplicativeBonus)
        {
            if (!IsValidSummonProjectile(proj))
            {
                return;
            }

            if (multiplicativeBonus != 1f)
            {
                modifiers.FinalDamage *= multiplicativeBonus;
            }
        }

        /// <summary>
        /// 应用固定伤害加成（乘法）到召唤物弹幕
        /// </summary>
        /// <param name="proj">击中 NPC 的弹幕</param>
        /// <param name="modifiers">伤害修饰符引用</param>
        /// <param name="flatBonus">固定伤害加成值</param>
        public static void ApplyFlatBonusMultiplierToSummon(Projectile proj, ref NPC.HitModifiers modifiers, int flatBonus)
        {
            if (!IsValidSummonProjectile(proj))
            {
                return;
            }

            if (flatBonus != 0)
            {
                modifiers.FinalDamage.Flat += flatBonus;
            }
        }
    }
}