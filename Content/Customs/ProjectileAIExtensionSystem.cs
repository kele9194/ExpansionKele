using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ExpansionKele.Content.Customs
{
    /// <summary>
    /// Projectile AI扩展系统 - 提供超出原生Projectile.ai[]数组的额外AI数据存储
    /// 支持多种数据类型，可在多人游戏中正确同步
    /// </summary>
    public static class ProjectileAIExtensionSystem
    {
        // 存储所有Projectile的扩展AI数据
        private static Dictionary<int, ExtendedAIData> _extendedAIData = new Dictionary<int, ExtendedAIData>();
        
        // 数据类型枚举 - 简化版本
        public enum DataType
        {
            Float,
            Int,      // 统一处理整数类型
            Bool,
            Vector2
        }

        /// <summary>
        /// 扩展AI数据容器
        /// </summary>
        public class ExtendedAIData
        {
            public Dictionary<string, object> Data { get; private set; } = new Dictionary<string, object>();
            public bool IsDirty { get; set; } = false; // 标记数据是否已修改需要同步

            public ExtendedAIData()
            {
                Data = new Dictionary<string, object>();
            }

            // 设置数据方法 - 简化版本
            public void SetFloat(string key, float value)
            {
                Data[key] = value;
                IsDirty = true;
            }

            public void SetInt(string key, int value)
            {
                Data[key] = value;
                IsDirty = true;
            }

            public void SetBool(string key, bool value)
            {
                Data[key] = value;
                IsDirty = true;
            }

            // 兼容方法 - short和byte内部转换为int存储
            public void SetShort(string key, short value)
            {
                Data[key] = (int)value;  // 转换为int存储
                IsDirty = true;
            }

            public void SetByte(string key, byte value)
            {
                Data[key] = (int)value;  // 转换为int存储
                IsDirty = true;
            }

            public void SetVector2(string key, Vector2 value)
            {
                Data[key] = value;
                IsDirty = true;
            }

            // 获取数据方法 - 简化版本
            public float GetFloat(string key, float defaultValue = 0f)
            {
                return Data.TryGetValue(key, out object value) ? (float)value : defaultValue;
            }

            public int GetInt(string key, int defaultValue = 0)
            {
                return Data.TryGetValue(key, out object value) ? (int)value : defaultValue;
            }

            public bool GetBool(string key, bool defaultValue = false)
            {
                return Data.TryGetValue(key, out object value) ? (bool)value : defaultValue;
            }

            // 兼容方法 - 支持获取short和byte类型
            public short GetShort(string key, short defaultValue = 0)
            {
                if (Data.TryGetValue(key, out object value))
                {
                    // 支持从int或short转换
                    if (value is int intValue)
                        return (short)intValue;
                    else if (value is short shortValue)
                        return shortValue;
                }
                return defaultValue;
            }

            public byte GetByte(string key, byte defaultValue = 0)
            {
                if (Data.TryGetValue(key, out object value))
                {
                    // 支持从int或byte转换
                    if (value is int intValue)
                        return (byte)intValue;
                    else if (value is byte byteValue)
                        return byteValue;
                }
                return defaultValue;
            }

            public Vector2 GetVector2(string key, Vector2 defaultValue = default)
            {
                return Data.TryGetValue(key, out object value) ? (Vector2)value : defaultValue;
            }

            // 检查键是否存在
            public bool ContainsKey(string key)
            {
                return Data.ContainsKey(key);
            }

            // 移除键
            public void RemoveKey(string key)
            {
                if (Data.ContainsKey(key))
                {
                    Data.Remove(key);
                    IsDirty = true;
                }
            }

            // 清空所有数据
            public void Clear()
            {
                Data.Clear();
                IsDirty = true;
            }
        }

        /// <summary>
        /// 获取指定Projectile的扩展AI数据
        /// </summary>
        /// <param name="projectile">Projectile实例</param>
        /// <returns>扩展AI数据容器</returns>
        public static ExtendedAIData GetExtendedAIData(this Projectile projectile)
        {
            int projIdentity = projectile.identity;
            
            if (!_extendedAIData.ContainsKey(projIdentity))
            {
                _extendedAIData[projIdentity] = new ExtendedAIData();
            }
            
            return _extendedAIData[projIdentity];
        }

        /// <summary>
        /// 移除指定Projectile的扩展AI数据（通常在Projectile死亡时调用）
        /// </summary>
        /// <param name="projectile">Projectile实例</param>
        public static void RemoveExtendedAIData(this Projectile projectile)
        {
            int projIdentity = projectile.identity;
            if (_extendedAIData.ContainsKey(projIdentity))
            {
                _extendedAIData.Remove(projIdentity);
            }
        }

        /// <summary>
        /// 清理所有扩展AI数据（谨慎使用）
        /// </summary>
        public static void ClearAllExtendedAIData()
        {
            _extendedAIData.Clear();
        }

        /// <summary>
        /// 获取所有活跃Projectile的数量统计
        /// </summary>
        /// <returns>活跃Projectile数量</returns>
        public static int GetActiveProjectileCount()
        {
            return _extendedAIData.Count;
        }

        /// <summary>
        /// 检查指定Projectile是否有扩展AI数据
        /// </summary>
        /// <param name="projectile">Projectile实例</param>
        /// <returns>是否存在扩展数据</returns>
        public static bool HasExtendedAIData(this Projectile projectile)
        {
            return _extendedAIData.ContainsKey(projectile.identity);
        }
    }

    /// <summary>
    /// Projectile AI扩展基类 - 为需要扩展AI的Projectile提供便利方法
    /// </summary>
    public abstract class ExtendedAIProjectile : ModProjectile
    {
        protected ProjectileAIExtensionSystem.ExtendedAIData ExtAI => Projectile.GetExtendedAIData();

        public override void AI()
        {
            // 检查是否需要同步扩展AI数据
            if (ExtAI.IsDirty && Main.netMode != NetmodeID.SinglePlayer&&Projectile.owner == Main.myPlayer)
            {
                Projectile.netUpdate = true;
                ExtAI.IsDirty = false; // 重置脏标记
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            
            // 序列化扩展AI数据
            writer.Write(ExtAI.Data.Count);
            
            foreach (var kvp in ExtAI.Data)
            {
                writer.Write(kvp.Key); // 写入键名
                
                // 根据值类型写入数据 - 简化版本
                if (kvp.Value is float f)
                {
                    writer.Write((byte)ProjectileAIExtensionSystem.DataType.Float);
                    writer.Write(f);
                }
                else if (kvp.Value is int i)
                {
                    writer.Write((byte)ProjectileAIExtensionSystem.DataType.Int);
                    writer.Write(i);
                }
                else if (kvp.Value is bool b)
                {
                    writer.Write((byte)ProjectileAIExtensionSystem.DataType.Bool);
                    writer.Write(b);
                }
                else if (kvp.Value is Vector2 v2)
                {
                    writer.Write((byte)ProjectileAIExtensionSystem.DataType.Vector2);
                    writer.Write(v2.X);
                    writer.Write(v2.Y);
                }
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            
            // 反序列化扩展AI数据
            int count = reader.ReadInt32();
            ExtAI.Data.Clear();
            
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                ProjectileAIExtensionSystem.DataType type = (ProjectileAIExtensionSystem.DataType)reader.ReadByte();
                
                switch (type)
                {
                    case ProjectileAIExtensionSystem.DataType.Float:
                        ExtAI.Data[key] = reader.ReadSingle();
                        break;
                    case ProjectileAIExtensionSystem.DataType.Int:
                        ExtAI.Data[key] = reader.ReadInt32();
                        break;
                    case ProjectileAIExtensionSystem.DataType.Bool:
                        ExtAI.Data[key] = reader.ReadBoolean();
                        break;
                    case ProjectileAIExtensionSystem.DataType.Vector2:
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        ExtAI.Data[key] = new Vector2(x, y);
                        break;
                }
            }
            
            ExtAI.IsDirty = false; // 接收后重置脏标记
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            // 清理扩展AI数据
            Projectile.RemoveExtendedAIData();
        }
    }

    /// <summary>
    /// 实用工具类，提供便捷的数据访问方法
    /// </summary>
    public static class ProjectileAIExtensions
    {
        // Float数据操作
        public static void SetExtAIFloat(this Projectile proj, string key, float value)
        {
            proj.GetExtendedAIData().SetFloat(key, value);
        }

        public static float GetExtAIFloat(this Projectile proj, string key, float defaultValue = 0f)
        {
            return proj.GetExtendedAIData().GetFloat(key, defaultValue);
        }

        // Int数据操作
        public static void SetExtAIInt(this Projectile proj, string key, int value)
        {
            proj.GetExtendedAIData().SetInt(key, value);
        }

        public static int GetExtAIInt(this Projectile proj, string key, int defaultValue = 0)
        {
            return proj.GetExtendedAIData().GetInt(key, defaultValue);
        }

        // Bool数据操作
        public static void SetExtAIBool(this Projectile proj, string key, bool value)
        {
            proj.GetExtendedAIData().SetBool(key, value);
        }

        public static bool GetExtAIBool(this Projectile proj, string key, bool defaultValue = false)
        {
            return proj.GetExtendedAIData().GetBool(key, defaultValue);
        }

        // Short数据操作 - 兼容方法
        public static void SetExtAIShort(this Projectile proj, string key, short value)
        {
            proj.GetExtendedAIData().SetShort(key, value);
        }

        public static short GetExtAIShort(this Projectile proj, string key, short defaultValue = 0)
        {
            return proj.GetExtendedAIData().GetShort(key, defaultValue);
        }

        // Byte数据操作 - 兼容方法
        public static void SetExtAIByte(this Projectile proj, string key, byte value)
        {
            proj.GetExtendedAIData().SetByte(key, value);
        }

        public static byte GetExtAIByte(this Projectile proj, string key, byte defaultValue = 0)
        {
            return proj.GetExtendedAIData().GetByte(key, defaultValue);
        }

        // Vector2数据操作
        public static void SetExtAIVector2(this Projectile proj, string key, Vector2 value)
        {
            proj.GetExtendedAIData().SetVector2(key, value);
        }

        public static Vector2 GetExtAIVector2(this Projectile proj, string key, Vector2 defaultValue = default)
        {
            return proj.GetExtendedAIData().GetVector2(key, defaultValue);
        }

        // 通用检查和清理方法
        public static bool HasExtAIKey(this Projectile proj, string key)
        {
            return proj.GetExtendedAIData().ContainsKey(key);
        }

        public static void RemoveExtAIKey(this Projectile proj, string key)
        {
            proj.GetExtendedAIData().RemoveKey(key);
        }

        public static void ClearExtAIData(this Projectile proj)
        {
            proj.GetExtendedAIData().Clear();
        }
    }
}