using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using static Terraria.NPC;
using static ExpansionKele.Content.Items.MentalOmegaProjectile;
using System;
using ExpansionKele.Content.Items;
using Terraria.WorldBuilding;

namespace ExpansionKele.Content.Weapons.Effects
{
    public interface IAntiInfantryModifier
    {
        /// <summary>
        /// 计算对低防御敌人的伤害加成
        /// </summary>
        /// <param name="antiInfantryLevel">AntiInfantry等级 (1/2/3)</param>
        /// <param name="targetDefense">目标防御力</param>
        /// <returns>伤害加成倍数</returns>
        float CalculateLowDefenseBonus(int antiInfantryLevel, int targetDefense);

        /// <summary>
        /// 计算对不同生命值敌人的伤害加成
        /// </summary>
        /// <param name="antiInfantryLevel">AntiInfantry等级 (1/2/3)</param>
        /// <param name="targetLife">目标当前生命值</param>
        /// <param name="targetLifeMax">目标最大生命值</param>
        /// <returns>伤害加成倍数</returns>
        float CalculateLifeBasedBonus(int antiInfantryLevel, int targetLife, int targetLifeMax);
    }

    public class AntiInfantryModifier : IAntiInfantryModifier
    {
        public float CalculateLowDefenseBonus(int antiInfantryLevel, int targetDefense)
        {
            // 防御阈值: 10/15/20
            int[] defenseThresholds = { 0, 10, 15, 20 };
            // 伤害加成: 1.5%/1.8%/2% 每减少1点防御
            float[] damageBonuses = { 0f, 0.015f, 0.018f, 0.02f };

            if (antiInfantryLevel < 1 || antiInfantryLevel > 3)
                return 1f;

            if (targetDefense >= defenseThresholds[antiInfantryLevel])
                return 1f;

            int defenseDiff = defenseThresholds[antiInfantryLevel] - targetDefense;
            return 1f + (defenseDiff * damageBonuses[antiInfantryLevel]);
        }

        public float CalculateLifeBasedBonus(int antiInfantryLevel, int targetLife, int targetLifeMax)
        {
            // 生命值阈值: 2000/4000/8000
            int[] lifeThresholds = { 0, 2000, 4000, 8000 };
            // 低生命值敌人额外伤害: 25%
            float[] lowLifeBonuses = { 0f, 0.25f, 0.25f, 0.25f };
            // 高生命值敌人血量低于50%时的伤害加成: 0.4%/0.45%/0.5% 每降低1%血量
            float[] highLifeBonuses = { 0f, 0.004f, 0.0045f, 0.005f };

            if (antiInfantryLevel < 1 || antiInfantryLevel > 3)
                return 1f;

            // 对低生命值敌人造成额外伤害
            if (targetLifeMax <= lifeThresholds[antiInfantryLevel])
            {
                return 1f + lowLifeBonuses[antiInfantryLevel];
            }
            // 对高生命值敌人，当血量低于50%时，每降低1%血量增加相应伤害
            else if (targetLifeMax > lifeThresholds[antiInfantryLevel] && targetLife < targetLifeMax * 0.5)
            {
                float lifePercentage = (float)targetLife / targetLifeMax;
                float belowThreshold = 0.5f - lifePercentage;
                int percentBelow = (int)(belowThreshold * 100);
                return 1f + (percentBelow * highLifeBonuses[antiInfantryLevel]);
            }

            return 1f;
        }

    }

    


    public interface IAntiArmorModifier
    {
        /// <summary>
        /// 计算对高防御敌人的伤害加成
        /// </summary>
        /// <param name="antiArmorLevel">AntiArmor等级 (1/2/3)</param>
        /// <param name="targetDefense">目标防御力</param>
        /// <returns>伤害加成倍数</returns>
        float CalculateHighDefenseBonus(int antiArmorLevel, int targetDefense);
    }

    // 新增反装甲实现类
    public class AntiArmorModifier : IAntiArmorModifier
    {
        public float CalculateHighDefenseBonus(int antiArmorLevel, int targetDefense)
        {
            // 防御阈值: 35/30/25 (高于此值的敌人)
            int[] defenseThresholds = { 0, 35, 30, 25 };
            // 伤害加成: 1.5%/1.8%/2% 每增加1点防御
            float[] damageBonuses = { 0f, 0.015f, 0.018f, 0.02f };

            if (antiArmorLevel < 1 || antiArmorLevel > 3)
                return 1f;

            if (targetDefense <= defenseThresholds[antiArmorLevel])
                return 1f;

            int defenseDiff = targetDefense - defenseThresholds[antiArmorLevel];
            return 1f + (defenseDiff * damageBonuses[antiArmorLevel]);
        }
    }

