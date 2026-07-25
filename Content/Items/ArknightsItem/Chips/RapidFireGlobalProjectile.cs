using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;
using System.Linq;
using System;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 速射手（RapidFire）天赋的弹幕处理类。
    /// 效果：
    /// - 弹速 +20%
    /// - 对空中目标伤害 ×1.1
    /// - 当有非空中敌人存在时，攻速 ×1.08
    /// </summary>
    public class RapidFireGlobalProjectile : GlobalProjectile
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

                // 从字典获取的方式与 OnSpawn 不同，这里用 statsSource 直接判断
                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                if (arkNum == 0) return;

                if ((SniperSubclass)arkNum != SniperSubclass.RapidFire) return;

                projectile.velocity *= 1.2f;
            }
        }
        public override void AI(Projectile projectile)
        {
            if (!TryGetRapidFireInfo(projectile, out Item sourceItem, out _)) return;

            // 检查 isHoming 开关
            if (!sourceItem.GetGlobalItem<ArknightsGlobalItem>().isHoming) return;
            //已经是在必须制导的职业中才会写这个，默认不写白名单判断

            NPC target = FindAirTarget(projectile.Center, 400);
            if (target != null)
            {
                Vector2 targetDir = target.Center - projectile.Center;
                targetDir.Normalize();
                Vector2 currentDir = projectile.velocity.SafeNormalize(Vector2.Zero);
                
                // ✅ 修复：Lerp 后归一化，保持速度恒定
                Vector2 newDir = Vector2.Lerp(currentDir, targetDir, 0.03f);
                newDir.Normalize();
                projectile.velocity = newDir * projectile.velocity.Length();
            }
        }
        /// <summary>
        /// 查找最近的空中单位（速射狙专用）
        /// </summary>
        private NPC FindAirTarget(Vector2 center, float range)
        {
            NPC best = null;
            float bestDist = range;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.life <= 0) continue;

                // 无重力单位
                if (npc.noGravity) continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = npc;
                }
            }

            return best;
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!TryGetRapidFireInfo(projectile, out _, out _)) return;

            if (target.velocity.Y != 0f)
            {
                modifiers.FinalDamage *= 1.2f;
            }
        }

        /// <summary>
        /// 尝试获取弹幕的速射手刻印信息。如果任何校验失败，返回 false。
        /// </summary>
        private static bool TryGetRapidFireInfo(Projectile projectile, out Item sourceItem, out Player owner)
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

            if ((SniperSubclass)arkNum != SniperSubclass.RapidFire)
                return false;

            return true;
        }

        public override void OnKill(Projectile projectile, int timeleft)
        {
            projectileSourceMap.Remove(projectile.whoAmI);
        }

        // 玩家退出世界时清理所有数据
        public override void Unload()
        {
            projectileSourceMap.Clear();
        }
    }

    /// <summary>
    /// 速射手（RapidFire）天赋的玩家效果处理类。
    /// 效果3：当有非空中敌人存在时，攻速 ×1.08
    /// </summary>
    public class RapidFirePlayer : ModPlayer
    {
        /// <summary>
        /// 玩家当前手持的武器是否为速射手刻印
        /// </summary>
        public bool HasRapidFireWeaponEquipped = false;

        /// <summary>
        /// 附近是否存在非空中敌人
        /// </summary>
        public bool HasNonAirborneTargetNearby = false;

        public override void ResetEffects()
        {
            HasRapidFireWeaponEquipped = false;
            HasNonAirborneTargetNearby = false;
        }

        public override void PostUpdate()
        {
            // 检查当前手持武器是否刻印为速射手
            Item heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
            {
                HasRapidFireWeaponEquipped = false;
                HasNonAirborneTargetNearby = false;
                return;
            }

            int arkNum = ArknightsGlobalItem.GetArknightsNum(heldItem);
            if (arkNum == 0)
            {
                HasRapidFireWeaponEquipped = false;
                HasNonAirborneTargetNearby = false;
                return;
            }

            SniperSubclass sub = (SniperSubclass)arkNum;
            if (sub != SniperSubclass.RapidFire)
            {
                HasRapidFireWeaponEquipped = false;
                HasNonAirborneTargetNearby = false;
                return;
            }

            HasRapidFireWeaponEquipped = true;

            // 扫描附近 NPC，检查是否存在非空中目标（velocity.Y == 0）
            // 检测范围：约 800 像素（大约一个屏幕宽度）
            HasNonAirborneTargetNearby = Main.npc.Any(npc =>
                npc != null && npc.active && !npc.friendly && npc.type != Terraria.ID.NPCID.TargetDummy &&
                Vector2.Distance(Player.Center, npc.Center) <= 800f &&
                npc.velocity.Y == 0f);
        }

        public override float UseSpeedMultiplier(Item item)
        {
            // 效果3：当手持速射手武器且附近有非空中敌人时，攻速 ×1.15
            if (HasRapidFireWeaponEquipped && HasNonAirborneTargetNearby)
            {
                return 1.15f;
            }
            return 1f;
        }
    }
}