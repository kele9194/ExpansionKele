using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExpansionKele.Content.Armor.StarArmorA;
using System;
namespace ExpansionKele.Content.Armor
{
    public static class ArmorData
    {
        public static  int[] HelmetDefense={ 4, 5, 6,  7,9,9,11   ,13,17,20 }; 
        public static  int[] PlateDefense={ 5, 6, 9,  10,11,12,13   ,14,16,24 };
        
        public static  int[] LeggingsDefense = { 4, 5, 7,  8,9,10,11   ,14,18,21 }; // 防御值
        public static float[] GenericDamageBonus = {6,8,12,  14,17,21,25,   28,31,36}; // 伤害加成
        public static  int[] CritChance = {5,5,6,  8,10,11,12,   14,16,18 }; // 暴击加成
        public static  float[] MoveSpeedBonus = {5,8,11,  14,17,20,23,   26,29,32 }; // 移速加成
        public static  int[] MeleeCritChance = {3,4,5,  6,7,8,9,   10,11,12}; // 近战暴击加成
        public static  int[] RangedCritChance = {2,2,3,  3,4,4,4,   5,6,6}; // 远程暴击加成
        public static  int[] SummonDamage = RangedCritChance; // 召唤伤害加成
        public static  int[] RogueCritChance = MeleeCritChance; // 盗贼暴击加成
        public static  float[] MeleeSpeed = {6,8,10,  12,13,15,16,   18,19,21}; // 近战攻速加成
        public static  int[] MaxMinions = {1,1,2,  2,2,3,3,   4,4,5}; // 最大仆从数加成
        public static  int[] MaxTurrets = {0,0,0,  1,1,1,1,   1,1,2}; // 最大哨兵数加成
        public static  int[] MaxMana = {20,20,40,  40,60,60,80,   80,100,100}; // 最大魔力值加成
        public static  float[] ManaCostReduction = {5,6,7,  8,9,10,11,   12,13,14}; // 魔力减耗
        public static  float[] AmmoCostReduction = {3,5,7,  9,12,15,18,   21,24,27}; // 弹药减耗
        public static  int[] StealthMax = {50,57,64,  71,78,85,92,   99,106,113}; // 潜伏值
        public static  int[] WhipRange={3,6,9,  12,14,16,18,   20,22,24};//鞭子范围

        public static float CalculateA(float t)
        {
            float discriminant = (float)Math.Sqrt(1 + 4f / t);
            float a1 = (-1f + discriminant) / 2f;
            
            // 根据实际情况选择合适的解
            return a1 ;
        }
        
        
    }
}

public class ArmorLogic
{
    public  bool IsCritEqualMelee_Rogue=true;
    public bool IsCritEqualRanged_Summon=true;
    public void ApplyArmorEffects()
    {
        if (IsCritEqualMelee_Rogue)
        {
            // 你的逻辑代码
        }

        if (IsCritEqualRanged_Summon)
        {
            // 你的逻辑代码
        }
    }
}

