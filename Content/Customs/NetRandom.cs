using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 多人安全的随机数工具。
    /// <para>
    /// 核心原则：<b>不要找「统一的随机数」，要找「统一的决定者」。</b>
    /// </para>
    /// <para>
    /// 两层分法：
    /// <list type="bullet">
    /// <item><b>逻辑随机</b>（选目标、走分支、掉落数量）→ <see cref="IsAuthority"/> 或
    /// <see cref="IsOwner"/> 里掷骰子，把<b>结果</b>同步出去（<c>ai[]</c> 或 <see cref="SyncAttribute"/>）。</item>
    /// <item><b>可复现的随机</b>（锯齿、抖动、dust、颜色微调）→ <see cref="Deterministic"/>，
    /// 两端各自算但结果相同，<b>零网络开销</b>。</item>
    /// </list>
    /// </para>
    /// <para>
    /// 不要把 <see cref="Main.rand"/> 包在这套里 —— 它是全局共享的单实例，
    /// 「被消耗到第几个」是全游戏共享的，包装之后依然是共享序列，两端仍会分叉。
    /// </para>
    /// </summary>
    public static class NetRandom
    {
        /// <summary>本端是否是「有决定权」的一端（单人 / 服务端 / 主机）。</summary>
        /// <remarks>
        /// 判据同 <c>Projectile.NewProjectile</c> 官方注释：NPC 或非玩家来源用这个。
        /// <para>
        /// 只适用于「<b>两端都会跑</b>」的代码（NPC AI、弹幕 AI）。以下钩子全游戏只在一个端执行一次，
        /// 加这个守卫会让功能在多人下<b>直接失效</b>：<c>FreeDodge</c>（<c>Player.cs:31480</c>
        /// 要求 <c>whoAmI == Main.myPlayer</c>）、玩家受击的 <c>ModifyHitByProjectile/NPC</c>、
        /// 攻击方对 NPC 的伤害结算（<c>Projectile.Damage</c> 整段包在 <c>if (owner == Main.myPlayer)</c> 内）。
        /// </para>
        /// </remarks>
        public static bool IsAuthority => Main.netMode != NetmodeID.MultiplayerClient;

        /// <summary>本端的玩家是否是某个弹幕 / 物品的主人。</summary>
        /// <param name="owner">
        /// <c>Projectile.owner</c> 或 <c>Item.playerIndexTheItemIsReservedFor</c>。
        /// </param>
        /// <remarks>
        /// 判据同 <c>Projectile.NewProjectile</c> 官方注释：弹幕生成子弹幕时用这个。
        /// 服务端恒为 false（服务端 <c>Main.myPlayer</c> 是 255），所以服务端不会重复生成。
        /// </remarks>
        public static bool IsOwner(int owner) => Main.myPlayer == owner;

        /// <summary>
        /// 创建一条<b>确定性随机流</b>：用同步数据当种子，两端会得到完全相同的序列。
        /// <para>
        /// 适用条件：所有 <paramref name="seeds"/> 都来自两端一致的数据
        /// （<c>Projectile.identity</c> / <c>ai[]</c> / <c>NPC.whoAmI</c> / 两端口径相同的列表下标）。
        /// </para>
        /// <para>
        /// 不要在种子不变的情况下期待「每帧都不同」—— 它每次都是从种子重新开始，
        /// 所以同一个种子永远给出同一个序列。这对视觉抖动是好事（不会闪烁）。
        /// </para>
        /// </summary>
        /// <param name="seeds">种子，顺序敏感。</param>
        /// <returns>两端序列一致的随机源。</returns>
        public static UnifiedRandom Deterministic(params int[] seeds)
        {
            unchecked
            {
                int hash = 17;
                foreach (int seed in seeds)
                    hash = hash * 31 + seed;
                return new UnifiedRandom(hash);
            }
        }
    }
}