    // 新增火炮扩散接口
    public interface IBallisticSplashModifier
    {
        /// <summary>
        /// 计算火炮扩散效果,严格注意modifier继承顺序，吃到所有效果
        /// </summary>
        /// <param name="splashLevel">火炮扩散等级 (1/2/3)</param>
        /// <param name="target">目标NPC</param>
        /// <param name="hitDirection">击中方向</param>
        /// <param name="damage">原始伤害</param>
        void ApplySplashDamage(int splashLevel, NPC target, int hitDirection, int damage,ref NPC.HitModifiers modifiers);
    }

    // 新增火炮扩散实现类
    public class BallisticSplashModifier : IBallisticSplashModifier
    {
        // ... existing code ...
public void ApplySplashDamage(int splashLevel, NPC target, int hitDirection, int damage,ref NPC.HitModifiers modifiers)
        {
            // 半径: 3/4.5/6
            float[] radiuses = { 0f, 3f, 4.5f, 6f };

            if (splashLevel < 1 || splashLevel > 3)
                return;

            float radius = radiuses[splashLevel];
            
            // 遍历所有NPC，对范围内的NPC造成溅射伤害
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc != target)
                {
                    float distance = Vector2.Distance(target.Center, npc.Center);
                    if (distance <= radius * 16)
                    {
                        // 伤害计算: 100/(距离+2)%
                        float damagePercentage = 100f / (distance / 16 + 1f);
                        int splashDamage = (int)(modifiers.FinalDamage.ApplyTo(damage) * damagePercentage / 100f);
                        
                        // 确保至少造成1点伤害
                        if (splashDamage < 1)
                            splashDamage = 1;
                        
                        // 应用伤害
                        npc.StrikeNPC(new NPC.HitInfo()
                        {
                            Damage = splashDamage,
                            Knockback = 0f,
                            HitDirection = hitDirection,
                            Crit = false
                        });
                    }
                }
            }
        }
// ... existing code ...
    }

    // ... existing code ...
    // 新增反空军接口
    public interface IAntiAirForceModifier
    {
        /// <summary>
        /// 计算对空军单位的伤害加成
        /// </summary>
        /// <param name="antiAirForceLevel">AntiAirForce等级 (1/2/3)</param>
        /// <param name="isOnlyAntiAirForce">是否仅为反空军武器</param>
        /// <param name="target">目标NPC</param>
        /// <returns>伤害加成倍数</returns>
        float CalculateAirForceBonus(int antiAirForceLevel, bool isOnlyAntiAirForce, NPC target);
    }

    // 新增反空军实现类
    public class AntiAirForceModifier : IAntiAirForceModifier
    {
        public float CalculateAirForceBonus(int antiAirForceLevel, bool isOnlyAntiAirForce, NPC target)
        {
            // 判断是否为空中单位（Y轴运动速度不为0）
            bool isAirUnit = target.velocity.Y != 0;

            if (!isOnlyAntiAirForce)
            {
                // isOnlyAntiAirForce为false时
                // AntiAirForce=1/2/3:对空中单位（定义为Y轴运动速度不是0的生物）伤害额外增加20/30/40%
                float[] airBonuses = { 0f, 0.2f, 0.3f, 0.4f };

                if (antiAirForceLevel < 1 || antiAirForceLevel > 3)
                    return 1f;

                if (isAirUnit)
                {
                    return 1f + airBonuses[antiAirForceLevel];
                }
                return 1f;
            }
            else
            {
                // isOnlyAntiAirForce为true时
                // AntiAirForce=1/2/3:仅对非空中单位只造成50/55/60%的伤害，但是对空中目标可以造成170/185/200%的伤害
                float[] groundDamagePercentages = { 0f, 0.5f, 0.55f, 0.6f };
                float[] airDamageBonuses = { 0f, 0.7f, 0.85f, 1.0f };

                if (antiAirForceLevel < 1 || antiAirForceLevel > 3)
                    return 1f;

                if (isAirUnit)
                {
                    return 1f + airDamageBonuses[antiAirForceLevel];
                }
                else
                {
                    return groundDamagePercentages[antiAirForceLevel];
                }
            }
        }
    }
// ... existing code ...

