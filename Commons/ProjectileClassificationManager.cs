using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using ExpansionKele.Content.Projectiles.MeleeProj;
using ExpansionKele.Content.Projectiles.RangedProj;
using ExpansionKele.Content.Projectiles.MagicProj;
using ExpansionKele.Content.Projectiles.SummonProj;
using ExpansionKele.Content.Projectiles.GenericProj;
using Microsoft.Build.Evaluation;

namespace ExpansionKele.Commons
{
    public enum DamageType
    {
        Melee,
        Ranged,
        Magic,
        Summon
    }

    public static class ProjectileClassificationManager
    {
        // 分类存储不同伤害类型的抛射体ID
        public static List<int> MeleeProjectiles { get; private set; } = new List<int>();
        public static List<int> RangedProjectiles { get; private set; } = new List<int>();
        public static List<int> MagicProjectiles { get; private set; } = new List<int>();
        public static List<int> SummonProjectiles { get; private set; } = new List<int>();
        public static List<int> GenericProjectiles { get; private set; } = new List<int>();

        /// <summary>
        /// 初始化所有分类的抛射体列表
        /// </summary>
        public static void InitializeProjectileClassifications()
        {
            InitializeMeleeProjectiles();
            InitializeRangedProjectiles();
            InitializeMagicProjectiles();
            InitializeSummonProjectiles();
        }

        /// <summary>
        /// 获取指定伤害类型的随机有效抛射体
        /// </summary>
        /// <param name="damageType">伤害类型</param>
        /// <returns>抛射体ID，如果找不到则返回子弹ID</returns>
        public static int GetRandomProjectileByDamageType(DamageType damageType)
        {
            List<int> projectileList = GetProjectileListByDamageType(damageType);
            
            if (projectileList == null || projectileList.Count == 0)
            {
                return ProjectileID.Bullet;
            }

            // 尝试多次随机获取有效的抛射体
            for (int i = 0; i < 10; i++)
            {
                int randomIndex = Main.rand.Next(projectileList.Count);
                int projectileType = projectileList[randomIndex];
                
                if (IsValidProjectile(projectileType))
                {
                    return projectileType;
                }
            }

            // 如果随机获取失败，则顺序查找第一个有效的
            foreach (int projectileType in projectileList)
            {
                if (IsValidProjectile(projectileType))
                {
                    return projectileType;
                }
            }

            return ProjectileID.Bullet;
        }

        /// <summary>
        /// 根据伤害类型获取对应的抛射体列表
        /// </summary>
        /// <param name="damageType">伤害类型</param>
        /// <returns>抛射体ID列表</returns>
        public static List<int> GetProjectileListByDamageType(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Melee => MeleeProjectiles,
                DamageType.Ranged => RangedProjectiles,
                DamageType.Magic => MagicProjectiles,
                DamageType.Summon => SummonProjectiles,
                _ => new List<int> { ProjectileID.Bullet }
            };
        }

        /// <summary>
        /// 检查抛射体ID是否有效
        /// </summary>
        /// <param name="projectileType">抛射体ID</param>
        /// <returns>是否有效</returns>
        private static bool IsValidProjectile(int projectileType)
        {
            return projectileType > 0 && ContentSamples.ProjectilesByType.ContainsKey(projectileType);
        }

        #region 初始化各分类抛射体

        private static void InitializeMeleeProjectiles()
        {
            MeleeProjectiles.Clear();
            
            // 添加近战抛射体类型
            AddProjectileIfExists<ChromiumSwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<ColaProjectile>(MeleeProjectiles);
            AddProjectileIfExists<ColaProjectileLower>(MeleeProjectiles);
            AddProjectileIfExists<EnergySwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<EnergySwordProjectileLinear>(MeleeProjectiles);
            AddProjectileIfExists<FloatingMushroomProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FragmentsEmergenceHitProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FragmentsEmergenceProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonShortSwordMoonProj>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonShortSwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonSpearHeadProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonSpearMoonProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonSpearProjectile>(MeleeProjectiles);
            AddProjectileIfExists<FullMoonSwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<KillingsBladeThrownProjectile>(MeleeProjectiles);
            AddProjectileIfExists<LifePercentageProjectile>(MeleeProjectiles);
            //AddProjectileIfExists<LightSpearProjectile>(MeleeProjectiles);
            AddProjectileIfExists<ShroomiteSwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<SolarFireball>(MeleeProjectiles);
            AddProjectileIfExists<SpectreSwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<SplitSolarFireball>(MeleeProjectiles);
            AddProjectileIfExists<SuperGearProjectile>(MeleeProjectiles);
            AddProjectileIfExists<TangDynastySwordEnergyProjectile>(MeleeProjectiles);
            AddProjectileIfExists<TangDynastySwordProjectile>(MeleeProjectiles);
            AddProjectileIfExists<ThoughtsCrossBladeMiniProjectile>(MeleeProjectiles);
            AddProjectileIfExists<ThoughtsCrossBladeProjectile>(MeleeProjectiles);
        }

