using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 追踪敌对弹幕的来源NPC。
    /// 在 OnSpawn 中通过 IEntitySource 判断弹幕是否由 NPC 发射，
    /// 记录 projectile.whoAmI → NPC 的映射，供 BlockingPlayer 等查询。
    /// 
    /// 原理：
    /// NPC 发射弹幕时使用 NPC.GetSource_FromAI() / NPC.GetSource_FromThis()，
    /// 它们返回 EntitySource_Parent，其中 Entity 即为发射该弹幕的 NPC。
    /// </summary>
    public class NPCProjectileSourceTracker : GlobalProjectile
    {
        /// <summary>弹幕ID → 来源NPC 的映射表</summary>
        private static readonly Dictionary<int, NPC> NPCSourceMap = new Dictionary<int, NPC>();

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 检查 source 是否为 EntitySource_Parent，且父实体为有效的 NPC
            if (source is EntitySource_Parent parent && parent.Entity is NPC sourceNPC && sourceNPC.active)
            {
                NPCSourceMap[projectile.whoAmI] = sourceNPC;
            }
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            NPCSourceMap.Remove(projectile.whoAmI);
        }

        /// <summary>
        /// 尝试获取发射指定弹幕的源NPC。
        /// </summary>
        public static bool TryGetSourceNPC(Projectile projectile, out NPC sourceNPC)
        {
            if (projectile != null && NPCSourceMap.TryGetValue(projectile.whoAmI, out sourceNPC))
            {
                if (sourceNPC != null && sourceNPC.active)
                    return true;

                // 源NPC已失效，清理记录
                NPCSourceMap.Remove(projectile.whoAmI);
            }

            sourceNPC = null;
            return false;
        }

        public override void Unload()
        {
            NPCSourceMap.Clear();
        }
    }
}