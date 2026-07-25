using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 散射手（Spread）天赋的弹幕处理类。
    /// 效果：
    /// - OnSpawn：获取弹幕伤害并累加到玩家存储伤害中
    /// - 设置3帧倒计时，倒计时结束后释放扇形AOE
    /// </summary>
    public class SpreadGlobalProjectile : GlobalProjectile
    {
        // 记录每个弹幕的来源武器和玩家（key = projectile.whoAmI）
        private static readonly Dictionary<int, (Item item, Player player)> projectileSourceMap
            = new Dictionary<int, (Item, Player)>();

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                Item sourceItem = statsSource.Item;
                Player owner = statsSource.Player;

                projectileSourceMap[projectile.whoAmI] = (sourceItem, owner);

                // 检查是否为散射手刻印
                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                if (arkNum == 0) return;
                if ((SniperSubclass)arkNum != SniperSubclass.Spread) return;

                // 获取玩家的散射手数据组件
                var spreadPlayer = owner.GetModPlayer<SpreadPlayer>();

                // 累弹药伤害到存储伤害
                spreadPlayer.accumulatedDamage += projectile.damage;

                // 重置AOE倒计时为3帧
                spreadPlayer.aoeTimer = 3;
            }
        }

        public override void OnKill(Projectile projectile, int timeleft)
        {
            projectileSourceMap.Remove(projectile.whoAmI);
        }

        public override void Unload()
        {
            projectileSourceMap.Clear();
        }
    }

    /// <summary>
    /// 散射手（Spread）天赋的玩家数据组件。
    /// 存储累积的弹药伤害，管理AOE释放倒计时。
    /// 攻速惩罚：×0.75（减少25%）
    /// </summary>
    public class SpreadPlayer : ModPlayer
    {
        /// <summary>从弹药累积的存储伤害</summary>
        public float accumulatedDamage = 0f;

        /// <summary>AOE释放倒计时（帧数）</summary>
        public int aoeTimer = 0;

        /// <summary>当前手持武器是否为散射手刻印</summary>
        public bool HasSpreadWeaponEquipped = false;

        public override void ResetEffects()
        {
            // 不在这里重置accumulatedDamage，由AOE发射后手动归零
            HasSpreadWeaponEquipped = false;
        }

        public override void PostUpdate()
        {
            // 更新武器状态
            HasSpreadWeaponEquipped = IsSpreadWeapon(Player.HeldItem);

            // ⚠ Main.MouseWorld 仅在客户端有效，服务器直接跳过
            if (Main.dedServ) return;

            // 倒计时递减
            if (aoeTimer > 0)
            {
                aoeTimer--;

                // 倒计时归零 → 释放AOE
                if (aoeTimer == 0 && accumulatedDamage > 0f)
                {
                    SpawnAOE();
                }
            }
        }

        /// <summary>
        /// 攻速修正：散射手基础攻速 ×0.75（减少25%）
        /// </summary>
        public override float UseSpeedMultiplier(Item item)
        {
            if (IsSpreadWeapon(item))
            {
                return 0.85f;
            }
            return 1f;
        }

        /// <summary>
        /// 判断物品是否为散射手刻印武器
        /// </summary>
        private static bool IsSpreadWeapon(Item item)
        {
            if (item == null || item.IsAir)
                return false;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(item);
            if (arkNum == 0)
                return false;

            return (SniperSubclass)arkNum == SniperSubclass.Spread;
        }

        /// <summary>
        /// 以玩家为中心生成扇形AOE弹幕
        /// </summary>
        private void SpawnAOE()
        {
            if (Player == null || !Player.active) return;

            int damage = (int)accumulatedDamage;
            if (damage < 1) damage = 1;

            // 获取玩家瞄准方向（从玩家指向鼠标光标的弧度角度）
            float facingAngle = (Main.MouseWorld - Player.Center).ToRotation();

            // 生成AOE弹幕
            int aoeType = ModContent.ProjectileType<SpreadProjectile>();
            Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.Center,
                Vector2.Zero,
                aoeType,
                damage,
                2f,
                Player.whoAmI,
                ai0: facingAngle  // 存储玩家瞄准角度用于扇形检测和绘制
            );

            // 消耗存储伤害
            accumulatedDamage = 0f;
        }
    }
}