// ... existing code ...
    // 新增反建筑接口
    public interface IAntiBuildingModifier
    {
        /// <summary>
        /// 计算对建筑单位的伤害加成
        /// </summary>
        /// <param name="antiBuildingLevel">AntiBuilding等级 (1/2/3)</param>
        /// <param name="isOnlyAntiBuilding">是否仅为反建筑武器</param>
        /// <param name="target">目标NPC</param>
        /// <returns>伤害加成倍数</returns>
        float CalculateBuildingBonus(int antiBuildingLevel, bool isOnlyAntiBuilding, NPC target);
        
        /// <summary>
        /// 计算弹速减少比例（仅适用于仅反建筑武器）
        /// </summary>
        /// <param name="antiBuildingLevel">AntiBuilding等级 (1/2/3)</param>
        /// <param name="isOnlyAntiBuilding">是否仅为反建筑武器</param>
        /// <returns>弹速减少比例</returns>
        float CalculateProjectileSpeedReduction(int antiBuildingLevel, bool isOnlyAntiBuilding);
    }

    // 新增反建筑实现类
    // ... existing code ...
    // 新增反建筑实现类
    public class AntiBuildingModifier : IAntiBuildingModifier
    {
        public float CalculateBuildingBonus(int antiBuildingLevel, bool isOnlyAntiBuilding, NPC target)
        {
            // 计算目标NPC的碰撞箱面积（宽*高），每20*20像素为一个单位
            int area = target.width * target.height;
            float units = area / (20f * 20f); // 计算20*20像素单位数量
            
            if (isOnlyAntiBuilding)
            {
                // isOnlyAntiBuilding=true时
                // AntiBuilding=1/2/3：根据碰撞箱对该敌人造成额外伤害，每20*20个像素点额外造成50%/75%/100%伤害
                float[] damageBonuses = { 0f, 0.5f, 0.75f, 1.0f };
                
                if (antiBuildingLevel < 1 || antiBuildingLevel > 3)
                    return 1f;
                
                return 1f + (units * damageBonuses[antiBuildingLevel]);
            }
            else
            {
                // isOnlyAntiBuilding=false时
                // AntiBuilding=1/2/3:每20*20个像素点额外造成只增加20%/25%/30%的伤害
                float[] damageBonuses = { 0f, 0.2f, 0.25f, 0.3f };
                
                if (antiBuildingLevel < 1 || antiBuildingLevel > 3)
                    return 1f;
                
                return 1f + (units * damageBonuses[antiBuildingLevel]);
            }
        }
        
        public float CalculateProjectileSpeedReduction(int antiBuildingLevel, bool isOnlyAntiBuilding)
        {
            if (!isOnlyAntiBuilding)
                return 1f; // 非仅反建筑武器不减少弹速
                
            // isOnlyAntiBuilding=true时，弹速减少60%/55%/50%
            float[] speedReductions = { 0f, 0.6f, 0.55f, 0.5f };
            
            if (antiBuildingLevel < 1 || antiBuildingLevel > 3)
                return 1f;
                
            return 1f - speedReductions[antiBuildingLevel];
        }
    }
