using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Projectiles;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.Chips
{
    /// <summary>
    /// 炮手天赋的弹幕处理类。
    /// 负责：命中时触发爆炸、减防、对低速敌人增伤。
    /// </summary>
    public class ArtilleristGlobalProjectile : GlobalProjectile
    {
        // 记录每个弹幕是否已经触发过炮手效果（防止重复触发）
        private static readonly HashSet<int> triggeredProjectiles = new HashSet<int>();

        // 记录每个弹幕的来源武器和玩家（key = projectile.whoAmI）
        private static readonly Dictionary<int, (Item item, Player player)> projectileSourceMap
            = new Dictionary<int, (Item, Player)>();


        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                projectileSourceMap[projectile.whoAmI] = (statsSource.Item, statsSource.Player);
            }
        }
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            // 只处理有效的弹幕
            if (projectile == null || !projectile.active) return;

            // 从字典中获取来源信息
            if (!projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo)) return;

            Item sourceItem = sourceInfo.item;
            if (sourceItem == null || sourceItem.IsAir) return;

            Player owner = sourceInfo.player ?? Main.player[projectile.owner];
            if (owner == null || !owner.active) return;

            // 检查该武器是否刻印为炮手
            int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
            if (arkNum == 0) return;

            SniperSubclass sub = (SniperSubclass)arkNum;
            if (sub != SniperSubclass.Artillerist) return;

            // 检查这个弹幕是否已经触发过炮手效果
            if (triggeredProjectiles.Contains(projectile.whoAmI)) return;
            modifiers.DefenseEffectiveness*=0.5f;
            if(target.velocity.Length()<=10f){
                modifiers.FinalDamage*=1.1f;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 只处理有效的弹幕
            if (projectile == null || !projectile.active) return;

            // 从字典中获取来源信息
            if (!projectileSourceMap.TryGetValue(projectile.whoAmI, out var sourceInfo)) return;

            Item sourceItem = sourceInfo.item;
            if (sourceItem == null || sourceItem.IsAir) return;

            Player owner = sourceInfo.player ?? Main.player[projectile.owner];
            if (owner == null || !owner.active) return;

            // 检查该武器是否刻印为炮手
            int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
            if (arkNum == 0) return;

            SniperSubclass sub = (SniperSubclass)arkNum;
            if (sub != SniperSubclass.Artillerist) return;

            // 检查这个弹幕是否已经触发过炮手效果
            if (triggeredProjectiles.Contains(projectile.whoAmI)) return;

            // 标记该弹幕已触发
            triggeredProjectiles.Add(projectile.whoAmI);

            // 执行炮手效果
            ApplyArtilleristEffect(projectile, target, hit.Damage, owner);
        }

        /// <summary>
        /// 应用炮手的具体效果
        /// </summary>
        private void ApplyArtilleristEffect(Projectile projectile, NPC target, int baseDamage, Player owner)
        {

            // 1. 生成爆炸（48像素半径，伤害 = 基础伤害 × 20%）
            int explosionDamage = (int)(baseDamage * 0.2f);
            // 确保至少造成1点伤害
            if (explosionDamage < 1) explosionDamage = 1;

            Vector2 explosionPos = target.Center;

            // 生成爆炸抛射体
            int explosionType = ModContent.ProjectileType<ArtilleristExplosion>();
            Projectile.NewProjectile(
                projectile.GetSource_FromThis(),
                explosionPos,
                Vector2.Zero,
                explosionType,
                explosionDamage,
                2f,
                owner.whoAmI
            );
        }
        

        public override void OnKill(Projectile projectile,int timeleft)
        {
            projectileSourceMap.Remove(projectile.whoAmI);
            triggeredProjectiles.Remove(projectile.whoAmI);
        }

        // 玩家退出世界时清理所有数据
        public override void Unload()
        {
            projectileSourceMap.Clear();
            triggeredProjectiles.Clear();
        }
    }
}