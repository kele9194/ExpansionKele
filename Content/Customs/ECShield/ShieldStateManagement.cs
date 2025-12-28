using Terraria;

namespace ExpansionKele.Content.Customs.ECShield
{
    /// <summary>
    /// 护盾状态管理类
    /// </summary>
    public class ShieldStateManagement
    {
        private readonly ShieldCore _core;

        public ShieldStateManagement(ShieldCore core)
        {
            _core = core;
        }

        /// <summary>
        /// 更新护盾状态机
        /// </summary>
        // ... existing code ...
        /// <summary>
        /// 更新护盾状态机
        /// </summary>
        // ... existing code ...
        /// <summary>
        /// 更新护盾状态机
        /// </summary>
        public void UpdateShieldState()
        {
            // 检查护盾是否破碎（当护盾值为0时）
            _core.IsBroken = _core.CurrentShield <= 0;

            // 根据当前护盾值更新状态
            if (_core.IsBroken)
            {
                _core.ShieldState = ShieldState.Broken;
            }
            else if (_core.IsRegenerating)
            {
                _core.ShieldState = ShieldState.InRegen;
            }
            else if (_core.CurrentShield >= _core.MaxShield)
            {
                _core.ShieldState = ShieldState.Full;
            }
            else
            {
                // 如果护盾未满且不在恢复中，设置为冷却状态
                _core.ShieldState = ShieldState.Cooldown;
            }
        }
// ... existing code ...
// ... existing code ...

        /// <summary>
        /// 更新护盾恢复逻辑
        /// </summary>
        // ... existing code ...
        public void UpdateShieldRegeneration()
        {
            // 检查是否过了恢复延迟
            int currentFrame = (int)Main.GameUpdateCount;
            int delay = _core.IsBroken ? _core.ShieldRegenDelayBroken : _core.ShieldRegenDelay;
            
            if (currentFrame - _core.LastHitFrame >= delay)
            {
                _core.OnCooldown = false;
                
                // 开始恢复护盾
                if (_core.CurrentShield < _core.MaxShield)
                {
                    _core.IsRegenerating = true;
                    float regenAmount = _core.ShieldRegenBase * _core.ShieldStrength * (1f / 60f); // 每帧恢复量
                    _core.CurrentShield = System.Math.Min(_core.MaxShield, _core.CurrentShield + regenAmount);
                    
                    // 如果护盾满了，停止恢复
                    if (_core.CurrentShield >= _core.MaxShield)
                    {
                        _core.CurrentShield = _core.MaxShield;
                        _core.IsRegenerating = false;
                        _core.ShieldState = ShieldState.Full;
                    }
                }
                else
                {
                    _core.IsRegenerating = false;
                }
            }
            else
            {
                // 仍在恢复延迟中
                _core.OnCooldown = true;
                _core.IsRegenerating = false;
            }
        }
// ... existing code ...

        /// <summary>
        /// 更新各种计时器
        /// </summary>
        public void UpdateTimers()
        {
            _core.TimeSinceLastHit = (int)(Main.GameUpdateCount - _core.LastHitFrame);
            
            if (_core.BreakCooldown > 0)
            {
                _core.BreakCooldown--;
            }

            // 清理过期的伤害记录（保留最近3秒的记录）
            _core.RecentHits.RemoveAll(hit => Main.GameUpdateCount - (int)hit > 180); // 180帧 = 3秒
        }
    }
}