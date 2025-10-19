using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{ 

public class DamageTrackerPlayer : ModPlayer
    {
        /// <summary>
        /// 玩家在当前战斗中造成的总伤害
        /// </summary>
        public long TotalDamageDealt { get; private set; } = 0;

        /// <summary>
        /// 玩家在当前游戏过程中造成的总伤害
        /// </summary>
        public long SessionDamageDealt { get; private set; } = 0;
        
        /// <summary>
        /// 玩家连续造成的伤害
        /// </summary>
        public long ConsecutiveDamage { get; private set; } = 0;
        
        /// <summary>
        /// 上次造成伤害的时间（帧数）
        /// </summary>
        public int LastDamageFrame { get; private set; } = 0;
        
        /// <summary>
        /// 连续伤害超时时间（帧数），默认为120帧（2秒）
        /// </summary>
        public int ConsecutiveDamageTimeout { get; set; } = 120;

        /// <summary>
        /// 重置当前战斗伤害统计
        /// </summary>
        public void ResetCombatDamage()
        {
            TotalDamageDealt = 0;
        }

        /// <summary>
        /// 增加玩家造成的伤害统计
        /// </summary>
        /// <param name="damage">造成的伤害值</param>
        public void AddDamage(long damage)
        {
            TotalDamageDealt += damage;
            SessionDamageDealt += damage;
            AddConsecutiveDamage(damage);
        }
        
        /// <summary>
        /// 增加连续伤害统计
        /// </summary>
        /// <param name="damage">造成的伤害值</param>
        public void AddConsecutiveDamage(long damage)
        {
            // 检查是否超过超时时间
            if (Main.GameUpdateCount - LastDamageFrame > ConsecutiveDamageTimeout)
            {
                // 重置连续伤害
                ConsecutiveDamage = 0;
            }
            
            // 更新最后伤害时间
            LastDamageFrame = (int)Main.GameUpdateCount;
            
            // 增加连续伤害
            ConsecutiveDamage += damage;
            
            // 如果超过10000，则保持在10000
            if (ConsecutiveDamage > 10000)
            {
                ConsecutiveDamage = 10000;
            }
        }
        
        /// <summary>
        /// 获取当前连续伤害值
        /// </summary>
        /// <returns>连续伤害值</returns>
        public long GetConsecutiveDamage()
        {
            // 检查是否超过超时时间
            if (Main.GameUpdateCount - LastDamageFrame > ConsecutiveDamageTimeout)
            {
                // 重置连续伤害
                ConsecutiveDamage = 0;
            }
            
            return ConsecutiveDamage;
        }
        
        /// <summary>
        /// 获取当前连续伤害值，并指定超时时间
        /// </summary>
        /// <param name="timeout">超时时间（帧数）</param>
        /// <returns>连续伤害值</returns>
        public long GetConsecutiveDamage(int timeout)
        {
            // 检查是否超过指定的超时时间
            if (Main.GameUpdateCount - LastDamageFrame > timeout)
            {
                // 重置连续伤害
                ConsecutiveDamage = 0;
            }
            
            return ConsecutiveDamage;
        }
        
        /// <summary>
        /// 重置连续伤害统计
        /// </summary>
        public void ResetConsecutiveDamage()
        {
            ConsecutiveDamage = 0;
            LastDamageFrame = 0;
        }
        
        /// <summary>
        /// 当玩家对NPC造成伤害时调用此方法
        /// </summary>
        /// <param name="npc">被攻击的NPC</param>
        /// <param name="hit">命中信息</param>
        /// <param name="damageDone">造成的伤害</param>
        public override void OnHitNPC(NPC npc, NPC.HitInfo hit, int damageDone)
        {
            // 当玩家击中NPC时，将造成的伤害添加到连续伤害统计中
            AddConsecutiveDamage(damageDone);
        }

        /// <summary>
        /// 当玩家退出世界时保存统计数据
        /// </summary>
        // 
    }

    /// <summary>
    /// 静态工具类，用于方便地访问伤害统计功能
    /// </summary>
    public static class DamageTrackingHelper
    {
        /// <summary>
        /// 获取玩家在当前战斗中造成的总伤害
        /// </summary>
        /// <param name="player">要查询的玩家</param>
        /// <returns>玩家造成的总伤害</returns>
        public static long GetTotalCombatDamage(Player player)
        {
            return player.GetModPlayer<DamageTrackerPlayer>().TotalDamageDealt;
        }

        /// <summary>
        /// 获取玩家在当前游戏过程中造成的总伤害
        /// </summary>
        /// <param name="player">要查询的玩家</param>
        /// <returns>玩家造成的总伤害</returns>
        public static long GetSessionDamage(Player player)
        {
            return player.GetModPlayer<DamageTrackerPlayer>().SessionDamageDealt;
        }

        /// <summary>
        /// 重置玩家的当前战斗伤害统计
        /// </summary>
        /// <param name="player">要重置的玩家</param>
        public static void ResetCombatDamage(Player player)
        {
            player.GetModPlayer<DamageTrackerPlayer>().ResetCombatDamage();
        }

        /// <summary>
        /// 为玩家添加造成的伤害到统计中
        /// </summary>
        /// <param name="player">造成伤害的玩家</param>
        /// <param name="damage">伤害值</param>
        public static void AddDamage(Player player, long damage)
        {
            if (damage > 0)
            {
                player.GetModPlayer<DamageTrackerPlayer>().AddDamage(damage);
            }
        }
        
        /// <summary>
        /// 获取玩家的连续伤害值
        /// </summary>
        /// <param name="player">要查询的玩家</param>
        /// <returns>玩家的连续伤害值</returns>
        public static long GetConsecutiveDamage(Player player)
        {
            return player.GetModPlayer<DamageTrackerPlayer>().GetConsecutiveDamage();
        }
        
        /// <summary>
        /// 获取玩家的连续伤害值，并指定超时时间
        /// </summary>
        /// <param name="player">要查询的玩家</param>
        /// <param name="timeout">超时时间（帧数）</param>
        /// <returns>玩家的连续伤害值</returns>
        public static long GetConsecutiveDamage(Player player, int timeout)
        {
            return player.GetModPlayer<DamageTrackerPlayer>().GetConsecutiveDamage(timeout);
        }
        
        /// <summary>
        /// 重置玩家的连续伤害统计
        /// </summary>
        /// <param name="player">要重置的玩家</param>
        public static void ResetConsecutiveDamage(Player player)
        {
            player.GetModPlayer<DamageTrackerPlayer>().ResetConsecutiveDamage();
        }
        
        /// <summary>
        /// 设置玩家连续伤害超时时间
        /// </summary>
        /// <param name="player">要设置的玩家</param>
        /// <param name="timeout">超时时间（帧数）</param>
        public static void SetConsecutiveDamageTimeout(Player player, int timeout)
        {
            player.GetModPlayer<DamageTrackerPlayer>().ConsecutiveDamageTimeout = timeout;
        }
    }
    }