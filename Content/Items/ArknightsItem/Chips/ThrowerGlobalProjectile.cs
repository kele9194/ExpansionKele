using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 投掷手（Thrower）天赋的弹幕处理类。
    /// 效果：
    /// - 主弹命中造成 0.75 × 基础伤害
    /// - 命中后 0.5秒（30 ticks）产生第一次爆炸：0.375 × 基础伤害，AOE 半径 24px
    /// - 命中后 1.0秒（60 ticks）产生第二次爆炸：0.375 × 基础伤害，AOE 半径 24px
    /// - 总计：1.5 × 基础伤害
    /// </summary>
    public class ThrowerGlobalProjectile : GlobalProjectile
    {
        // 记录每个弹幕的来源武器和玩家
        private static readonly Dictionary<int, (Item item, Player player)> projectileSourceMap
            = new Dictionary<int, (Item, Player)>();

        // 记录已触发过投掷手效果的弹幕（防止重复触发）
        private static readonly HashSet<int> triggeredProjectiles = new HashSet<int>();

        public override bool InstancePerEntity => false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                projectileSourceMap[projectile.whoAmI] = (statsSource.Item, statsSource.Player);
            }
        }

        /// <summary>
        /// 主弹命中伤害 × 0.75
        /// </summary>
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!TryGetThrowerInfo(projectile, out _, out _)) return;
            if (triggeredProjectiles.Contains(projectile.whoAmI)) return;

            // 主弹仅造成 0.75 倍基础伤害
            modifiers.FinalDamage *= 0.75f;
        }

        /// <summary>
        /// 命中后产生两次延迟爆炸
        /// </summary>
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!TryGetThrowerInfo(projectile, out _, out Player owner)) return;
            if (triggeredProjectiles.Contains(projectile.whoAmI)) return;

            // 标记已触发
            triggeredProjectiles.Add(projectile.whoAmI);

            // 反推基础伤害：实际伤害 ÷ 0.75
            int baseDamage = (int)(hit.Damage / 0.75f);
            // 爆炸伤害 = 基础伤害 × 0.375
            int explosionDamage = (int)(baseDamage * 0.375f);
            if (explosionDamage < 1) explosionDamage = 1;

            Vector2 explosionPos = target.Center;

            // 第一次爆炸：0.5秒后（30 ticks）
            Projectile.NewProjectile(
                projectile.GetSource_FromThis(),
                explosionPos,
                Vector2.Zero,
                ModContent.ProjectileType<ThrowerExplosion>(),
                explosionDamage,
                2f,
                owner.whoAmI,
                ai0: 30f
            );

            // 第二次爆炸：1.0秒后（60 ticks）
            Projectile.NewProjectile(
                projectile.GetSource_FromThis(),
                explosionPos,
                Vector2.Zero,
                ModContent.ProjectileType<ThrowerExplosion>(),
                explosionDamage,
                2f,
                owner.whoAmI,
                ai0: 60f
            );
        }

        public override void OnKill(Projectile projectile, int timeleft)
        {
            projectileSourceMap.Remove(projectile.whoAmI);
            triggeredProjectiles.Remove(projectile.whoAmI);
        }

        private static bool TryGetThrowerInfo(Projectile projectile, out Item sourceItem, out Player owner)
        {
            sourceItem = null;
            owner = null;

            if (projectile == null || !projectile.active) return false;
            if (!projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo)) return false;

            sourceItem = sourceInfo.item;
            if (sourceItem == null || sourceItem.IsAir) return false;

            owner = sourceInfo.player ?? Main.player[projectile.owner];
            if (owner == null || !owner.active) return false;

            int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
            if (arkNum == 0) return false;
            if ((SniperSubclass)arkNum != SniperSubclass.Thrower) return false;

            return true;
        }

        public override void Unload()
        {
            projectileSourceMap.Clear();
            triggeredProjectiles.Clear();
        }
    }
}