using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs.ECShield;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System.Collections.Generic;

// ... existing code ...
// ... existing code ...
public class ECShieldSystem : ModPlayer
    {
    public enum ShieldState
    {
        Broken,     // 破碎
        Cooldown,   // 冷却
        InRegen,    // 恢复中
        Full        // 满护盾
    }

        // 护盾核心属性 - 直接在ECShieldSystem中定义
        public StatModifier MaxShieldModifier = new StatModifier(); // 护盾容量上限修饰符
        public float MaxShieldBase = 0f; // 基础护盾容量
        public float MaxShield => MaxShieldModifier.ApplyTo(MaxShieldBase); // 护盾容量上限

        public float CurrentShield = 0f;        // 动态变化的核心值
        public StatModifier ShieldRegenModifier = new StatModifier(); // 恢复率修饰符
        public float ShieldRegenBase = 0f;
        public float ShieldRegen => ShieldRegenModifier.ApplyTo(ShieldRegenBase); // 每秒自然恢复的量（基于最大护盾值的百分比）
        public int ShieldRegenDelay = 150;             // 护盾>0时，受击后到开始恢复的等待时间（帧）
        public int ShieldRegenDelayBroken = 150;       // 护盾=0时，受击后到开始恢复的等待时间（帧）
        public ShieldState CurrentShieldState = ShieldState.Broken; // 驱动状态机
        public bool ShieldActive = false;       // 护盾是否激活可用

        // 护盾参数
        public StatModifier DamageAbsorptionModifier = new StatModifier(); // 伤害吸收修饰符
        public float DamageAbsorptionRatioBase = 1.0f;
        public float DamageAbsorptionRatio => DamageAbsorptionModifier.ApplyTo(DamageAbsorptionRatioBase); // 伤害吸收比例（1.0 = 1:1抵消）
        public StatModifier ShieldStrengthModifier = new StatModifier(); // 护盾强度修饰符
        public float ShieldStrengthBase = 1.0f;
        public float ShieldStrength => ShieldStrengthModifier.ApplyTo(ShieldStrengthBase); // 护盾强度乘数，影响所有吸收效果
        public float BreakThreshold = 0.0f;        // 破碎阈值，护盾值低于等于此值视为破碎
        public float VisualAlpha = 0.7f;           // 护盾可视化透明度（0-1）
        public float VisualScale = 1.2f;           // 护盾可视化大小乘数
        public float HueShift = 0.0f;              // 色相偏移（用于视觉区分）

        // 运行时状态
        public bool IsBroken = true;            // 护盾当前是否处于破碎状态
        public int BreakCooldown = 0;           // 破碎冷却剩余时间（帧）
        public List<float> RecentHits = new List<float>(); // 近期受击记录
        public bool OnCooldown = false;         // 护盾是否处于冷却中
        public float OverloadCharge = 0.0f;     // 过载充能（0-100）
        public float DamageAbsorbedThisCycle = 0.0f; // 当前周期吸收的伤害量
        
        // 内部状态
        public bool IsRegenerating = false;     // 是否正在恢复
        public int TimeSinceLastHit = 0;        // 距离上次受击的时间（帧）
        public int RegenStartTime = 0;          // 开始恢复的时间点
        public int ShieldToughness = 0;         // 护盾韧性，减少某些控制效果

        // 内部计时器
        public int LastHitFrame = 0;
        private float _damageBuffer = 0f; // 伤害缓冲区，用于处理伤害减免
        // 添加一个标志来跟踪是否应该完全闪避伤害
        private bool _shouldDodgeNextHit = false;

        private ShieldStateManagement _stateManagement;

        public int UnknownMultiplier = 2;

        public ECShieldSystem()
        {
            _stateManagement = new ShieldStateManagement(this); // 现在传入ECShieldSystem本身
        }

        /// <summary>
        /// 重置护盾效果，每次玩家更新时调用
        /// </summary>
        public override void ResetEffects()
        {
            bool wasActive = ShieldActive;
            ResetShield();
            ShieldActive = false; // 重置时禁用护盾，需要通过装备等激活

            // 如果护盾之前是激活的而现在不是，隐藏UI
            if (wasActive && !ShieldActive)
            {
            }
        }

        /// <summary>
        /// 玩家加载时的初始化方法
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            // 玩家进入游戏时启动恢复计时器
        }

        /// <summary>
        /// 预更新方法，在玩家更新前处理护盾逻辑
        /// 更新护盾状态、再生、计时器等
        /// </summary>
        public override void PreUpdate()
        {

            if (!ShieldActive)
            {
                return;
            }

            _stateManagement.UpdateShieldState();
            _stateManagement.UpdateShieldRegeneration();
            _stateManagement.UpdateTimers();
        }

        /// <summary>
        /// 处理玩家受到伤害时的护盾减免
        /// 在玩家受伤时调用，处理护盾对伤害的吸收
        /// </summary>
        /// 
        /// 默认先处理modifyHit在处理modifyhurt
        /// 然后再处理dodge和Onhurt,Posthurt
        /// 判定这些通过HurtModifier=>HurtInfo
        

                public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            float Projectilemodifier = UnknownMultiplier*Main.GameModeInfo.EnemyDamageMultiplier;
            float incomingDamage = proj.damage*Projectilemodifier;
            ProcessShieldDamage(incomingDamage, ref modifiers);
        }

        /// <summary>
        /// 处理玩家被NPC击中时的护盾减免
        /// </summary>
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            float incomingDamage = npc.damage;
            ProcessShieldDamage(incomingDamage, ref modifiers);
        }
         /// <summary>
        /// 处理护盾对伤害的减免逻辑
        /// </summary>
        private void ProcessShieldDamage(float incomingDamage, ref Player.HurtModifiers modifiers)
        {
            if (!ShieldActive || CurrentShield <= 0)
            {
                return;
            }
            if (CurrentShield >= incomingDamage)
            {
                // 护盾值大于等于伤害，标记为完全闪避，但不立即处理
                _shouldDodgeNextHit = true;
                CurrentShield -= incomingDamage;
                DamageAbsorbedThisCycle += incomingDamage;
                
                // 更新上次受击时间
                LastHitFrame = (int)Main.GameUpdateCount;
                OnCooldown = true;
                
                // 记录本次伤害
                RecentHits.Add(incomingDamage);
            }
            else
            {
                // 护盾值小于伤害，从伤害中减去部分护盾值
                modifiers.SourceDamage.Base -= CurrentShield;
                DamageAbsorbedThisCycle += CurrentShield;
                
                // 护盾值归零
                CurrentShield = 0;
                
                // 更新上次受击时间
                LastHitFrame = (int)Main.GameUpdateCount;
                OnCooldown = true;
                
                // 记录本次伤害
                RecentHits.Add(incomingDamage);
            }
        }



        // ... existing code ...
        /// <summary>
        /// 完全闪避伤害
        /// </summary>
        // ... existing code ...
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!ShieldActive || CurrentShield <= 0)
            {
                return base.FreeDodge(info);
            }

            
            if (_shouldDodgeNextHit)
            {
                _shouldDodgeNextHit = false; // 重置标志
                
                // 给予30帧无敌时间
                Player.immune = true;
                Player.immuneTime = 10;
                
                return true; // 完全闪避成功
            }
            
            return base.FreeDodge(info); // 无法完全闪避
        }
        /// <summary>
        /// 重置护盾到初始状态
        /// </summary>
        public void ResetShield()
        {
            //CurrentShield = MaxShield;
            IsBroken = false;
            IsRegenerating = false;
            OnCooldown = false;
            BreakCooldown = 0;
            //CurrentShieldState = ShieldState.Full;
            RecentHits.Clear();
            // 重置所有修饰符
            MaxShieldModifier = new StatModifier();
            ShieldRegenModifier = new StatModifier();
            DamageAbsorptionModifier = new StatModifier();
            ShieldStrengthModifier = new StatModifier();
        }

        /// <summary>
        /// 激活护盾
        /// </summary>
        public void ActivateShield()
        {
            ShieldActive = true;
        }

        /// <summary>
        /// 关闭护盾
        /// </summary>
        public void DeactivateShield()
        {
            ShieldActive = false;
        }

        /// <summary>
        /// 应用护盾的属性修正
        /// </summary>
        public void ApplyModifiers()
        {
            // 由于不再使用接口，这个方法可以移除或保留空实现
            // 目前保留空实现以避免其他代码出错
        }
    }
// ... existing code ...