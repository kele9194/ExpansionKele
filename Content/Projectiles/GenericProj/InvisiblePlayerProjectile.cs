using System;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.GenericProj
{
    /// <summary>
    /// 用于处理玩家逻辑的不可见投射物基类
    /// 此类专门设计用于在玩家身上运行后台逻辑，如计时器、状态管理等
    /// 投射物完全不可见且不会造成任何伤害或碰撞
    /// </summary>
    public abstract class InvisiblePlayerProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.PlayerLogic";
        public sealed override string Texture => "ExpansionKele/Content/Projectiles/GenericProj/InvisiblePlayerProjectile";
        /// <summary>
        /// 最大存活时间（帧数）
        /// </summary>
        protected abstract int MaxLifetime { get; }
        
        /// <summary>
        /// 关联的玩家索引
        /// </summary>
        protected int OwnerPlayerIndex => Projectile.owner;
        
        /// <summary>
        /// 获取关联的玩家对象
        /// </summary>
        protected Player OwnerPlayer
        {
            get
            {
                if (OwnerPlayerIndex >= 0 && OwnerPlayerIndex < Main.maxPlayers)
                    return Main.player[OwnerPlayerIndex];
                return null;
            }
        }
        
        /// <summary>
        /// 检查关联的玩家是否有效且存活
        /// </summary>
        protected bool IsOwnerValid => OwnerPlayer != null && OwnerPlayer.active && !OwnerPlayer.dead;
        
        public sealed override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.extraUpdates = 0;
            Projectile.DamageType = DamageClass.Default;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            
            SetInvisibleDefaults();
        }
        
        /// <summary>
        /// 子类可以重写此方法来设置额外的默认值
        /// </summary>
        protected virtual void SetInvisibleDefaults()
        {
        }
        
        public sealed override bool? CanDamage()
        {
            return false;
        }
        
        public sealed override bool ShouldUpdatePosition()
        {
            return false;
        }
        
        public sealed override Color? GetAlpha(Color lightColor)
        {
            return Color.Transparent;
        }
        
        public sealed override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        
        /// <summary>
        /// 当玩家无效时自动销毁投射物
        /// </summary>
        public sealed override void AI()
        {
            if (!IsOwnerValid)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = OwnerPlayer.MountedCenter;
            
            UpdatePlayerLogic();
        }
        
        /// <summary>
        /// 子类重写此方法来实现具体的玩家逻辑
        /// </summary>
        protected virtual void UpdatePlayerLogic()
        {
        }
        
        /// <summary>
        /// 当投射物被销毁时调用
        /// </summary>
        public sealed override void OnKill(int timeLeft)
        {
            OnPlayerProjectileKilled(timeLeft);
        }
        
        /// <summary>
        /// 子类可以重写此方法来处理投射物销毁时的清理工作
        /// </summary>
        protected virtual void OnPlayerProjectileKilled(int timeLeft)
        {
        }
        
        /// <summary>
        /// 便捷方法：在指定玩家位置创建不可见投射物
        /// </summary>
        /// <typeparam name="T">投射物类型</typeparam>
        /// <param name="player">目标玩家</param>
        /// <param name="lifetime">存活时间（帧数），-1表示使用默认值</param>
        /// <returns>创建的投射物ID</returns>
        public static int SpawnForPlayer<T>(Player player, int lifetime = -1) where T : InvisiblePlayerProjectile
        {
            if (player == null || !player.active || player.dead)
                return -1;
            
            var proj = Projectile.NewProjectileDirect(
                player.GetSource_FromThis(),
                player.position,
                Vector2.Zero,
                ModContent.ProjectileType<T>(),
                0,
                0f,
                player.whoAmI
            );
            
            if (lifetime > 0)
                proj.timeLeft = lifetime;
            
            proj.netUpdate = true;
            return proj.whoAmI;
        }

        /// <summary>
        /// 便捷方法：在指定玩家位置创建不可见投射物（带AI参数）
        /// </summary>
        /// <typeparam name="T">投射物类型</typeparam>
        /// <param name="player">目标玩家</param>
        /// /// <param name="lifetime">存活时间（帧数），-1表示使用默认值</param>
        /// <param name="ai0">AI参数0</param>
        /// <param name="ai1">AI参数1</param>
        /// <param name="ai2">AI参数2</param>
        
        /// <returns>创建的投射物ID</returns>
        public static int SpawnForPlayer<T>(Player player, int lifetime = -1,float ai0=0, float ai1=0, float ai2=0 ) where T : InvisiblePlayerProjectile
        {
            if (player == null || !player.active || player.dead)
                return -1;
            
            var proj = Projectile.NewProjectileDirect(
                player.GetSource_FromThis(),
                player.position,
                Vector2.Zero,
                ModContent.ProjectileType<T>(),
                0,
                0f,
                player.whoAmI,
                ai0,
                ai1,
                ai2
            );
            
            if (lifetime > 0)
                proj.timeLeft = lifetime;
            
            proj.netUpdate = true;
            return proj.whoAmI;
        }
        
        /// <summary>
        /// 便捷方法：检查是否存在指定类型的玩家投射物
        /// </summary>
        /// <typeparam name="T">投射物类型</typeparam>
        /// <param name="player">目标玩家</param>
        /// <returns>是否存在该类型的投射物</returns>
        public static bool ExistsForPlayer<T>(Player player) where T : InvisiblePlayerProjectile
        {
            if (player == null || !player.active)
                return false;
            
            int projType = ModContent.ProjectileType<T>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                var proj = Main.projectile[i];
                if (proj.active && proj.type == projType && proj.owner == player.whoAmI)
                    return true;
            }
            return false;
        }
    }
}