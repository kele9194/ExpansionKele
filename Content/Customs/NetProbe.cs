using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 多人同步探针（开发期工具，不参与任何游戏逻辑）。
    /// <para>
    /// 目的：在不启动多人环境的前提下，尽量早地暴露「标了 [Sync] 但其实不会同步」的情况。
    /// 在 <see cref="PostSetupContent"/> 里用反射扫描整个程序集，一次性给出结论。
    /// </para>
    /// <para>
    /// 同步链路有三个环节，缺一不可：
    /// <list type="number">
    /// <item><see cref="SyncAttribute"/> 标记字段 —— 只是声明，本身不做任何事。</item>
    /// <item><see cref="NetSerializer"/> 把标记字段写成字节 / 读回。</item>
    /// <item><see cref="AutoSyncProjectile"/> / <see cref="AutoSyncNPC"/> / <see cref="AutoSyncItem"/>
    /// 在全局钩子里调用它，从而覆盖全部弹幕 / NPC / 物品。</item>
    /// </list>
    /// 所以「标上就同步」成立的前提是第 3 步覆盖得到该类型 —— 本工具检查的正是这个前提。
    /// </para>
    /// <para>
    /// 注意 <c>ModPlayer</c> 不在此列：TML 没有 GlobalPlayer 这类全局钩子，
    /// 玩家状态必须自己实现 <c>SyncPlayer</c> / <c>CopyClientState</c> / <c>SendClientChanges</c>
    /// （<c>ModPlayer.cs:178/164/188</c>），实际发包还得自己用 ModPacket。
    /// </para>
    /// <para>
    /// WARNING: 本工具不能替代真实联机测试 -- 它无法覆盖「发送时机」「客户端预测/插值」
    /// 「主机与专用服务器差异」这几类问题。
    /// </para>
    /// </summary>
    public sealed class NetProbe : ModSystem
    {
        /// <summary>总开关。不想看日志时改为 <see langword="false"/>。</summary>
        public const bool Enabled = true;

        private const BindingFlags DeclaredInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private int _errorCount;
        private int _warningCount;

        /// <summary>
        /// 未覆写任何同步钩子的 ModPlayer 名字。
        /// 本项目这类类型有几十个，逐个报会淹没真正的问题，所以最后汇总成一行输出。
        /// </summary>
        private readonly List<string> _silentModPlayers = new List<string>();

        // ======================== 加载期审计 ========================

        /// <summary>内容加载完成后执行一次静态审计，结果写入 client.log / 服务器日志。</summary>
        public override void PostSetupContent()
        {
            // 即使 NetProbe 自身关闭，也允许 DevGuard 独立工作
            DevGuard.Init(Mod);

            if (!Enabled)
                return;

            _errorCount = 0;
            _warningCount = 0;
            _silentModPlayers.Clear();

            try
            {
                // 官方注释：不要对 Mod.Code 直接调用 GetTypes()（弱引用模组会抛异常），
                // 必须走 AssemblyManager.GetLoadableTypes（AssemblyManager.cs:347）。
                foreach (Type type in AssemblyManager.GetLoadableTypes(Mod.Code))
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    AuditSyncFields(type);
                    AuditGlobalItem(type);
                    AuditModPlayer(type);
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                Mod.Logger.Error("[NetProbe] 类型加载失败，审计中止：" + e);
            }
            catch (Exception e)
            {
                Mod.Logger.Error("[NetProbe] 审计异常：" + e);
            }

            // 未接线的 ModPlayer 单独汇总成一行，避免几十条 INFO 淹没真正的错误
            if (_silentModPlayers.Count > 0)
            {
                _warningCount += _silentModPlayers.Count;
                Mod.Logger.Info(
                    $"[NetProbe][INFO] 共 {_silentModPlayers.Count} 个 ModPlayer 未覆写任何同步钩子" +
                    "（多数是纯本地状态，两端各自计算即可；ModPlayer 不受 AutoSync 覆盖）：" +
                    string.Join("、", _silentModPlayers));
            }

            Mod.Logger.Info($"[NetProbe] L1 审计结束：{_errorCount} 个错误、{_warningCount} 个警告。");
        }

        /// <summary>
        /// 检查 [Sync] 是否真的会被自动化通道读取。
        /// <para>两类问题：宿主类型不在 AutoSync 覆盖范围内；字段类型无法被 NetSerializer 序列化。</para>
        /// </summary>
        private void AuditSyncFields(Type type)
        {
            FieldInfo[] marked = GetSyncFields(type);
            if (marked.Length == 0)
                return;

            bool covered = typeof(ModProjectile).IsAssignableFrom(type)
                || typeof(ModNPC).IsAssignableFrom(type)
                || typeof(ModItem).IsAssignableFrom(type);

            string names = string.Join("、", marked.Select(field => field.Name));

            if (!covered)
            {
                _errorCount++;
                Mod.Logger.Error(
                    $"[NetProbe][ERROR] {type.FullName} 的 {names} 标记了 [Sync]，" +
                    "但该类不是 ModProjectile / ModNPC / ModItem —— AutoSync 只覆盖这三类，" +
                    "这些标记不会被任何通道读取。");
                return;
            }

            foreach (FieldInfo field in marked)
            {
                if (NetSerializer.IsSupported(field.FieldType))
                    continue;

                _errorCount++;
                Mod.Logger.Error(
                    $"[NetProbe][ERROR] {type.FullName}.{field.Name} 的类型 {field.FieldType.Name} 无法序列化。" +
                    "请在 NetSerializer.IsSupported / WriteValue / ReadValue 里补上支持，或改用基础类型。");
            }
        }

        /// <summary>
        /// 提示：GlobalItem 有实例字段，但既没标 [Sync] 也没覆写 NetSend/NetReceive。
        /// </summary>
        private void AuditGlobalItem(Type type)
        {
            if (!typeof(GlobalItem).IsAssignableFrom(type))
                return;

            FieldInfo[] instanceFields = type.GetFields(DeclaredInstance).Where(field => !field.IsStatic).ToArray();
            if (instanceFields.Length == 0)
                return;

            if (GetSyncFields(type).Length > 0)
                return;

            if (HasDeclaredMethod(type, nameof(GlobalItem.NetSend)) && HasDeclaredMethod(type, nameof(GlobalItem.NetReceive)))
                return;

            Mod.Logger.Info(
                $"[NetProbe][INFO] {type.FullName} 有实例字段（{string.Join("、", instanceFields.Select(field => field.Name))}），" +
                "但既没标 [Sync] 也没覆写 NetSend/NetReceive。" +
                $"（InstancePerEntity：{(HasDeclaredProperty(type, "InstancePerEntity") ? "已覆写" : "未覆写，默认 false")}）");
        }

        /// <summary>
        /// 汇总：ModPlayer 有实例字段但没覆写任何同步钩子。
        /// <para>
        /// ModPlayer 不在 AutoSync 覆盖范围内（TML 没有 GlobalPlayer），
        /// 要让状态跨端必须自己实现 SyncPlayer / CopyClientState / SendClientChanges。
        /// </para>
        /// </summary>
        private void AuditModPlayer(Type type)
        {
            if (!typeof(ModPlayer).IsAssignableFrom(type))
                return;

            FieldInfo[] instanceFields = type.GetFields(DeclaredInstance).Where(field => !field.IsStatic).ToArray();
            if (instanceFields.Length == 0)
                return;

            bool wired = HasDeclaredMethod(type, nameof(ModPlayer.SyncPlayer))
                || HasDeclaredMethod(type, nameof(ModPlayer.CopyClientState))
                || HasDeclaredMethod(type, nameof(ModPlayer.SendClientChanges));

            if (wired)
                return;

            if (GetSyncFields(type).Length > 0)
            {
                _errorCount++;
                Mod.Logger.Error(
                    $"[NetProbe][ERROR] {type.FullName} 标了 [Sync]，但 ModPlayer 不在 AutoSync 覆盖范围内。" +
                    "需要自己实现 SyncPlayer / CopyClientState / SendClientChanges 才能跨端。");
                return;
            }

            _silentModPlayers.Add(type.Name);
        }

        // ======================== 反射工具 ========================

        /// <summary>取该类型自己声明的、标了 <see cref="SyncAttribute"/> 的字段。</summary>
        private static FieldInfo[] GetSyncFields(Type type)
        {
            return type.GetFields(DeclaredInstance)
                .Where(field => field.GetCustomAttribute<SyncAttribute>() != null)
                .ToArray();
        }

        /// <summary>该类型自己是否声明（覆写）了指定方法。</summary>
        private static bool HasDeclaredMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, DeclaredInstance) != null;
        }

        /// <summary>该类型自己是否声明（覆写）了指定属性。</summary>
        private static bool HasDeclaredProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, DeclaredInstance) != null;
        }
    }
}
