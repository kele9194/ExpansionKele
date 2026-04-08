using System;
using System.IO;
using System.Reflection;
using Terraria.ModLoader;
using Terraria;

namespace ExpansionKele.Content.Customs
{
    public static class TexturePathHelper
    {
        /// <summary>
        /// 根据类型获取纹理路径
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>默认的纹理路径（基于命名空间）</returns>
        public static string GetDefaultTexturePath(Type type)
        {
            return type.FullName.Replace('.', '/') + ".png";
        }

        /// <summary>
        /// 根据相对路径获取纹理路径
        /// </summary>
        /// <param name="type">当前类型</param>
        /// <param name="relativePath">相对路径，支持 ./ 和 ../ </param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTexturePath(Type type, string relativePath)
        {
            // 获取类型的完整名称
            string fullName = type.FullName;
            
            // 转换命名空间为路径格式
            string basePath = fullName.Replace('.', '/');
            
            // 移除类名部分，只保留命名空间路径
            int lastSlash = basePath.LastIndexOf('/');
            if (lastSlash > 0)
            {
                basePath = basePath.Substring(0, lastSlash);
            }
            
            // 处理相对路径
            if (relativePath.StartsWith("./"))
            {
                // 当前目录
                string fileName = relativePath.Substring(2);
                return basePath + "/" + fileName;
            }
            else if (relativePath.StartsWith("../"))
            {
                // 父级目录
                string[] parts = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string[] basePathParts = basePath.Split('/');
                
                int levelUp = 0;
                int i = 0;
                while (i < parts.Length && parts[i] == "..")
                {
                    levelUp++;
                    i++;
                }
                
                if (levelUp >= basePathParts.Length)
                {
                    basePath = "ExpansionKele";
                }
                else
                {
                    int newLength = basePathParts.Length - levelUp;
                    basePath = string.Join("/", basePathParts, 0, newLength);
                }
                
                string remainingPath = string.Join("/", parts, i, parts.Length - i);
                if (!string.IsNullOrEmpty(remainingPath))
                {
                    return basePath + "/" + remainingPath;
                }
                else
                {
                    return basePath + "/" + type.Name;
                }
            }
            
            return relativePath;
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
        /// </summary>
        /// <param name="type">当前类型</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(Type type, string defaultTexturePath)
        {
            string expectedTexturePath = GetDefaultTexturePath(type);
            
            if (ModContent.FileExists(expectedTexturePath))
            {
                return expectedTexturePath;
            }
            
            if (!string.IsNullOrEmpty(defaultTexturePath) && ModContent.FileExists(defaultTexturePath))
            {
                return defaultTexturePath;
            }
            
            return expectedTexturePath;
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径
        /// </summary>
        /// <param name="type">当前类型</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(Type type, params string[] fallbackPaths)
        {
            string expectedTexturePath = GetDefaultTexturePath(type);
            
            if (ModContent.FileExists(expectedTexturePath))
            {
                return expectedTexturePath;
            }
            
            foreach (string path in fallbackPaths)
            {
                if (!string.IsNullOrEmpty(path) && ModContent.FileExists(path))
                {
                    return path;
                }
            }
            
            return expectedTexturePath;
        }

        /// <summary>
        /// 获取当前命名空间下的指定纹理路径
        /// </summary>
        /// <param name="type">当前类型</param>
        /// <param name="textureName">纹理名称（不包含扩展名）</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTextureInNamespace(Type type, string textureName)
        {
            string fullName = type.FullName;
            string namespacePath = fullName.Replace('.', '/');
            
            int lastSlash = namespacePath.LastIndexOf('/');
            if (lastSlash > 0)
            {
                namespacePath = namespacePath.Substring(0, lastSlash);
            }
            
            return namespacePath + "/" + textureName;
        }

        /// <summary>
        /// 根据相对路径获取纹理路径（ModItem 版本）
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="relativePath">相对路径，支持 ./ 和 ../ </param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTexturePath(ModItem item, string relativePath)
        {
            return GetTexturePath(item.GetType(), relativePath);
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径（ModItem 版本）
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(ModItem item, string defaultTexturePath)
        {
            return GetTexturePathOrDefault(item.GetType(), defaultTexturePath);
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径（ModItem 版本）
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(ModItem item, params string[] fallbackPaths)
        {
            return GetTexturePathWithFallback(item.GetType(), fallbackPaths);
        }

        /// <summary>
        /// 获取当前命名空间下的指定纹理路径（ModItem 版本）
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="textureName">纹理名称（不包含扩展名）</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTextureInNamespace(ModItem item, string textureName)
        {
            return GetTextureInNamespace(item.GetType(), textureName);
        }
    }
    
    // Deleted:// 为 ModItem 添加扩展方法
    // Deleted:public static class ModItemExtensions
    // Deleted:{
    // Deleted:    /// <summary>
    // Deleted:    /// 获取基于相对路径的纹理路径
    // Deleted:    /// </summary>
    // Deleted:    /// <param name="item">当前物品实例</param>
    // Deleted:    /// <param name="relativePath">相对路径</param>
    // Deleted:    /// <returns>完整的纹理路径</returns>
    // Deleted:    public static string GetRelativeTexturePath(this ModItem item, string relativePath)
    // Deleted:    {
    // Deleted:        return TexturePathHelper.GetTexturePath(item, relativePath);
    // Deleted:    }
    // Deleted:
    // Deleted:    /// <summary>
    // Deleted:    /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
    // Deleted:    /// </summary>
    // Deleted:    /// <param name="item">当前物品实例</param>
    // Deleted:    /// <param name="defaultTexturePath">默认纹理路径</param>
    // Deleted:    /// <returns>存在的纹理路径</returns>
    // Deleted:    public static string GetTexturePathOrDefault(this ModItem item, string defaultTexturePath)
    // Deleted:    {
    // Deleted:        return TexturePathHelper.GetTexturePathOrDefault(item, defaultTexturePath);
    // Deleted:    }
    // Deleted:
    // Deleted:    /// <summary>
    // Deleted:    /// 获取纹理路径，支持多个备选路径
    // Deleted:    /// </summary>
    // Deleted:    /// <param name="item">当前物品实例</param>
    // Deleted:    /// <param name="fallbackPaths">备选纹理路径数组</param>
    // Deleted:    /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
    // Deleted:    public static string GetTexturePathWithFallback(this ModItem item, params string[] fallbackPaths)
    // Deleted:    {
    // Deleted:        return TexturePathHelper.GetTexturePathWithFallback(item, fallbackPaths);
    // Deleted:    }
    // Deleted:     /// <summary>
    // Deleted:    /// 获取当前命名空间下的指定纹理路径
    // Deleted:    /// </summary>
    // Deleted:    /// <param name="item">当前物品实例</param>
    // Deleted:    /// <param name="textureName">纹理名称（不包含扩展名）</param>
    // Deleted:    /// <returns>完整的纹理路径</returns>
    // Deleted:    public static string GetTextureInNamespace(this ModItem item, string textureName)
    // Deleted:    {
    // Deleted:        return TexturePathHelper.GetTextureInNamespace(item, textureName);
    // Deleted:    }
    // Deleted:}
    
    /// <summary>
    /// 为任意类型提供纹理路径扩展方法
    /// </summary>
    public static class TypeTextureExtensions
    {
        /// <summary>
        /// 获取基于相对路径的纹理路径
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="relativePath">相对路径</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetRelativeTexturePath(this Type type, string relativePath)
        {
            return TexturePathHelper.GetTexturePath(type, relativePath);
        }

        /// <summary>
        /// 获取默认纹理路径（基于命名空间）
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>默认纹理路径</returns>
        public static string GetDefaultTexturePath(this Type type)
        {
            return TexturePathHelper.GetDefaultTexturePath(type);
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(this Type type, string defaultTexturePath)
        {
            return TexturePathHelper.GetTexturePathOrDefault(type, defaultTexturePath);
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(this Type type, params string[] fallbackPaths)
        {
            return TexturePathHelper.GetTexturePathWithFallback(type, fallbackPaths);
        }

        /// <summary>
        /// 获取当前命名空间下的指定纹理路径
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="textureName">纹理名称（不包含扩展名）</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTextureInNamespace(this Type type, string textureName)
        {
            return TexturePathHelper.GetTextureInNamespace(type, textureName);
        }
    }
    
    /// <summary>
    /// 为 object 提供运行时纹理路径扩展方法
    /// </summary>
    public static class ObjectTextureExtensions
    {
        /// <summary>
        /// 获取基于相对路径的纹理路径
        /// </summary>
        /// <param name="obj">任意对象实例</param>
        /// <param name="relativePath">相对路径</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetRelativeTexturePath(this object obj, string relativePath)
        {
            return TexturePathHelper.GetTexturePath(obj.GetType(), relativePath);
        }

        /// <summary>
        /// 获取默认纹理路径（基于命名空间）
        /// </summary>
        /// <param name="obj">任意对象实例</param>
        /// <returns>默认纹理路径</returns>
        public static string GetDefaultTexturePath(this object obj)
        {
            return TexturePathHelper.GetDefaultTexturePath(obj.GetType());
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
        /// </summary>
        /// <param name="obj">任意对象实例</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(this object obj, string defaultTexturePath)
        {
            return TexturePathHelper.GetTexturePathOrDefault(obj.GetType(), defaultTexturePath);
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径
        /// </summary>
        /// <param name="obj">任意对象实例</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(this object obj, params string[] fallbackPaths)
        {
            return TexturePathHelper.GetTexturePathWithFallback(obj.GetType(), fallbackPaths);
        }

        /// <summary>
        /// 获取当前命名空间下的指定纹理路径
        /// </summary>
        /// <param name="obj">任意对象实例</param>
        /// <param name="textureName">纹理名称（不包含扩展名）</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTextureInNamespace(this object obj, string textureName)
        {
            return TexturePathHelper.GetTextureInNamespace(obj.GetType(), textureName);
        }
    }
    
    /// <summary>
    /// 保留 ModItem 扩展方法以维持向后兼容性
    /// </summary>
    public static class ModItemExtensions
    {
        /// <summary>
        /// 获取基于相对路径的纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="relativePath">相对路径</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetRelativeTexturePath(this ModItem item, string relativePath)
        {
            return TexturePathHelper.GetTexturePath(item, relativePath);
        }

        /// <summary>
        /// 获取纹理路径，如果指定的纹理不存在，则返回默认纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="defaultTexturePath">默认纹理路径</param>
        /// <returns>存在的纹理路径</returns>
        public static string GetTexturePathOrDefault(this ModItem item, string defaultTexturePath)
        {
            return TexturePathHelper.GetTexturePathOrDefault(item, defaultTexturePath);
        }

        /// <summary>
        /// 获取纹理路径，支持多个备选路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="fallbackPaths">备选纹理路径数组</param>
        /// <returns>第一个存在的纹理路径，如果都不存在则返回默认路径</returns>
        public static string GetTexturePathWithFallback(this ModItem item, params string[] fallbackPaths)
        {
            return TexturePathHelper.GetTexturePathWithFallback(item, fallbackPaths);
        }

        /// <summary>
        /// 获取当前命名空间下的指定纹理路径
        /// </summary>
        /// <param name="item">当前物品实例</param>
        /// <param name="textureName">纹理名称（不包含扩展名）</param>
        /// <returns>完整的纹理路径</returns>
        public static string GetTextureInNamespace(this ModItem item, string textureName)
        {
            return TexturePathHelper.GetTextureInNamespace(item, textureName);
        }
    }
}