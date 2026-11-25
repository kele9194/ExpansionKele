using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 开发期轻量守卫：<b>手动插桩</b>式的「检测 + 遏制」。
    /// <para>
    /// 与 <see cref="NetProbe"/> 的分工：
    /// <list type="bullet">
    /// <item><see cref="NetProbe"/> —— <b>自动</b>扫描，回答「有没有接线」，不需要写调用。</item>
    /// <item><see cref="DevGuard"/> —— <b>手动</b>插桩，回答「这段代码在哪个端跑了、值是多少」，
    /// 并且能在错误端<b>直接拦住</b>执行。</item>
    /// </list>
    /// </para>
    /// <para>
    /// 设计原则：
    /// <list type="number">
    /// <item>零依赖：只依赖 tModLoader，不引用任何前置模组。</item>
    /// <item>可整体删除：发布前把 <see cref="Enabled"/> 改为 <see langword="false"/> 即可，
    /// 由于它是 <c>const</c>，Release 构建下 <c>if (!Enabled) return;</c> 会被编译器直接消除，
    /// 运行期成本为零。</item>
    /// <item>防刷屏：所有输出按「文件:行号」做 1 秒冷却，与本项目 <c>DebugMarker</c> 同一策略。</item>
    /// </list>
    /// </para>
    /// <para>
    /// 典型用法：
    /// <code>
    /// if (!DevGuard.ServerOnly("SpawnBoss")) return;   // 遏制：客户端误调用时直接中止
    /// DevGuard.Net("SubstituteTimeLeft", t);           // 检测：两端日志里对照同一个值
    /// </code>
    /// </para>
    /// </summary>
    public static class DevGuard
    {
        /// <summary>
        /// 开发期总开关。<b>发布前改为 <see langword="false"/></b>（或删除本文件）。
        /// </summary>
        public const bool Enabled = true;

        /// <summary>同一条插桩的最小输出间隔（tick）。</summary>
        private const uint CooldownTicks = 60;

        /// <summary>按「文件:行号」记录上次输出时刻。</summary>
        private static readonly Dictionary<string, uint> _lastPrintTicks = new Dictionary<string, uint>();

        private static Mod _mod;

        /// <summary>
        /// 注入所属模组实例（由 <see cref="NetProbe.PostSetupContent"/> 调用一次）。
        /// 未注入时退化为 <see cref="Console.WriteLine(string)"/>。
        /// </summary>
        /// <param name="mod">当前模组实例。</param>
        public static void Init(Mod mod)
        {
            _mod = mod;
        }

        // ======================== 端判定（遏制） ========================

        /// <summary>
        /// 断言「以下逻辑只能在服务端执行」，否则输出告警并返回 <see langword="false"/>。
        /// <para>
        /// 判据（<c>Terraria/ID/NetmodeID.cs</c>）：单人与服务端都视为「有权力的端」，
        /// 只有多人客户端会被拦下。主机联机时 <c>Main.netMode == NetmodeID.Server</c>，同样放行。
        /// </para>
        /// </summary>
        /// <param name="what">正在做的事（用于日志定位，可省略）。</param>
        /// <param name="file">调用方文件（编译器自动填充）。</param>
        /// <param name="line">调用方行号（编译器自动填充）。</param>
        /// <returns>允许继续执行时返回 <see langword="true"/>。</returns>
        public static bool ServerOnly(
            string what = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!Enabled)
                return true;

            if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer)
                return true;

            Report("ERROR", "ServerOnly", what, file, line,
                $"该逻辑只能在服务端执行，当前 netMode={NetmodeName()}（客户端执行会导致改动只在本地生效）");
            return false;
        }

        /// <summary>
        /// 断言「以下逻辑是纯客户端表现」，在专用服务器上应被跳过，否则输出告警并返回 <see langword="false"/>。
        /// <para>
        /// 判据：<c>Main.dedServ == false</c>。<b>不要用 <c>Main.netMode</c> 判断</b>——
        /// 主机联机时 <c>netMode</c> 为 Server 但主机进程仍有 GPU，粒子与 UI 必须照常运行。
        /// </para>
        /// </summary>
        /// <param name="what">正在做的事（用于日志定位，可省略）。</param>
        /// <param name="file">调用方文件（编译器自动填充）。</param>
        /// <param name="line">调用方行号（编译器自动填充）。</param>
        /// <returns>允许继续执行时返回 <see langword="true"/>。</returns>
        public static bool ClientVisualOnly(
            string what = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!Enabled)
                return true;

            if (!Main.dedServ)
                return true;

            Report("WARN", "ClientVisualOnly", what, file, line,
                "该逻辑是客户端表现，不应在专用服务器上执行（粒子 / 音效 / UI / 绘制）");
            return false;
        }

        // ======================== 输出（检测） ========================

        /// <summary>输出一条带文件行号与冷却的调试标记；只写日志，不弹屏幕。</summary>
        /// <param name="value">要观察的值（可为 <see langword="null"/>）。</param>
        /// <param name="file">调用方文件（编译器自动填充）。</param>
        /// <param name="line">调用方行号（编译器自动填充）。</param>
        public static void Mark(
            object value = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!Enabled)
                return;

            if (!ShouldPrint(KeyOf(file, line, "mark")))
                return;

            Log($"[DevGuard] {Path.GetFileName(file)}:{line} | {value ?? "null"}");
        }

        /// <summary>
        /// 输出当前端身份与一个字段值，用于在两端日志里对照同一个字段。
        /// <para>
        /// 这是「手动指定检测点」的主力：在客户端与服务端都会执行的钩子里插一行，
        /// 就能看出哪一端拿到了错误的值。
        /// </para>
        /// </summary>
        /// <param name="field">字段名或任意标签。</param>
        /// <param name="value">该字段在当前端的值。</param>
        /// <param name="file">调用方文件（编译器自动填充）。</param>
        /// <param name="line">调用方行号（编译器自动填充）。</param>
        public static void Net(
            string field,
            object value,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!Enabled)
                return;

            if (!ShouldPrint(KeyOf(file, line, "net:" + field)))
                return;

            Log($"[DevGuard:Net] {Path.GetFileName(file)}:{line} | " +
                $"netMode={NetmodeName()} myPlayer={Main.myPlayer} dedServ={Main.dedServ} | " +
                $"{field}={value ?? "null"}");
        }

        /// <summary>输出一条观察值到屏幕（专用服务器上自动跳过）。</summary>
        /// <param name="message">要显示的文本。</param>
        /// <param name="color">文字颜色。</param>
        /// <param name="file">调用方文件（编译器自动填充）。</param>
        /// <param name="line">调用方行号（编译器自动填充）。</param>
        public static void Screen(
            string message,
            Color? color = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
        {
            if (!Enabled || Main.dedServ)
                return;

            if (!ShouldPrint(KeyOf(file, line, "screen")))
                return;

            Main.NewText($"[DevGuard] {Path.GetFileName(file)}:{line} | {message}", color ?? Color.Yellow);
        }

        // ======================== 内部实现 ========================

        /// <summary>生成冷却键。</summary>
        private static string KeyOf(string file, int line, string tag)
        {
            return $"{file}:{line}:{tag}";
        }

        /// <summary>按冷却规则判断本次是否应输出。</summary>
        private static bool ShouldPrint(string key)
        {
            uint now = Main.GameUpdateCount;

            if (_lastPrintTicks.TryGetValue(key, out uint last) && now - last < CooldownTicks)
                return false;

            _lastPrintTicks[key] = now;
            return true;
        }

        /// <summary>当前网络模式的简短名称。</summary>
        private static string NetmodeName()
        {
            return Main.netMode switch
            {
                NetmodeID.SinglePlayer => "SinglePlayer",
                NetmodeID.MultiplayerClient => "MultiplayerClient",
                NetmodeID.Server => "Server",
                _ => Main.netMode.ToString()
            };
        }

        /// <summary>统一输出：优先写模组日志，未注入时退回控制台。</summary>
        private static void Log(string message)
        {
            if (_mod != null)
                _mod.Logger.Info(message);
            else
                Console.WriteLine(message);
        }

        /// <summary>输出断言失败信息（日志 + 客户端屏幕）。</summary>
        private static void Report(
            string level,
            string kind,
            string what,
            string file,
            int line,
            string detail)
        {
            string target = string.IsNullOrEmpty(what) ? "" : $" ({what})";
            string message = $"[DevGuard][{level}][{kind}] {Path.GetFileName(file)}:{line}{target} | {detail}";

            Log(message);

            if (!Main.dedServ)
                Main.NewText(message, level == "ERROR" ? Color.OrangeRed : Color.Yellow);
        }
    }
}
