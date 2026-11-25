using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 手持物品的使用时间快照，供诊断窗口直接读取。
    /// </summary>
    /// <remarks>
    /// 攻速必须拆成两层看，否则窗口上的百分比和玩家实际堆的装备对不上：
    /// <list type="bullet">
    /// <item><b>加算</b>：<see cref="AdditiveSpeed"/> = <see cref="Player.GetTotalAttackSpeed"/>，各来源直接相加</item>
    /// <item><b>乘算</b>：<see cref="AttackSpeedFactor"/>（武器系数）、<see cref="SpeedHookMult"/>、
    /// <see cref="TimeHookMult"/>、<see cref="AnimationHookMult"/> 彼此相乘</item>
    /// </list>
    /// <see cref="FinalSpeed"/> = 1 + (加算 - 1) × 武器系数。
    /// </remarks>
    public readonly struct HeldItemStatsData
    {
        /// <summary>是否拿到了可统计的手持物品（空气 / useTime 为 0 时为 false）。</summary>
        public bool HasItem { get; init; }

        /// <summary>物品显示名。</summary>
        public string ItemName { get; init; }

        /// <summary>物品自身的使用时间（不受任何倍率影响）。</summary>
        public int RawUseTime { get; init; }

        /// <summary>受当前攻速与各类钩子倍率影响后的实际使用时间。</summary>
        public int ActualUseTime { get; init; }

        /// <summary>物品自身的动画时间。</summary>
        public int RawAnimation { get; init; }

        /// <summary>受当前攻速与各类钩子倍率影响后的实际动画时间。</summary>
        public int ActualAnimation { get; init; }

        /// <summary>加算攻速倍率（各来源相加，基底 1.0）。</summary>
        public float AdditiveSpeed { get; init; }

        /// <summary>武器自身的乘算攻速系数（<see cref="ItemID.Sets.BonusAttackSpeedMultiplier"/>）。</summary>
        public float AttackSpeedFactor { get; init; }

        /// <summary>乘算的使用速度倍率（玩家 × 物品钩子）。</summary>
        public float SpeedHookMult { get; init; }

        /// <summary>乘算的使用时间倍率（玩家 × 物品钩子）。</summary>
        public float TimeHookMult { get; init; }

        /// <summary>乘算的动画时间倍率（玩家 × 物品钩子）。</summary>
        public float AnimationHookMult { get; init; }

        /// <summary>加算再乘上武器系数后真正进入公式的攻速倍率。</summary>
        public float FinalSpeed { get; init; }

        /// <summary>攻速是否参与使用时间计算（部分物品声明攻速只影响动画）。</summary>
        public bool SpeedAffectsUseTime { get; init; }

        /// <summary>武器攻速系数为 0，攻速对该物品完全无效。</summary>
        public bool AttackSpeedIgnored { get; init; }

        /// <summary>要维持当前使用时间，加算攻速至少要达到此值（已量化到 0.01%）。</summary>
        public float KeepAdditiveSpeed { get; init; }

        /// <summary>要让使用时间再快一帧，加算攻速至少要达到此值（已量化到 0.01%）。</summary>
        public float NextAdditiveSpeed { get; init; }

        /// <summary>使用时间是否还存在「再快一帧」的空间（1 帧已是最快）。</summary>
        public bool HasNextLevel { get; init; }

        /// <summary>要维持当前动画帧数，加算攻速至少要达到此值（已量化到 0.01%）。</summary>
        public float KeepAnimationAdditiveSpeed { get; init; }

        /// <summary>要让动画再快一帧，加算攻速至少要达到此值（已量化到 0.01%）。</summary>
        public float NextAnimationAdditiveSpeed { get; init; }

        /// <summary>动画是否还存在「再快一帧」的空间（1 帧已是最快）。</summary>
        public bool HasNextAnimationLevel { get; init; }
    }

    /// <summary>
    /// 把手持物品的使用时间 / 动画时间 / 攻速阈值算成一份快照。
    /// </summary>
    /// <remarks>
    /// 公式必须与 <see cref="CombinedHooks.TotalUseTimeMultiplier"/>、
    /// <see cref="CombinedHooks.TotalAnimationTime"/> 保持一致，
    /// 否则窗口里的数字和游戏内实际手感会对不上。
    /// </remarks>
    public static class HeldItemStatsCalculator
    {
        public static HeldItemStatsData Compute(Player player)
        {
            Item item = player?.HeldItem;

            if (item is null || item.IsAir || item.useTime <= 0)
                return default;

            // 玩家 / 物品两侧的钩子倍率（不含攻速本身）
            float timeHookMult = PlayerLoader.UseTimeMultiplier(player, item) * ItemLoader.UseTimeMultiplier(item, player);
            float speedHookMult = PlayerLoader.UseSpeedMultiplier(player, item) * ItemLoader.UseSpeedMultiplier(item, player);
            float animationHookMult = PlayerLoader.UseAnimationMultiplier(player, item) * ItemLoader.UseAnimationMultiplier(item, player);

            // 加算部分：各来源直接相加，基底 1.0
            float additiveSpeed = player.GetTotalAttackSpeed(item.DamageType);

            // 乘算部分：武器系数把「加算增量」缩放后，才是真正进入公式的倍率
            float attackSpeedFactor = ItemID.Sets.BonusAttackSpeedMultiplier[item.type];
            float finalSpeed = player.GetWeaponAttackSpeed(item);

            bool speedAffectsUseTime = !item.attackSpeedOnlyAffectsWeaponAnimation;
            bool attackSpeedIgnored = attackSpeedFactor <= 0f;

            float totalUseTimeMult = timeHookMult;
            if (speedAffectsUseTime)
                totalUseTimeMult /= speedHookMult * finalSpeed;

            int actualUseTime = Math.Max(1, (int)(item.useTime * totalUseTimeMult));

            // 动画时间交给框架算：内部还有一层 relativeUseAnimation 取整，自己复刻容易差一帧
            float animationExtra = item.createTile >= 0 ? player.tileSpeed
                : item.createWall >= 0 ? player.wallSpeed
                : 1f;
            int actualAnimation = CombinedHooks.TotalAnimationTime(item.useAnimation * animationExtra, player, item);

            bool hasNextLevel = actualUseTime > 1;
            bool hasNextAnimationLevel = actualAnimation > 1;
            bool canSolve = !attackSpeedIgnored && speedHookMult > 0f;

            float keepAdditiveSpeed = additiveSpeed;
            float nextAdditiveSpeed = additiveSpeed;
            float keepAnimationAdditiveSpeed = additiveSpeed;
            float nextAnimationAdditiveSpeed = additiveSpeed;

            if (canSolve)
            {
                float upperBound = Math.Max(3f, additiveSpeed * 8f + 3f);

                if (speedAffectsUseTime)
                {
                    keepAdditiveSpeed = SearchMinAdditive(
                        a => SimulateUseTime(item, timeHookMult, speedHookMult, ToFinalSpeed(a, attackSpeedFactor)),
                        actualUseTime,
                        upperBound);

                    if (hasNextLevel)
                    {
                        nextAdditiveSpeed = SearchMinAdditive(
                            a => SimulateUseTime(item, timeHookMult, speedHookMult, ToFinalSpeed(a, attackSpeedFactor)),
                            actualUseTime - 1,
                            upperBound);
                    }
                }

                keepAnimationAdditiveSpeed = SearchMinAdditive(
                    a => SimulateAnimationTime(item, animationHookMult, speedHookMult, ToFinalSpeed(a, attackSpeedFactor), animationExtra),
                    actualAnimation,
                    upperBound);

                if (hasNextAnimationLevel)
                {
                    nextAnimationAdditiveSpeed = SearchMinAdditive(
                        a => SimulateAnimationTime(item, animationHookMult, speedHookMult, ToFinalSpeed(a, attackSpeedFactor), animationExtra),
                        actualAnimation - 1,
                        upperBound);
                }
            }

            return new HeldItemStatsData
            {
                HasItem = true,
                ItemName = item.Name,
                RawUseTime = item.useTime,
                ActualUseTime = actualUseTime,
                RawAnimation = item.useAnimation,
                ActualAnimation = actualAnimation,
                AdditiveSpeed = additiveSpeed,
                AttackSpeedFactor = attackSpeedFactor,
                SpeedHookMult = speedHookMult,
                TimeHookMult = timeHookMult,
                AnimationHookMult = animationHookMult,
                FinalSpeed = finalSpeed,
                SpeedAffectsUseTime = speedAffectsUseTime,
                AttackSpeedIgnored = attackSpeedIgnored,
                KeepAdditiveSpeed = keepAdditiveSpeed,
                NextAdditiveSpeed = nextAdditiveSpeed,
                HasNextLevel = hasNextLevel,
                KeepAnimationAdditiveSpeed = keepAnimationAdditiveSpeed,
                NextAnimationAdditiveSpeed = nextAnimationAdditiveSpeed,
                HasNextAnimationLevel = hasNextAnimationLevel,
            };
        }

        /// <summary>
        /// 在「加算攻速」上二分，求让帧数不超过 <paramref name="targetFrames"/> 的最小值（量化到 0.01%）。
        /// </summary>
        /// <remarks>
        /// 用二分而不是解析式：动画时间中间有两层 <c>(int)</c> 截断，没有可反解的闭式。
        /// 另外加算攻速在游戏里只能以 0.01% 为步进增长，所以最终还要落到 0.01% 网格上回代验证，
        /// 返回值本身就是「刚好再多 0.01% 就跨过门槛」的那个攻速。
        /// </remarks>
        private static float SearchMinAdditive(Func<float, int> framesAt, int targetFrames, float upperBound)
        {
            const float Step = 0.0001f;
            const float LowerBound = 0.1f;

            // 帧数随加算攻速单调不增，上界都到不了就说明目标不可能
            if (framesAt(upperBound) > targetFrames)
                return float.PositiveInfinity;

            if (framesAt(LowerBound) <= targetFrames)
                return LowerBound;

            float low = LowerBound;
            float high = upperBound;

            for (int i = 0; i < 48; i++)
            {
                float mid = (low + high) * 0.5f;

                if (framesAt(mid) <= targetFrames)
                    high = mid;
                else
                    low = mid;
            }

            // 二分结果是连续值，向上落到 0.01% 网格再回代验证
            long stepIndex = (long)Math.Ceiling(high / Step);

            for (int guard = 0; guard < 64; guard++)
            {
                float candidate = stepIndex * Step;

                if (framesAt(candidate) <= targetFrames)
                    return candidate;

                stepIndex++;
            }

            return high;
        }

        private static float ToFinalSpeed(float additiveSpeed, float attackSpeedFactor)
        {
            // 攻速被严重削减时别让倍率跑到 0 以下，否则除法翻号会破坏单调性
            return Math.Max(0.01f, 1f + (additiveSpeed - 1f) * attackSpeedFactor);
        }

        /// <summary>按与 <see cref="CombinedHooks.TotalUseTimeMultiplier"/> 相同的方式复算使用时间。</summary>
        private static int SimulateUseTime(Item item, float timeHookMult, float speedHookMult, float finalSpeed)
        {
            float useTimeMult = timeHookMult / (speedHookMult * finalSpeed);
            return Math.Max(1, (int)(item.useTime * useTimeMult));
        }

        /// <summary>按与 <see cref="CombinedHooks.TotalAnimationTime"/> 相同的方式复算动画时间。</summary>
        /// <remarks>
        /// 源码在这里用的是 <c>useTime * (1 / useSpeed)</c> 而不是 <c>useTime / useSpeed</c>，
        /// 两种写法浮点结果可能差 1（CombinedHooks 的 #3179 注释就是踩过这个坑），必须逐字照抄顺序。
        /// </remarks>
        private static int SimulateAnimationTime(Item item, float animationHookMult, float speedHookMult, float finalSpeed, float extra)
        {
            int multipliedUseTime = Math.Max(1, (int)(item.useTime * (1f / (speedHookMult * finalSpeed))));
            int relativeUseAnimation = Math.Max(1, (int)(multipliedUseTime * item.useAnimation / item.useTime));

            float animationMult = animationHookMult;
            animationMult *= relativeUseAnimation / (float)item.useAnimation;

            return Math.Max(1, (int)(item.useAnimation * extra * animationMult));
        }

        /// <summary>把倍率格式化成带符号百分比，保留两位小数，如 <c>+7.09%</c>。</summary>
        public static string Percent2(float multiplier)
        {
            return string.Format("{0:+0.00;-0.00;0.00}%", (multiplier - 1f) * 100f);
        }
    }
}
