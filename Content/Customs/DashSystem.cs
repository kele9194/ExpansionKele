using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 通用冲刺系统，可用于各种武器的冲刺功能
    /// </summary>
    public class DashSystem
    {
        // 冲刺状态
        public enum DashState
        {
            NotDashing,
            Dashing,
            Cooldown
        }

        // 冲刺方向
        public Vector2 DashDirection { get; private set; }
        
        // 冲刺状态
        public DashState State { get; private set; } = DashState.NotDashing;
        
        // 冲刺持续时间
        public int DashDuration { get; set; } = 20;
        
        // 冲刺速度
        public float DashSpeed { get; set; } = 10f;
        
        // 冲刺冷却时间
        public int CooldownTime { get; set; } = 30;
        
        // 冲刺伤害倍数
        public float DamageMultiplier { get; set; } = 1f;
        
        // 是否在冲刺过程中可以控制方向
        public bool CanChangeDirection { get; set; } = true;
        
        // 冲刺过程中应用的重力
        public float DashGravity { get; set; } = 0.1f;
        
        // 默认重力
        public float DefaultGravity { get; set; } = 0.4f;
        
        // 冲刺时的无敌帧
        public int IFrames { get; set; } = 10;
        
        // 当前冲刺计时器
        private int dashTimer = 0;
        
        // 冲刺冷却计时器
        private int cooldownTimer = 0;
        
        // 冲刺开始时的速度
        private Vector2 initialVelocity = Vector2.Zero;
        
        // 冲刺结束时的速度
        private Vector2 finalVelocity = Vector2.Zero;

        /// <summary>
        /// 开始冲刺
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <param name="direction">冲刺方向</param>
        public void StartDash(Player player, Vector2 direction)
        {
            if (State != DashState.NotDashing || cooldownTimer > 0)
                return;

            DashDirection = direction.SafeNormalize(Vector2.Zero);
            State = DashState.Dashing;
            dashTimer = DashDuration;
            
            // 保存初始速度
            initialVelocity = player.velocity;
            
            // 设置冲刺速度
            player.velocity = DashDirection * DashSpeed;
            
            // 应用无敌帧
            if (IFrames > 0)
            {
                player.immune = true;
                player.immuneTime = IFrames;
            }
            
            // 播放冲刺音效（可选）
            // SoundEngine.PlaySound(SoundID.Item1, player.Center);
            
            // 重置玩家其他状态
            player.fallStart = (int)(player.position.Y / 16f);
        }

        /// <summary>
        /// 更新冲刺状态
        /// </summary>
        /// <param name="player">玩家实例</param>
        public void UpdateDash(Player player)
        {
            switch (State)
            {
                case DashState.Dashing:
                    UpdateDashing(player);
                    break;
                case DashState.Cooldown:
                    UpdateCooldown();
                    break;
            }
        }

        /// <summary>
        /// 更新冲刺过程
        /// </summary>
        /// <param name="player">玩家实例</param>
        private void UpdateDashing(Player player)
        {
            if (dashTimer <= 0)
            {
                // 冲刺结束
                EndDash(player);
                return;
            }

            dashTimer--;

            // 更新玩家速度（可以添加速度曲线变化）
            float progress = (float)dashTimer / DashDuration;
            player.velocity = DashDirection * DashSpeed * GetDashSpeedMultiplier(progress);
            
            // 应用重力
            player.gravity = DashGravity;
            
            // 如果允许改变方向，可以在此处更新方向
            if (CanChangeDirection)
            {
                // 可以根据需要添加方向调整逻辑
            }
        }

        /// <summary>
        /// 获取冲刺速度倍数（用于创建速度曲线）
        /// </summary>
        /// <param name="progress">冲刺进度 (0-1)</param>
        /// <returns>速度倍数</returns>
        private float GetDashSpeedMultiplier(float progress)
        {
            // 使用正弦曲线创建更自然的加速/减速效果
            return (float)System.Math.Sin(progress * MathHelper.Pi);
        }

        /// <summary>
        /// 结束冲刺
        /// </summary>
        /// <param name="player">玩家实例</param>
        private void EndDash(Player player)
        {
            State = DashState.Cooldown;
            cooldownTimer = CooldownTime;
            
            // 恢复默认重力
            player.gravity = DefaultGravity;
            
            // 设置结束速度（可以是减速或其他效果）
            player.velocity *= 0.2f;
        }

        /// <summary>
        /// 更新冷却时间
        /// </summary>
        private void UpdateCooldown()
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer--;
            }
            else
            {
                State = DashState.NotDashing;
            }
        }

        /// <summary>
        /// 检查当前是否可以开始冲刺
        /// </summary>
        /// <returns>是否可以冲刺</returns>
        public bool CanDash()
        {
            return State == DashState.NotDashing && cooldownTimer <= 0;
        }

        /// <summary>
        /// 获取冲刺伤害倍数
        /// </summary>
        /// <returns>伤害倍数</returns>
        public float GetDamageMultiplier()
        {
            return State == DashState.Dashing ? DamageMultiplier : 1f;
        }

        /// <summary>
        /// 检查是否处于冲刺状态
        /// </summary>
        /// <returns>是否正在冲刺</returns>
        public bool IsDashing()
        {
            return State == DashState.Dashing;
        }

        /// <summary>
        /// 检查是否处于冷却状态
        /// </summary>
        /// <returns>是否正在冷却</returns>
        public bool IsCoolingDown()
        {
            return State == DashState.Cooldown || cooldownTimer > 0;
        }

        /// <summary>
        /// 获取冷却进度 (0-1)
        /// </summary>
        /// <returns>冷却进度</returns>
        public float GetCooldownProgress()
        {
            if (CooldownTime <= 0) return 0f;
            return 1f - (float)cooldownTimer / CooldownTime;
        }

        /// <summary>
        /// 获取冲刺进度 (0-1)
        /// </summary>
        /// <returns>冲刺进度</returns>
        public float GetDashProgress()
        {
            if (DashDuration <= 0) return 0f;
            return 1f - (float)dashTimer / DashDuration;
        }

        /// <summary>
        /// 重置冲刺系统
        /// </summary>
        public void Reset()
        {
            State = DashState.NotDashing;
            dashTimer = 0;
            cooldownTimer = 0;
        }
    }
}