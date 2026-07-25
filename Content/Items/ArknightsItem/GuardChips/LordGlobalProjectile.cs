using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 领主（Lord）天赋的弹幕处理类。
    /// 效果：
    /// 武器自带射弹在128px~320px范围内获得额外加成：
    /// - 伤害从+100%线性递增至+120%（贴脸最大）
    /// - 穿甲从0%线性递增至10%（贴脸最大）
    /// </summary>
    public class LordGlobalProjectile : GlobalProjectile
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

                // 快速检查：如果不是领主，直接移除记录
                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                if (arkNum == 0) return;
                if (ArknightsGlobalItem.GetGuardSubclass(sourceItem) != GuardSubclass.Lord)
                {
                    projectileSourceMap.Remove(projectile.whoAmI);
                }
            }
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!TryGetLordInfo(projectile, out Item sourceItem, out Player owner))
                return;

            // 只处理武器自带的射弹（排除领主剑气本身）
            // lordSlashType 是领主剑气的射弹类型，需要在 LordSlashProjectile 中暴露
            if (sourceItem.shoot != projectile.type) return;
            if (!projectile.DamageType.CountsAsClass(DamageClass.Melee)) return;

            // 计算距离（用玩家到目标的距离，而非弹幕到目标的距离）
            float distance = Vector2.Distance(owner.Center, target.Center);

            // 128~320px范围内生效，低于128px时取最大加成
            if (distance <= 320f)
            {
                // t: 0 at 320px, 1 at ≤128px (贴脸时达到最大)
                float t = distance <= 128f
                    ? 1f
                    : (320f - distance) / (320f - 128f);

                // 伤害从+100%线性递增至+120%（贴脸最大）
                float damageBonus = 1.0f + t * 0.2f; // +100% ~ +120%
                modifiers.FinalDamage *= 1f + damageBonus;

                // 穿甲从0%线性递增至10%（贴脸最大）
                float armorPen = t * 10f; // 0 ~ 10
                modifiers.ArmorPenetration += armorPen;
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

        /// <summary>
        /// 尝试获取弹幕的领主刻印信息。如果任何校验失败，返回 false。
        /// </summary>
        private static bool TryGetLordInfo(Projectile projectile, out Item sourceItem, out Player owner)
        {
            sourceItem = null;
            owner = null;

            if (projectile == null || !projectile.active)
                return false;

            if (!projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo))
                return false;

            sourceItem = sourceInfo.item;
            if (sourceItem == null || sourceItem.IsAir)
                return false;

            owner = sourceInfo.player ?? Main.player[projectile.owner];
            if (owner == null || !owner.active)
                return false;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
            if (arkNum == 0)
                return false;

            if (ArknightsGlobalItem.GetGuardSubclass(sourceItem) != GuardSubclass.Lord)
                return false;

            return true;
        }
    }
}