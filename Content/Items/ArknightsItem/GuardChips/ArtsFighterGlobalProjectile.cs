using Terraria;
using Terraria.ModLoader;
using ExpansionKele.Content.Customs;
using Terraria.DataStructures;

namespace ExpansionKele.Content.Items.ArknightsItem.GuardChips
{
    /// <summary>
    /// 术战者（ArtsFighter）天赋的弹幕处理类。
    /// 效果：
    /// 1. 所有来自术战者刻印武器的弹幕 → 伤害类型转为法术伤害
    /// </summary>
    public class ArtsFighterGlobalProjectile : GlobalProjectile
    {
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem statsSource)
            {
                Item sourceItem = statsSource.Item;
                if (sourceItem == null || sourceItem.IsAir) return;

                int arkNum = ArknightsGlobalItem.GetArknightsNum(sourceItem);
                if (arkNum == 0) return;

                GuardSubclass subclass = ArknightsGlobalItem.GetGuardSubclass(sourceItem);
                if (subclass == GuardSubclass.ArtsFighter)
                {
                    // ★ 直接在生成时改为法术伤害
                    projectile.DamageType = DamageClass.Magic;
                }
            }
        }
    }
}