// ... existing code ...
// ... existing code ...

    

    // ... existing code ...
    // ... existing code ...
    public static class MentalOmegaUtils
    {
        private static IAntiInfantryModifier _antiInfantryModifier = new AntiInfantryModifier();
        private static IAntiArmorModifier _antiArmorModifier = new AntiArmorModifier();
        private static IBallisticSplashModifier _ballisticSplashModifier = new BallisticSplashModifier();
        private static IAntiAirForceModifier _antiAirForceModifier = new AntiAirForceModifier();
        private static IAntiBuildingModifier _antiBuildingModifier = new AntiBuildingModifier();

        public static void ModifyHitNPCAgainstInfantry(NPC target, ref NPC.HitModifiers modifiers, OmegaLevel antiInfantryLevel)
        {
            // 获取AntiInfantry等级数值 (1, 2, 3)
            int antiInfantryLevelInt = (int)antiInfantryLevel;

            // 计算对低防御敌人的伤害加成
            float lowDefenseBonus = _antiInfantryModifier.CalculateLowDefenseBonus(antiInfantryLevelInt, target.defense);

            // 计算基于生命值的伤害加成
            float lifeBasedBonus = _antiInfantryModifier.CalculateLifeBasedBonus(antiInfantryLevelInt, target.life, target.lifeMax);

            // 应用伤害修饰
            modifiers.FinalDamage *= lowDefenseBonus * lifeBasedBonus;
            
        }

        // 新增反装甲伤害修饰方法
        public static void ModifyHitNPCAgainstArmor(NPC target, ref NPC.HitModifiers modifiers, OmegaLevel antiArmorLevel)
        {
            // 获取AntiArmor等级数值 (1, 2, 3)
            int antiArmorLevelInt = (int)antiArmorLevel;

            // 计算对高防御敌人的伤害加成
            float highDefenseBonus = _antiArmorModifier.CalculateHighDefenseBonus(antiArmorLevelInt, target.defense);

            // 应用伤害修饰
            modifiers.FinalDamage *= highDefenseBonus;
        }

        // 新增火炮扩散效果应用方法
        
        
        // 新增反空军伤害修饰方法
        public static void ModifyHitNPCAgainstAirForce(NPC target, ref NPC.HitModifiers modifiers, OmegaLevel antiAirForceLevel, bool isOnlyAntiAirForce)
        {
            // 获取AntiAirForce等级数值 (1, 2, 3)
            int antiAirForceLevelInt = (int)antiAirForceLevel;

            // 计算对空军单位的伤害加成
            float airForceBonus = _antiAirForceModifier.CalculateAirForceBonus(antiAirForceLevelInt, isOnlyAntiAirForce, target);

            // 应用伤害修饰
            modifiers.FinalDamage *= airForceBonus;
        }
        
        // 新增反建筑伤害修饰方法
        public static void ModifyHitNPCAgainstBuilding(NPC target, ref NPC.HitModifiers modifiers, OmegaLevel antiBuildingLevel, bool isOnlyAntiBuilding)
        {
            // 获取AntiBuilding等级数值 (1, 2, 3)
            int antiBuildingLevelInt = (int)antiBuildingLevel;

            // 计算对建筑单位的伤害加成
            float buildingBonus = _antiBuildingModifier.CalculateBuildingBonus(antiBuildingLevelInt, isOnlyAntiBuilding, target);

            // 应用伤害修饰
            modifiers.FinalDamage *= buildingBonus;
        }
        
        // 获取反建筑武器的弹速减少比例
        public static float GetBuildingProjectileSpeedReduction(OmegaLevel antiBuildingLevel, bool isOnlyAntiBuilding)
        {
            // 获取AntiBuilding等级数值 (1, 2, 3)
            int antiBuildingLevelInt = (int)antiBuildingLevel;
            
            // 计算弹速减少比例
            return _antiBuildingModifier.CalculateProjectileSpeedReduction(antiBuildingLevelInt, isOnlyAntiBuilding);
        }
        public static void ApplyBallisticSplash(NPC target, ref NPC.HitModifiers modifiers,int hitDirection, int damage, OmegaLevel splashLevel)
        {
            int splashLevelInt = (int)splashLevel;
            
            _ballisticSplashModifier.ApplySplashDamage(splashLevelInt, target, hitDirection, damage,ref modifiers);
        }
        public static void Newly(){}

        // ... existing code ...
        public static void MentalOmegaModifyHit(NPC target, ref NPC.HitModifiers modifiers, OmegaLevel AntiInfantry, OmegaLevel AntiArmor, OmegaLevel AntiAirForce, bool isOnlyAntiAirForce, OmegaLevel AntiBuilding, bool isOnlyAntiBuilding, int hitDirection, int damage)
{
    MentalOmegaUtils.ModifyHitNPCAgainstInfantry(target, ref modifiers, AntiInfantry);
    MentalOmegaUtils.ModifyHitNPCAgainstArmor(target, ref modifiers, AntiArmor);
    MentalOmegaUtils.ModifyHitNPCAgainstAirForce(target, ref modifiers, AntiAirForce, isOnlyAntiAirForce);
    MentalOmegaUtils.ModifyHitNPCAgainstBuilding(target, ref modifiers, AntiBuilding, isOnlyAntiBuilding);
    MentalOmegaUtils.ApplyBallisticSplash(target, ref modifiers, hitDirection, damage, AntiArmor); 
}

        // 新增简化版的MentalOmegaModifyHit方法，接受更少的参数
        public static void MentalOmegaModifyHit(MentalOmegaProjectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            MentalOmegaUtils.ModifyHitNPCAgainstInfantry(target, ref modifiers, projectile.AntiInfantry);
            MentalOmegaUtils.ModifyHitNPCAgainstArmor(target, ref modifiers, projectile.AntiArmor);
            MentalOmegaUtils.ModifyHitNPCAgainstAirForce(target, ref modifiers, projectile.AntiAirForce, projectile.isOnlyAntiAirForce);
            MentalOmegaUtils.ModifyHitNPCAgainstBuilding(target, ref modifiers, projectile.AntiBuilding, projectile.isOnlyAntiBuilding);
            MentalOmegaUtils.ApplyBallisticSplash(target,  ref modifiers,projectile.Projectile.direction, projectile.Projectile.damage, projectile.AntiArmor);
        }//
// ... existing code ...

    }
// ... existing code ...
// ... existing code ...



}