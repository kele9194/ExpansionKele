// using System;
// using System.Collections.Generic;
// using Terraria;
// using Terraria.ModLoader;

// namespace ExpansionKele.Content.Customs.ECShield
// {
//     /// <summary>
//     /// 护盾状态枚举
//     /// </summary>
//     public enum ShieldState
//     {
//         Broken,     // 破碎
//         Cooldown,   // 冷却
//         InRegen,    // 恢复中
//         Full        // 满护盾
//     }

//     /// <summary>
//     /// 护盾核心数据类
//     /// </summary>
//     public class ShieldCore
//     {
//         // 护盾核心属性
//         public StatModifier MaxShieldModifier= new StatModifier(); // 护盾容量上限修饰符
//         public float MaxShieldBase = 10f; // 基础护盾容量
// // ... existing code ...
//         public float MaxShield => MaxShieldModifier.ApplyTo(MaxShieldBase); // 护盾容量上限

//         public float CurrentShield  = 0f;        // 动态变化的核心值
//         public StatModifier ShieldRegenModifier = new StatModifier(); // 恢复率修饰符
//         public float ShieldRegenBase = 1f;
//         public float ShieldRegen => ShieldRegenModifier.ApplyTo(ShieldRegenBase); // 每秒自然恢复的量（基于最大护盾值的百分比）
//         public int ShieldRegenDelay = 75;             // 护盾>0时，受击后到开始恢复的等待时间（帧）
//         public int ShieldRegenDelayBroken = 300;       // 护盾=0时，受击后到开始恢复的等待时间（帧）
//         public ShieldState ShieldState = ShieldState.Broken; // 驱动状态机
//         public bool ShieldActive = false;       // 护盾是否激活可用
// // ... existing code ...

//         // 护盾参数
//         public StatModifier DamageAbsorptionModifier = new StatModifier(); // 伤害吸收修饰符
//         public float DamageAbsorptionRatioBase = 1.0f;
//         public float DamageAbsorptionRatio => DamageAbsorptionModifier.ApplyTo(DamageAbsorptionRatioBase); // 伤害吸收比例（1.0 = 1:1抵消）
//         public StatModifier ShieldStrengthModifier = new StatModifier(); // 护盾强度修饰符
//         public float ShieldStrengthBase = 1.0f;
//         public float ShieldStrength => ShieldStrengthModifier.ApplyTo(ShieldStrengthBase); // 护盾强度乘数，影响所有吸收效果
//         public float BreakThreshold = 0.0f;        // 破碎阈值，护盾值低于等于此值视为破碎
//         public float VisualAlpha = 0.7f;           // 护盾可视化透明度（0-1）
//         public float VisualScale = 1.2f;           // 护盾可视化大小乘数
//         public float HueShift = 0.0f;              // 色调偏移（用于视觉区分）

//         // 运行时状态
//         public bool IsBroken = true;            // 护盾当前是否处于破碎状态
//         public int BreakCooldown = 0;           // 破碎冷却剩余时间（帧）
//         public List<float> RecentHits = new List<float>(); // 近期受击记录
//         public bool OnCooldown = false;         // 护盾是否处于冷却中
//         public float OverloadCharge = 0.0f;     // 过载充能（0-100）
//         public float DamageAbsorbedThisCycle = 0.0f; // 当前周期吸收的伤害量
        
//         // 内部状态
//         public bool IsRegenerating = false;     // 是否正在恢复
//         public int TimeSinceLastHit = 0;        // 距离上次受击的时间（帧）
//         public int RegenStartTime = 0;          // 开始恢复的时间点
//         public int ShieldToughness = 0;         // 护盾韧性，减少某些控制效果

//         // 内部计时器
//         public int LastHitFrame = 0;
//         private float _damageBuffer = 0f; // 伤害缓冲区，用于处理伤害减免
//         // 添加一个标志来跟踪是否应该完全闪避伤害
  

//         public void ResetShield()
//         {
//             //CurrentShield = MaxShield;
//             IsBroken = false;
//             IsRegenerating = false;
//             OnCooldown = false;
//             BreakCooldown = 0;
//             ShieldState = ShieldState.Full;
//             RecentHits.Clear();
//             // 重置所有修饰符
//             MaxShieldModifier = new StatModifier();
//             ShieldRegenModifier = new StatModifier();
//             DamageAbsorptionModifier = new StatModifier();
//             ShieldStrengthModifier = new StatModifier();
//         }

//         public void ActivateShield()
//         {
//             ShieldActive = true;
//         }

//         public void DeactivateShield()
//         {
//             ShieldActive = false;
//         }

//         public void ApplyModifiers()
//         {
//             // 由于不再使用接口，这个方法可以移除或保留空实现
//             // 目前保留空实现以避免其他代码出错
//         }
// }
// }