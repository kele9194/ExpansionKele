using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs.ECShield;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// ECShield护盾系统 - 一个特殊的前置防御减伤机制
    /// 提供护盾的管理、状态更新、伤害处理等功能
    /// </summary>
    public class ECShieldSystem : ModPlayer
    {
        private ShieldCore _core = new ShieldCore();
        private ShieldStateManagement _stateManagement;

        // 添加一个标志来跟踪是否应该完全闪避伤害
        private bool _shouldDodgeNextHit = false;

        public int UnknownMultiplier => 2;

        public ECShieldSystem()
        {
            _stateManagement = new ShieldStateManagement(_core);
        }

        /// <summary>
        /// 重置护盾效果，每次玩家更新时调用
        /// </summary>
        public override void ResetEffects()
        {
            bool wasActive = _core.ShieldActive;
            _core.ResetShield();
            _core.ShieldActive = false; // 重置时禁用护盾，需要通过装备等激活

            // 如果护盾之前是激活的而现在不是，隐藏UI
            if (wasActive && !_core.ShieldActive)
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

            if (!_core.ShieldActive)
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
            Main.NewText($"{incomingDamage}");
            Main.NewText($"{modifiers.SourceDamage.Base},{modifiers.SourceDamage.Additive},{modifiers.SourceDamage.Multiplicative},{modifiers.SourceDamage.Flat}");
            Main.NewText($"{modifiers.FinalDamage.Base},{modifiers.FinalDamage.Additive},{modifiers.FinalDamage.Multiplicative},{modifiers.FinalDamage.Flat}");
            if (!_core.ShieldActive || _core.CurrentShield <= 0)
            {
                return;
            }

            if (_core.CurrentShield >= incomingDamage)
            {
                // 护盾值大于等于伤害，标记为完全闪避，但不立即处理
                _shouldDodgeNextHit = true;
                _core.CurrentShield -= incomingDamage;
                _core.DamageAbsorbedThisCycle += incomingDamage;
                
                // 更新上次受击时间
                _core.LastHitFrame = (int)Main.GameUpdateCount;
                _core.OnCooldown = true;
                
                // 记录本次伤害
                _core.RecentHits.Add(incomingDamage);
            }
            else
            {
                // 护盾值小于伤害，从伤害中减去部分护盾值
                modifiers.FinalDamage.Base -= _core.CurrentShield;
                _core.DamageAbsorbedThisCycle += _core.CurrentShield;
                
                // 护盾值归零
                _core.CurrentShield = 0;
                
                // 更新上次受击时间
                _core.LastHitFrame = (int)Main.GameUpdateCount;
                _core.OnCooldown = true;
                
                // 记录本次伤害
                _core.RecentHits.Add(incomingDamage);
            }
        }

        /// <summary>
        /// 处理玩家被NPC击中时的护盾减免
        /// </summary>
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            Main.NewText($"{npc.strengthMultiplier}");
            float incomingDamage = npc.damage;
            Main.NewText($"{incomingDamage}");
            Main.NewText($"{modifiers.SourceDamage.Base},{modifiers.SourceDamage.Additive},{modifiers.SourceDamage.Multiplicative},{modifiers.SourceDamage.Flat}");
            Main.NewText($"{modifiers.FinalDamage.Base},{modifiers.FinalDamage.Additive},{modifiers.FinalDamage.Multiplicative},{modifiers.FinalDamage.Flat}");
            if (!_core.ShieldActive || _core.CurrentShield <= 0)
            {
                return;
            }

            
            
            if (_core.CurrentShield >= incomingDamage)
            {
                // 护盾值大于等于伤害，标记为完全闪避，但不立即处理
                _shouldDodgeNextHit = true;
                _core.CurrentShield -= incomingDamage;
                _core.DamageAbsorbedThisCycle += incomingDamage;
                
                // 更新上次受击时间
                _core.LastHitFrame = (int)Main.GameUpdateCount;
                _core.OnCooldown = true;
                
                // 记录本次伤害
                _core.RecentHits.Add(incomingDamage);
            }
            else
            {
                // 护盾值小于伤害，从伤害中减去部分护盾值
                modifiers.SourceDamage.Base -= _core.CurrentShield;
                _core.DamageAbsorbedThisCycle += _core.CurrentShield;
                
                // 护盾值归零
                _core.CurrentShield = 0;
                
                // 更新上次受击时间
                _core.LastHitFrame = (int)Main.GameUpdateCount;
                _core.OnCooldown = true;
                
                // 记录本次伤害
                _core.RecentHits.Add(incomingDamage);
            }
        }



        // ... existing code ...
        /// <summary>
        /// 完全闪避伤害
        /// </summary>
        // ... existing code ...
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!_core.ShieldActive || _core.CurrentShield <= 0)
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
// ... existing code ...

        #region 代理方法 - 暴露核心功能给其他模块


        // 护盾核心属性
        /// <summary>
        /// 最大护盾值（只读）- 由基础值和修正值共同决定
        /// </summary>
        public float MaxShield 
        { 
            get => _core.MaxShield; 
            //set => _core.MaxShield = value; 
        }

        /// <summary>
        /// 最大护盾基础值 - 护盾容量的基础部分
        /// </summary>
        public float MaxShieldBase
        {
            get => _core.MaxShieldBase;
            set => _core.MaxShieldBase = value;
        }

        /// <summary>
        /// 最大护盾修正值 - 用于计算最终最大护盾值的修正系数
        /// </summary>
        public StatModifier MaxShieldModifier
        {
            get => _core.MaxShieldModifier;
            set => _core.MaxShieldModifier = value;
        }

        /// <summary>
        /// 当前护盾值 - 护盾当前剩余值
        /// </summary>
        public float CurrentShield 
        { 
            get => _core.CurrentShield; 
            set => _core.CurrentShield = value; 
        }

        /// <summary>
        /// 基础再生速率（只读）- 护盾自然恢复的基础速度
        /// </summary>
        public float ShieldRegen 
        { 
            get => _core.ShieldRegen; 
            //set => _core.BaseRegenRate = value; 
        }

        /// <summary>
        /// 再生速率修正值 - 用于计算最终再生速率的修正系数
        /// </summary>
        public StatModifier ShieldRegenModifier 
        {
            get => _core.ShieldRegenModifier;
            set => _core.ShieldRegenModifier = value;
        }

        /// <summary>
        /// 再生基础值 - 再生速率的基础部分
        /// </summary>
        public float ShieldRegenBase
        {
            get => _core.ShieldRegenBase;
            set => _core.ShieldRegenBase = value;
        }

        /// <summary>
        /// 再生延迟 - 护盾在受到伤害后开始再生前的延迟时间（帧）
        /// </summary>
        public int ShieldRegenDelay
        { 
            get => _core.ShieldRegenDelay; 
            set => _core.ShieldRegenDelay = value; 
        }

        /// <summary>
        /// 破碎时再生延迟 - 护盾破碎后重新开始再生前的延迟时间（帧）
        /// </summary>
        public int ShieldRegenDelayBroken
        { 
            get => _core.ShieldRegenDelayBroken; 
            set => _core.ShieldRegenDelayBroken = value; 
        }

        /// <summary>
        /// 护盾状态 - 当前护盾的状态（如正常、破碎、再生等）
        /// </summary>
        public ShieldState ShieldState 
        { 
            get => _core.ShieldState; 
            set => _core.ShieldState = value; 
        }

        /// <summary>
        /// 护盾激活状态 - 护盾是否处于激活状态
        /// </summary>
        public bool ShieldActive 
        { 
            get => _core.ShieldActive; 
            set => _core.ShieldActive = value; 
        }

        // 护盾参数
        /// <summary>
        /// 伤害吸收比率（只读）- 护盾吸收伤害的比例
        /// </summary>
        public float DamageAbsorptionRatio 
        { 
            get => _core.DamageAbsorptionRatio; 
            //set => _core.DamageAbsorptionRatio = value; 
        }

        /// <summary>
        /// 伤害吸收修正值 - 用于计算最终伤害吸收比率的修正系数
        /// </summary>
        public StatModifier DamageAbsorptionModifier
        {
            get => _core.DamageAbsorptionModifier;
            set => _core.DamageAbsorptionModifier = value;
        }

        /// <summary>
        /// 伤害吸收基础值 - 伤害吸收比率的基础部分
        /// </summary>
        public float DamageAbsorptionRatioBase
        {
            get => _core.DamageAbsorptionRatioBase;
            set => _core.DamageAbsorptionRatioBase = value;
        }

        /// <summary>
        /// 护盾强度（只读）- 影响护盾性能的综合参数
        /// </summary>
        public float ShieldStrength 
        { 
            get => _core.ShieldStrength; 
            //set => _core.ShieldStrength = value; 
        }

        /// <summary>
        /// 护盾强度修正值 - 用于计算最终护盾强度的修正系数
        /// </summary>
        public StatModifier ShieldStrengthModifier
        {
            get => _core.ShieldStrengthModifier;
            set => _core.ShieldStrengthModifier = value;
        }

        /// <summary>
        /// 护盾强度基础值 - 护盾强度的基础部分
        /// </summary>
        public float ShieldStrengthBase
        {
            get => _core.ShieldStrengthBase;
            set => _core.ShieldStrengthBase = value;
        }

        /// <summary>
        /// 破碎阈值 - 护盾破碎的临界值（低于此值时护盾破碎）
        /// </summary>
        public float BreakThreshold 
        { 
            get => _core.BreakThreshold; 
            set => _core.BreakThreshold = value; 
        }

        /// <summary>
        /// 可视化透明度 - 护盾视觉效果的透明度
        /// </summary>
        public float VisualAlpha 
        { 
            get => _core.VisualAlpha; 
            set => _core.VisualAlpha = value; 
        }

        /// <summary>
        /// 可视化缩放 - 护盾视觉效果的缩放比例
        /// </summary>
        public float VisualScale 
        { 
            get => _core.VisualScale; 
            set => _core.VisualScale = value; 
        }

        /// <summary>
        /// 色相偏移 - 护盾视觉效果的颜色偏移
        /// </summary>
        public float HueShift 
        { 
            get => _core.HueShift; 
            set => _core.HueShift = value; 
        }

        // 运行时状态
        /// <summary>
        /// 是否破碎 - 护盾是否处于破碎状态
        /// </summary>
        public bool IsBroken 
        { 
            get => _core.IsBroken; 
            set => _core.IsBroken = value; 
        }

        /// <summary>
        /// 破碎冷却时间 - 护盾破碎后到可以重新激活的时间（帧）
        /// </summary>
        public int BreakCooldown 
        { 
            get => _core.BreakCooldown; 
            set => _core.BreakCooldown = value; 
        }

        /// <summary>
        /// 最近受到的伤害列表 - 用于计算护盾的动态响应
        /// </summary>
        public System.Collections.Generic.List<float> RecentHits => _core.RecentHits;

        /// <summary>
        /// 是否在冷却中 - 护盾是否处于冷却状态
        /// </summary>
        public bool OnCooldown 
        { 
            get => _core.OnCooldown; 
            set => _core.OnCooldown = value; 
        }

        /// <summary>
        /// 过载充能 - 护盾的过载状态充能值
        /// </summary>
        public float OverloadCharge 
        { 
            get => _core.OverloadCharge; 
            set => _core.OverloadCharge = value; 
        }

        /// <summary>
        /// 当前周期吸收的伤害 - 当前再生周期内护盾吸收的总伤害
        /// </summary>
        public float DamageAbsorbedThisCycle 
        { 
            get => _core.DamageAbsorbedThisCycle; 
            set => _core.DamageAbsorbedThisCycle = value; 
        }
        
        // 内部状态
        /// <summary>
        /// 是否正在再生 - 护盾是否正在恢复
        /// </summary>
        public bool IsRegenerating 
        { 
            get => _core.IsRegenerating; 
            set => _core.IsRegenerating = value; 
        }

        /// <summary>
        /// 自上次受击以来的时间 - 上次受到伤害后经过的时间（帧）
        /// </summary>
        public int TimeSinceLastHit 
        { 
            get => _core.TimeSinceLastHit; 
            set => _core.TimeSinceLastHit = value; 
        }

        /// <summary>
        /// 再生开始时间 - 开始再生时的时间点（帧）
        /// </summary>
        public int RegenStartTime 
        { 
            get => _core.RegenStartTime; 
            set => _core.RegenStartTime = value; 
        }

        /// <summary>
        /// 护盾韧性 - 影响护盾对伤害抵抗能力的参数
        /// </summary>
        public int ShieldToughness 
        { 
            get => _core.ShieldToughness; 
            set => _core.ShieldToughness = value; 
        }

        /// <summary>
        /// 重置护盾到初始状态
        /// </summary>
        public void ResetShield() => _core.ResetShield();

        /// <summary>
        /// 激活护盾
        /// </summary>
        public void ActivateShield() => _core.ActivateShield();

        /// <summary>
        /// 关闭护盾
        /// </summary>
        public void DeactivateShield() => _core.DeactivateShield();

        /// <summary>
        /// 应用护盾的属性修正
        /// </summary>
        public void ApplyModifiers() => _core.ApplyModifiers();

        #endregion
    }
}