        private static void InitializeRangedProjectiles()
        {
            RangedProjectiles.Clear();
            
            // 添加远程抛射体类型
            AddProjectileIfExists<AAMissile>(RangedProjectiles);
            AddProjectileIfExists<AutoAimingMarker>(RangedProjectiles);
            AddProjectileIfExists<AutoAimingSniperBullet>(RangedProjectiles);
            AddProjectileIfExists<BalloonArrowProjectile>(RangedProjectiles);
            AddProjectileIfExists<BurningPaperAirPlaneProjectile>(RangedProjectiles);
            AddProjectileIfExists<ChromiumArrowProjectile>(RangedProjectiles);
            AddProjectileIfExists<DotZeroFiveSniperBullet>(RangedProjectiles);
            AddProjectileIfExists<DotZeroFiveSniperCrosshair>(RangedProjectiles);
            AddProjectileIfExists<EMPBallProjectile>(RangedProjectiles);
            AddProjectileIfExists<EMPWave>(RangedProjectiles);
            AddProjectileIfExists<ElectricBallProjectile>(RangedProjectiles);
            AddProjectileIfExists<ElectricBallistaShockProjectile>(RangedProjectiles);
            AddProjectileIfExists<FadeProjectile>(RangedProjectiles);
            AddProjectileIfExists<FrostRayProjectile>(RangedProjectiles);
            AddProjectileIfExists<FullMoonArrowProj>(RangedProjectiles);
            AddProjectileIfExists<GaussRifleProjectile>(RangedProjectiles);
            AddProjectileIfExists<IronCurtainCannonLaser>(RangedProjectiles);
            AddProjectileIfExists<IronCurtainCannonProjectile>(RangedProjectiles);
            AddProjectileIfExists<LaserCutterProjectile>(RangedProjectiles);
            //AddProjectileIfExists<ProtonCannonHoldOut>(RangedProjectiles);//蓄力抛射体会有bug
            AddProjectileIfExists<RedemptionArrowProjectile>(RangedProjectiles);
            AddProjectileIfExists<RedemptionBulletProjectile>(RangedProjectiles);
            AddProjectileIfExists<SeitaadBallistaProjectile>(RangedProjectiles);
            AddProjectileIfExists<SeitaadBallistaShockProjectile>(RangedProjectiles);
            AddProjectileIfExists<SelfRedemptionProjectile>(RangedProjectiles);
            AddProjectileIfExists<SharkyBullet>(RangedProjectiles);
            AddProjectileIfExists<SharkyBulletPlus>(RangedProjectiles);
            AddProjectileIfExists<SixthSniperBullet>(RangedProjectiles);
            //AddProjectileIfExists<SoulCannonHoldOut>(RangedProjectiles);//蓄力抛射体会有bug
            AddProjectileIfExists<SpectralCurtainCannonProj>(RangedProjectiles);
            AddProjectileIfExists<StingerProjectile>(RangedProjectiles);
            AddProjectileIfExists<VortexMainProjectile>(RangedProjectiles);
            AddProjectileIfExists<VortexHomingProjectile>(RangedProjectiles);
        }

        private static void InitializeMagicProjectiles()
        {
            MagicProjectiles.Clear();
            
            // 添加魔法抛射体类型
            AddProjectileIfExists<AnaxaMagicTrickProjectile>(MagicProjectiles);
            AddProjectileIfExists<EnhancedLaserProjectile>(MagicProjectiles);
            AddProjectileIfExists<EnhancedLaserSplitProjectile>(MagicProjectiles);
            AddProjectileIfExists<FullMoonEchoProj>(MagicProjectiles);
            AddProjectileIfExists<MagicBlueProjectile>(MagicProjectiles);
            AddProjectileIfExists<MagicCyanProjectile>(MagicProjectiles);
            AddProjectileIfExists<MagicPurpleProjectile>(MagicProjectiles);
            AddProjectileIfExists<MagicRedProjectile>(MagicProjectiles);
            AddProjectileIfExists<MagicStarProjectile>(MagicProjectiles);
            AddProjectileIfExists<NightFireflyProjectile>(MagicProjectiles);
            AddProjectileIfExists<NoMoreRovingProjectile>(MagicProjectiles);
            AddProjectileIfExists<ResentmentProjectile>(MagicProjectiles);
            AddProjectileIfExists<RippleProjectile>(MagicProjectiles);
        }

        private static void InitializeSummonProjectiles()
        {
            SummonProjectiles.Clear();
            
            // 添加召唤抛射体类型
            AddProjectileIfExists<FullMoonMinion>(SummonProjectiles);
            AddProjectileIfExists<GoldenWeaverMinion>(SummonProjectiles);
            AddProjectileIfExists<SpaceFunnelBeam>(SummonProjectiles);
            AddProjectileIfExists<SpaceFunnelMinion>(SummonProjectiles);
        }

        // private static void InitializeGenericProjectiles()
        // {
        //     GenericProjectiles.Clear();
            
        //     // 添加通用抛射体类型（基础游戏抛射体）
        //     GenericProjectiles.AddRange(new int[]
        //     {
        //         ProjectileID.Bullet,
        //         ProjectileID.Fireball,
        //         ProjectileID.MagicMissile,
        //         ProjectileID.DeathLaser,
        //         ProjectileID.EyeLaser,
        //         ProjectileID.Grenade,
        //         ProjectileID.Bomb,
        //         ProjectileID.Flamarang,
        //         ProjectileID.WoodenArrowFriendly,
        //         ProjectileID.FireArrow,
        //         ProjectileID.UnholyArrow,
        //         ProjectileID.JestersArrow,
        //         ProjectileID.HellfireArrow
        //     });
        // }

        #endregion

        /// <summary>
        /// 安全地添加抛射体类型，避免不存在的类型导致错误
        /// </summary>
        /// <typeparam name="T">抛射体类型</typeparam>
        /// <param name="list">目标列表</param>
        private static void AddProjectileIfExists<T>(List<int> list) where T : ModProjectile
        {
            try
            {
                int type = ModContent.ProjectileType<T>();
                if (type > 0)
                {
                    list.Add(type);
                }
            }
            catch
            {
                // 忽略无法加载的抛射体类型
            }
        }

        /// <summary>
        /// 获取所有分类抛射体的统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public static string GetClassificationStatistics()
        {
            return $"近战: {MeleeProjectiles.Count}, 远程: {RangedProjectiles.Count}, 魔法: {MagicProjectiles.Count}, 召唤: {SummonProjectiles.Count}";
        }
    }
}