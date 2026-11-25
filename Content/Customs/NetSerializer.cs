using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// 标记该字段需要自动跨端同步。
    /// <para>
    /// 本标记**本身不做任何事**——它只是一份「序列化清单」的记录。
    /// 真正读它的是 <see cref="NetSerializer"/>，调用时机来自
    /// <see cref="AutoSyncProjectile"/> / <see cref="AutoSyncNPC"/> / <see cref="AutoSyncItem"/>
    /// 这三个全局钩子。所以「标上就同步」的前提是那三个钩子存在。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SyncAttribute : Attribute { }

    /// <summary>
    /// 把标了 <see cref="SyncAttribute"/> 的字段写成字节 / 从字节还原。
    /// <para>
    /// 字段清单在首次遇到该类型时反射一次并缓存，之后只做一次字典查找，
    /// 因此可以在每次同步时调用。
    /// </para>
    /// <para>
    /// 这是<b>全量</b>同步：每次把所有同步字段都写一遍。
    /// 字段少（≤4 个）时全量反而比增量省 —— 增量要额外付出位掩码的那几个字节。
    /// </para>
    /// </summary>
    public static class NetSerializer
    {
        private static readonly Dictionary<Type, FieldInfo[]> Cache = new Dictionary<Type, FieldInfo[]>();

        /// <summary>该类型是否有任何需要同步的字段。结果已缓存，可每帧调用。</summary>
        /// <param name="type">待检查的类型。</param>
        /// <returns>至少有一个字段标了 <see cref="SyncAttribute"/> 时返回 <see langword="true"/>。</returns>
        public static bool HasSync(Type type) => Members(type).Length > 0;

        /// <summary>
        /// 该字段类型能否被本序列化器写出 / 读回。
        /// <para>供加载期审计使用：标了 <see cref="SyncAttribute"/> 但类型不支持时，应在启动时就看到报错，
        /// 而不是等到联机时才抛异常。</para>
        /// </summary>
        /// <param name="type">字段的声明类型。</param>
        /// <returns>支持时返回 <see langword="true"/>。</returns>
        public static bool IsSupported(Type type)
        {
            if (type.IsEnum)
                return true;

            return type == typeof(int)
                || type == typeof(float)
                || type == typeof(bool)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(long)
                || type == typeof(double)
                || type == typeof(string)
                || type == typeof(Vector2)
                || type == typeof(Color);
        }

        private static FieldInfo[] Members(Type type)
        {
            if (Cache.TryGetValue(type, out FieldInfo[] cached))
                return cached;

            // DeclaredOnly：不继承基类字段，避免把 ModItem / ModProjectile 内部的字段卷进来。
            // Ordinal 排序：两端必须用同一个顺序，不能依赖反射的返回顺序。
            FieldInfo[] found = type
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(field => field.GetCustomAttribute<SyncAttribute>() != null)
                .OrderBy(field => field.Name, StringComparer.Ordinal)
                .ToArray();

            Cache[type] = found;
            return found;
        }

        /// <summary>按固定顺序写入全部同步字段。</summary>
        /// <param name="target">要序列化的对象实例（不能为 <see langword="null"/>）。</param>
        /// <param name="writer">目标写入器。</param>
        public static void Write(object target, BinaryWriter writer)
        {
            foreach (FieldInfo field in Members(target.GetType()))
                WriteValue(writer, field.FieldType, field.GetValue(target));
        }

        /// <summary>按同一个固定顺序读回并赋值。</summary>
        /// <param name="target">要还原的对象实例（不能为 <see langword="null"/>）。</param>
        /// <param name="reader">来源读取器。</param>
        public static void Read(object target, BinaryReader reader)
        {
            foreach (FieldInfo field in Members(target.GetType()))
                field.SetValue(target, ReadValue(reader, field.FieldType));
        }

        private static void WriteValue(BinaryWriter writer, Type type, object value)
        {
            if (type.IsEnum)
            {
                writer.Write(Convert.ToInt64(value));
                return;
            }

            switch (value)
            {
                case int intValue:
                    writer.Write(intValue);
                    break;
                case float floatValue:
                    writer.Write(floatValue);
                    break;
                case bool boolValue:
                    writer.Write(boolValue);
                    break;
                case byte byteValue:
                    writer.Write(byteValue);
                    break;
                case short shortValue:
                    writer.Write(shortValue);
                    break;
                case long longValue:
                    writer.Write(longValue);
                    break;
                case double doubleValue:
                    writer.Write(doubleValue);
                    break;
                case string text:
                    writer.Write(text ?? string.Empty);
                    break;
                case Vector2 vector:
                    writer.Write(vector.X);
                    writer.Write(vector.Y);
                    break;
                case Color color:
                    writer.Write(color.PackedValue);
                    break;
                default:
                    throw new NotSupportedException(
                        $"[NetSerializer] 不支持同步类型 {type.Name}。" +
                        "请改用基础类型，或在 WriteValue/ReadValue 里补一个分支。");
            }
        }

        private static object ReadValue(BinaryReader reader, Type type)
        {
            if (type.IsEnum)
                return Enum.ToObject(type, reader.ReadInt64());

            if (type == typeof(int))
                return reader.ReadInt32();
            if (type == typeof(float))
                return reader.ReadSingle();
            if (type == typeof(bool))
                return reader.ReadBoolean();
            if (type == typeof(byte))
                return reader.ReadByte();
            if (type == typeof(short))
                return reader.ReadInt16();
            if (type == typeof(long))
                return reader.ReadInt64();
            if (type == typeof(double))
                return reader.ReadDouble();
            if (type == typeof(string))
                return reader.ReadString();
            if (type == typeof(Vector2))
                return new Vector2(reader.ReadSingle(), reader.ReadSingle());
            if (type == typeof(Color))
                return new Color { PackedValue = reader.ReadUInt32() };

            throw new NotSupportedException(
                $"[NetSerializer] 不支持同步类型 {type.Name}。" +
                "请改用基础类型，或在 WriteValue/ReadValue 里补一个分支。");
        }
    }
}
